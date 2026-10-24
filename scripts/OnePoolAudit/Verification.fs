module OnePoolAuditVerification

open GeneSort.Project.V1.Parameters

let private require condition message = if not condition then failwith message

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
