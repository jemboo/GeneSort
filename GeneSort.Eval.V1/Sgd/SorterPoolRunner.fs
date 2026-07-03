namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1

module SorterPoolRunner =

    /// Evaluates members of a sorterPoolSet against a sortableTest suite.
    /// Skips already evaluated members if reEvaluateParents is false.
    let evaluatePoolSet 
            (sortableTest: sortableTest)
            (sorterEvalType: sorterEvalType)
            (reEvaluateParents: bool)
            (poolSet: sorterPoolSet)
            : Map<Guid<sorterPoolMemberId>, sorterEval> =

        // 1. Collect all members across all pools
        let allMembers = 
            poolSet.SorterPools
            |> Map.values
            |> Seq.collect (fun pool -> pool.SorterPoolMembers)
            |> Seq.toArray

        if allMembers.Length = 0 then 
            Map.empty
        else
            // 2. Separate members that need evaluation from those that can be skipped
            let membersToEvaluate, cachedEvaluations =
                if reEvaluateParents then
                    allMembers, Map.empty
                else
                    let toEval = allMembers |> Array.filter (fun m -> m.SorterEval.IsNone)
                    let cached = 
                        allMembers 
                        |> Seq.choose (fun m -> m.SorterEval |> Option.map (fun eval -> m.SorterPoolMemberId, eval))
                        |> Map.ofSeq
                    toEval, cached

            // If everything is already cached and no new evaluations are needed, exit early
            if membersToEvaluate.Length = 0 then
                cachedEvaluations
            else
                // 3. Compile an ID mapping between sorterId and poolMemberId for the targets being evaluated
                let idMapping = 
                    membersToEvaluate 
                    |> Array.map (fun m -> 
                        let modelId = SorterModel.getId m.SorterModel
                        let sorterId = %modelId |> UMX.tag<sorterId>
                        sorterId, m.SorterPoolMemberId
                    ) 
                    |> Map.ofArray

                // 4. Materialize the filtered models into actual sorter executable structures
                let sorters = 
                    membersToEvaluate 
                    |> Array.map (fun m -> SorterModel.makeSorter m.SorterModel)

                // 5. Run the evaluation engine on the filtered pool subset
                let collectNewSortableTests = false
                let rawEvaluations =
                    SorterSetEval.makeSorterEvals 
                        sorters 
                        sortableTest 
                        sorterEvalType 
                        collectNewSortableTests

                // 6. Map raw evaluations back to poolMemberId and merge them with any cached evaluations
                rawEvaluations
                |> Array.fold (fun accMap eval ->
                    let sorterId = SorterEval.getSorterId eval
                    match Map.tryFind sorterId idMapping with
                    | Some poolMemberId -> Map.add poolMemberId eval accMap
                    | None -> accMap // Defensive bypass
                ) cachedEvaluations