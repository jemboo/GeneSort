namespace GeneSort.Eval.V1.Sgd.Standard

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1

module PoolRunner_Standard =

    /// Evaluates members of a sorterPoolSet against a sortableTests suite.
    /// Skips already evaluated members if reEvaluateParents is false.
    let evaluatePoolSet 
            (sortableTests: sortableTests)
            (prefix: ceBlock)
            (sorterEvalType: sorterEvalType)
            (reEvaluateParents: bool)
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (poolSet: sorterPoolSet_Standard)
            : Map<Guid<sorterPoolMemberId>, sorterEval> =

        // 1. Collect all members across all pools paired with their respective pool's ceLength
        
        let allMembers = 
            poolSet.SorterPools
            |> Map.values
            |> Seq.collect (fun pool -> 
                let paddedCeLength = ((float pool.RawCeLength) * 1.1) |> int |> UMX.tag<ceLength>
                pool.SorterPoolMembers 
                |> Seq.map (fun m -> paddedCeLength, m)
            )
            |> Seq.toArray

        if allMembers.Length = 0 then 
            Map.empty
        else
            // 2. Separate members that need evaluation from those that can be skipped
            let membersToEvaluate, cachedEvaluations =
                if reEvaluateParents then
                    allMembers, Map.empty
                else
                    let toEval = allMembers |> Array.filter (fun (_, m) -> m.SorterEval.IsNone)
                    let cached = 
                        allMembers 
                        |> Seq.choose (fun (_, m) -> m.SorterEval |> Option.map (fun eval -> m.SorterPoolMemberId, eval))
                        |> Map.ofSeq
                    toEval, cached

            // If everything is already cached and no new evaluations are needed, exit early
            if membersToEvaluate.Length = 0 then
                cachedEvaluations
            else
                // 4. Materialize the filtered models into actual sorter executable structures
                //    We wrap the pool's ceLength into an option block to match the original signature requirement
                let sorters = 
                    membersToEvaluate 
                    |> Array.map (fun (ceLength, m) -> SorterModel.makeSorter m.SorterModel (Some ceLength))

                // 5. Run the evaluation engine on the filtered pool subset
                let (rawEvaluations : sorterEval array) =
                    SorterSetEval.makeSorterEvals 
                        sorters 
                        prefix
                        sortableTests 
                        sorterEvalType 
                        collectNewSortableTests

                // Debugger check for short CE lengths (< 120)
                if rawEvaluations |> Array.exists (fun eval -> %(SorterEval.getCeLength eval) < 120) then
                    let evalWithShortCe = rawEvaluations |> Array.find (fun eval -> %(SorterEval.getCeLength eval) < 120)
                    System.Diagnostics.Debugger.Break()

                // Evaluation preserves input order. Copied members can share a model ID,
                // and each pool can evaluate that model at a different CE length.
                Array.zip membersToEvaluate rawEvaluations
                |> Array.fold (fun accMap ((_, memberValue), eval) ->
                    Map.add memberValue.SorterPoolMemberId eval accMap
                ) cachedEvaluations
