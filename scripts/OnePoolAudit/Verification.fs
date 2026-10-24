module OnePoolAuditVerification

open GeneSort.Project.V1.Parameters
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.Model.Sorting.Mp.V1.Simple

let private require condition message = if not condition then failwith message

let verifyControlledSeeds (simple: sorterModel[]) (bounded: sorterModel[]) (boolean: sorterModel[]) =
    let index models =
        let indexed = models |> Array.map (fun model -> SorterModel.getId model, model) |> Map.ofArray
        require (indexed.Count = models.Length && models.Length > 0) "Controlled seeds contain duplicate IDs or no models."
        indexed
    let plain, rs, ss = index simple, index bounded, index boolean
    require (Set.ofSeq plain.Keys = Set.ofSeq rs.Keys && Set.ofSeq plain.Keys = Set.ofSeq ss.Keys)
        "Controlled seed model IDs differ."
    for KeyValue(id, source) in plain do
        let original = SorterModel.asSimpleSorterModel source
        let r = SorterModel.asSimpleSorterModelR rs.[id]
        let s = SorterModel.asSimpleSorterModelS ss.[id]
        let snapshot = SimpleSorterModelDto.fromDomain
        require (snapshot original = snapshot r.Model && snapshot original = snapshot s.Model) "Controlled underlying seed arrays differ."
        require ((r.Activators |> Array.map (fun value -> value > 0)) = s.Activators)
            "Controlled initial activation masks differ."

let verifyActivatorSettings path (parameters: Map<string, string>) (rows: Map<string, string>[]) =
    for key in [ParamKeys.aMaxKey; ParamKeys.activationProbabilityKey; ParamKeys.activatorMutationRateKey; ParamKeys.controlledSeedAMaxKey] do
        let expected = Map.tryFind key parameters |> Option.defaultValue ""
        require (rows |> Array.forall (fun row -> Map.tryFind key row |> Option.defaultValue "" = expected))
            (sprintf "Summary report contains an incorrect %s (expected '%s'): %s" key expected path)

let verifyModelActivators maximum (models: sorterModel seq) =
    for model in models do
        match maximum, model with
        | Some expected, sorterModel.SimpleR paired ->
            require (paired.AMax = expected) "SimpleR model AMax differs from the configured value."
            require (paired.Activators.Length = SimpleSorterModel.getLength paired.Model)
                "SimpleR activator count differs from the underlying model length."
            require (paired.Activators |> Array.forall (fun value -> value >= -expected && value <= expected))
                "SimpleR activator is outside the configured bounds."
        | None, sorterModel.SimpleR _ -> failwith "Unexpected SimpleR model without configured AMax."
        | None, sorterModel.SimpleS _ -> failwith "Unexpected SimpleS model without configured ActivationProbability."
        | Some _, _ -> failwith "Expected a SimpleR model for configured AMax."
        | None, _ -> ()

let verifyModelActivationParameters maximum probability models =
    match probability with
    | None -> verifyModelActivators maximum models
    | Some value ->
        require (maximum.IsNone && System.Double.IsFinite value && value >= 0.0 && value <= 1.0)
            "SimpleS requires valid ActivationProbability and no AMax."
        for model in models do
            match model with
            | sorterModel.SimpleS paired ->
                require (paired.Activators.Length = SimpleSorterModel.getLength paired.Model)
                    "SimpleS activator count differs from the underlying model length."
            | _ -> failwith "Expected a SimpleS model for configured ActivationProbability."

let verifyMutations path mutation sourceMutation (rows: Map<string, string>[]) =
    for key, expected in
        [ ParamKeys.mutationModKey, mutation
          ParamKeys.sourceMutationModKey, sourceMutation |> Option.defaultValue "" ] do
        require (rows |> Array.forall (fun row -> Map.tryFind key row |> Option.defaultValue "" = expected))
            (sprintf "Summary report contains an incorrect %s (expected '%s'): %s" key expected path)

let verifyExpansion runName spans sources actual =
    // Expand independently of the production parameter generator.
    let combinations =
        spans |> List.fold (fun combinations (key, values) ->
            [ for combination in combinations do
                for value in values do yield Map.add key value combination ]) [Map.empty]
    let expected =
        [| for sourceId, sourceMutation in sources do
               for combination in combinations do
                   yield sourceId, sourceMutation,
                         Map.tryFind ParamKeys.mutationModKey combination |> Option.defaultValue sourceMutation,
                         combination |]
    require (Array.sort expected = Array.sort actual)
        (sprintf "%s: source mutations or execution expansion differ from configured spans." runName)
    combinations.Length, expected.Length

// Compare multisets so missing/duplicate rows cannot hide behind identical mutation values.
let verifyCombinedRows headers (expected: Map<string, string>[]) (lines: string[]) =
    require (lines.Length > 1 && lines.[0].Split '\t' = headers)
        "Combined summary headers are incorrect."
    let expectedRows = expected |> Array.map (fun row ->
        headers |> Array.map (fun key -> Map.tryFind key row |> Option.defaultValue "") |> String.concat "\t")
    require (Array.sort expectedRows = Array.sort lines.[1..])
        "Combined summary differs from verified individual reports (missing, duplicate, or changed rows, including mutation values and execution expansion)."
