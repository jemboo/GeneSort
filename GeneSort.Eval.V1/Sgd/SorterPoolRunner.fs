namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1

module SorterPoolRunner =

    /// Evaluates all members of a sorterPoolSet against a sortableTest suite.
    /// Safely handles the conversion mapping between pool member tracking IDs and sorter processing IDs.
    let evaluatePoolSet 
            (sortableTest: sortableTest)
            (sorterEvalType: sorterEvalType)
            (collectNewSortableTests: bool)
            (poolSet: sorterPoolSet) : Map<Guid<sorterPoolMemberId>, sorterEval> =

        // 1. Collect all unique members across all pools in the set
        let allMembers = 
            poolSet.SorterPools
            |> Map.values
            |> Seq.collect (fun pool -> pool.SorterPoolMembers)
            |> Seq.toArray

        if allMembers.Length = 0 then 
            Map.empty
        else
            // 2. Compile an explicit, safe ID mapping dictionary between sorterId and poolMemberId.
            // Note: sorterId is derived from the sorterModelId via %modelId |> UMX.tag
            let idMapping = 
                allMembers 
                |> Array.map (fun m -> 
                    let modelId = SorterModel.getId m.SorterModel
                    let sorterId = %modelId |> UMX.tag<sorterId>
                    sorterId, m.SorterPoolMemberId
                ) 
                |> Map.ofArray

            // 3. Materialize the abstract models into actual sorter executable structures
            let sorters = 
                allMembers 
                |> Array.map (fun m -> SorterModel.makeSorter m.SorterModel)

            // 4. Fire the evaluation engine via the SorterSetEval core module routines
            let rawEvaluations = 
                SorterSetEval.makeSorterEvals 
                    sorters 
                    sortableTest 
                    sorterEvalType 
                    collectNewSortableTests

            // 5. Map the results back from sorterId keys to sorterPoolMemberId keys
            rawEvaluations
            |> Array.fold (fun accMap eval ->
                let sorterId = SorterEval.getSorterId eval
                match Map.tryFind sorterId idMapping with
                | Some poolMemberId -> Map.add poolMemberId eval accMap
                | None -> accMap // Defensive bypass
            ) Map.empty