namespace GeneSort.Dispatch.V1.SorterSgd

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Project.V1.Parameters
open GeneSort.SortingOps

/// Apply branch settings once when loading imported Setup, never when resuming Results.
module EvoAppendPools =
    let private branch (rp: runParameters) (originals: 'pool array) bounds getModifier derive retag =
        if not (Map.containsKey ParamKeys.sourcePoolFileIdKey rp.ParamMap) then Ok None
        else
            rp.GetMutationMod() |> Result.ofOption "Missing MutationMod."
            |> Result.bind (fun modifier ->
                let targetCount =
                    match rp.GetSorterCountPerPoolSet(), rp.GetSorterCountPerPool() with
                    | Some total, Some perPool when int total > 0 && int perPool > 0 && int total % int perPool = 0 ->
                        if originals.Length = 0 then Error "Cannot resize an empty imported pool set."
                        else Ok (int total / int perPool)
                    | _ -> Error "SorterCountPerPoolSet must be positive and divisible by the positive SorterCountPerPool."
                targetCount |> Result.bind (fun count ->
                    let resized = count <> originals.Length
                    let newBounds =
                        if not resized then Ok bounds
                        else
                            match bounds with
                            | Dim1 _ -> Ok (Dim1 count)
                            | _ -> Error "Append resizing currently supports one-dimensional pool lattices."
                    newBounds |> Result.map (fun newBounds ->
                        if not resized && (originals |> Array.forall (fun item -> getModifier item = modifier)) then None
                        else
                            // Map order is stable: shrink by pool ID, grow by cycling over all source pools.
                            let pools = Array.init count (fun index ->
                                let item = originals.[index % originals.Length]
                                let branched =
                                    if index < originals.Length && getModifier item = modifier then item
                                    else derive (UMX.tag (Guid.NewGuid())) modifier item
                                if resized then retag (D1 index) branched else branched)
                            Some (pools, newBounds))))

    let prepareStandard rp (pool: sorterPoolSet_Standard) =
        branch rp (pool.SorterPools |> Map.values |> Seq.toArray) pool.LatticeBounds
            (fun item -> item.MutationMod) SorterPool_Standard.deriveChildPool
            (fun tag item -> sorterPool_Standard.create item.SorterPoolId item.ParentSorterPoolId item.Name tag
                                (item.SorterPoolMembers |> Seq.toArray) item.RawCeLength item.MutationMod)
        |> Result.map (function
            | None -> pool
            | Some (pools, bounds) -> sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) pool.GenerationNumber bounds (Some pools))

    let prepareSoss rp (pool: sorterPoolSet_Soss) =
        branch rp (pool.SorterPools |> Map.values |> Seq.toArray) pool.LatticeBounds
            (fun item -> item.MutationMod) SorterPool_Soss.deriveChildPool
            (fun tag item -> sorterPool_Soss.create item.SorterPoolId item.ParentSorterPoolId item.Name tag
                                (item.SorterPoolMembers |> Seq.toArray) item.RawCeLength item.MutationMod)
        |> Result.map (function
            | None -> pool
            | Some (pools, bounds) -> sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) pool.GenerationNumber bounds (Some pools))
