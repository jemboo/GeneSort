namespace GeneSort.Project.V1.Catalogs.Evolution

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Parameters

/// Convert checkpoints during Setup and apply branch settings when loading imported pools.


module EvoAppendPools =
    /// Convert during Setup; Standard selection must never rank partial Soss evaluations.
    let convertForSetup executionFunction tests prefix (source: sorterPoolSet) =
        match executionFunction, source with
        | ExecuteFunctionNames.soss, sorterPoolSet.Standard standard ->
            SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss
        | ExecuteFunctionNames.standard, sorterPoolSet.Soss soss ->
            let pools = soss.SorterPools |> Map.values |> Seq.map (fun pool ->
                let members = pool.SorterPoolMembers |> Seq.map (fun m ->
                    spMember_Standard.create m.SorterPoolMemberId m.SorterModel m.MutationIndex m.MutationMod
                        m.SorterMutationSource None m.Birthday) |> Seq.toArray
                sorterPool_Standard.create pool.SorterPoolId pool.ParentSorterPoolId pool.Name pool.SorterPoolTag
                    members pool.RawCeLength pool.MutationMod)
            let converted = sorterPoolSet_Standard.create soss.SorterPoolSetId soss.GenerationNumber soss.LatticeBounds (Some pools)
            let evaluations = PoolRunner_Standard.evaluatePoolSet tests prefix sorterEvalType.V2 true (UMX.tag false) converted
            converted |> SorterPoolSet_Standard.updateSorterEvals evaluations |> sorterPoolSet.Standard
        | _ -> source

    let private branch (rp: runParameters) (originals: 'pool array) bounds getModifier derive retag =
        if not (Map.containsKey ParamKeys.sourceExecutionIdKey rp.ParamMap) then Ok None
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
