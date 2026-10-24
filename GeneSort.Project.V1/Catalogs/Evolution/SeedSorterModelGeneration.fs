namespace GeneSort.Project.V1.Catalogs.Evolution

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.Project.V1.Parameters

module SeedSorterModelGeneration =

    /// AMax opts into SimpleR while retaining the underlying simple model type and source catalog.
    let applyParameters (rp: runParameters) (generator: sorterModelGen) =
        if Map.containsKey ParamKeys.activationProbabilityKey rp.ParamMap then
            if Map.containsKey ParamKeys.aMaxKey rp.ParamMap then Error "ActivationProbability and AMax cannot be combined."
            else
                match rp.GetActivationProbability(), generator with
                | Some probability, sorterModelGen.Simple underlying
                    when System.Double.IsFinite probability && probability >= 0.0 && probability <= 1.0 ->
                    simpleSorterModelSGen.create underlying probability |> sorterModelGen.SimpleS |> Ok
                | _ -> Error "SimpleS requires an underlying Simple generator and a finite ActivationProbability between 0 and 1."
        elif not (Map.containsKey ParamKeys.aMaxKey rp.ParamMap) then Ok generator
        else
            match rp.GetAMax(), generator with
            | Some maximum, sorterModelGen.Simple underlying when maximum >= 0 ->
                simpleSorterModelRGen.create underlying maximum |> sorterModelGen.SimpleR |> Ok
            | Some maximum, _ when maximum >= 0 -> Error "SimpleR generation requires an underlying Simple generator."
            | _ -> Error "AMax must be a nonnegative integer."

    let getStageLength 
                (smt: simpleSorterModelType) 
                (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 4 -> 15
        | 5 -> 25
        | 6 -> 40 
        | 7 -> 50 
        | 8 -> 60
        | 9 -> 70
        | 10 -> 80
        | 11 -> 90
        | 12 -> 100
        | 14 -> 120
        | 16 -> match smt with | simpleSorterModelType.Msuf4 -> 300 | _ -> 150
        | 18 -> 180
        | 20 -> 200
        | 22 -> 250
        | 24 -> 300
        | 28 -> 300
        | 32 -> match smt with | simpleSorterModelType.Msuf4 -> 600 | _ -> 300
        | 36 -> 350
        | 48 -> 400
        | 64 -> match smt with | simpleSorterModelType.Msuf4 -> 2000 | _ -> 600
        | 96 -> 800
        | 128 -> match smt with | simpleSorterModelType.Msuf4 -> 4000 | _ -> 1200
        | 192 -> 2000
        | 256 -> match smt with | simpleSorterModelType.Msuf4 -> 6000 | _ -> 3000
        | 512 -> match smt with | simpleSorterModelType.Msuf4 -> 8000 | _ -> 8000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag


    let getSimpleUniformSorterModelGen
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>)
            (simpleSorterModelType: simpleSorterModelType) 
            (excludeSelfCe: bool<excludeSelfCe>)
                            : sorterModelGen =
            let stageLength = getStageLength simpleSorterModelType sortingWidth
            let rngFactory = rngType |> RngFactory.create
            SimpleSorterModelGen.makeUniform 
                                    rngFactory 
                                    sortingWidth 
                                    stageLength 
                                    simpleSorterModelType
                                    excludeSelfCe
                                    |> sorterModelGen.Simple

    /// Paired experimental seeds: the replica and index determine the underlying model,
    /// independently of workflow, mutation variant, and activator representation.
    let makeControlledModelSet (rp: runParameters) =
        try
            let maximum = ParamKeys.tryGetInt ParamKeys.controlledSeedAMaxKey rp.ParamMap |> Option.defaultWith (fun () -> invalidArg "ControlledSeedAMax" "A positive integer is required.")
            if maximum <= 0 then invalidArg "ControlledSeedAMax" "A positive integer is required."
            let probability = float maximum / (2.0 * float maximum + 1.0)
            match rp.GetAMax(), rp.GetActivationProbability() with
            | Some m, None when m = maximum -> ()
            | None, Some p when abs (p - probability) < 1e-12 -> ()
            | None, None -> ()
            | _ -> invalidArg "ControlledSeedAMax" "AMax must match the controlled bound, or ActivationProbability must equal AMax/(2*AMax+1)."
            if rp.GetSorterPoolCount() <> Some (UMX.tag 1) then invalidArg "SorterPoolCount" "Controlled comparisons require one pool."
            let generator =
                getSimpleUniformSorterModelGen (rp.GetRngType().Value) (rp.GetSortingWidth().Value)
                    (rp.GetSimpleSorterModelType().Value) (rp.GetExcludeSelfCe().Value)
            let underlying = match generator with sorterModelGen.Simple g -> g | _ -> failwith "Expected Simple generator."
            let paired = simpleSorterModelRGen.create underlying maximum
            let models = Array.init (int (rp.GetSorterCountPerPool().Value)) (fun index ->
                let id = [box "OnePoolControlledSeeds-v1"; box (SimpleSorterModelGen.getId underlying); box (int (rp.GetRepl().Value)); box index]
                         |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>
                let r = paired.MakeSorterModelFromId id
                match rp.GetAMax(), rp.GetActivationProbability() with
                | Some _, _ -> sorterModel.SimpleR r
                | _, Some _ -> simpleSorterModelS.create r.Model (r.Activators |> Array.map (fun a -> a > 0)) |> sorterModel.SimpleS
                | _ -> sorterModel.Simple r.Model)
            sorterModelSet.create (UMX.tag Guid.Empty) models (SorterModelGen.getCeLength generator) |> Ok
        with error -> Error (sprintf "Controlled seed generation: %s" error.Message)






