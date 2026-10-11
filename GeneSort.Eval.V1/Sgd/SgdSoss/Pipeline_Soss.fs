namespace GeneSort.Eval.V1.Sgd.Soss

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open System.Diagnostics

module Pipeline_Soss =

    /// Soss generation step with per-partition and merged evaluations and debug breakpoints
    let runGenerationStepSoss
            (mutator: sorterModelMutator)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (selectedSorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterChildCount>)
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sortableTestPartitions: partitionOfSortableTests)
            (prefix: ceBlock)
            (sorterEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (currentPoolSet: sorterPoolSet_Soss)
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (sortedFractionThreshold: float<sortedFraction>) : sorterPoolSet_Soss =

        // Helper to check if any pool in a poolSet has dropped to 0 members
        let hasEmptyPool (poolSet: sorterPoolSet_Soss) =
            poolSet.SorterPools
            |> Map.exists (fun _ pool -> Seq.isEmpty pool.SorterPoolMembers)

        let subsetIdForSelection =
            sortableTestPartitions.SortableTests
            |> Map.keys
            |> Seq.toArray
            |> CommonSgdSoss.subsetIdForGeneration currentPoolSet.GenerationNumber

        // Members retain the selection subset used by the preceding generation step.
        let reEvaluateParents =
            currentPoolSet.SorterPools
            |> Map.values
            |> Seq.collect (fun pool -> pool.SorterPoolMembers)
            |> Seq.exists (fun memberValue -> memberValue.SubsetIdForSelection <> subsetIdForSelection)

        let poolSetForSelection =
            currentPoolSet
            |> SorterPoolSet_Soss.withSubsetIdForSelection subsetIdForSelection

        let selectionPartitionId = UMX.cast<sortableTestsSubsetId, sortableTestsPartitionId> subsetIdForSelection
        if subsetIdForSelection <> CommonSgdSoss.mergedSubsetId
           && not (Map.containsKey selectionPartitionId sortableTestPartitions.SortableTests) then
            invalidArg "subsetIdForSelection" (sprintf "Unknown selection subset '%s'." %subsetIdForSelection)

        let evaluatePartitions reEvaluate poolSet =
            sortableTestPartitions.SortableTests
            |> Map.map (fun partitionId tests ->
                let subsetId = UMX.cast<sortableTestsPartitionId, sortableTestsSubsetId> partitionId
                PoolRunner_Soss.evaluatePoolSet tests prefix sorterEvalType reEvaluate
                    collectNewSortableTests
                    (poolSet |> SorterPoolSet_Soss.withSubsetIdForSelection subsetId)
                |> Map.map (fun _ eval -> SorterEval.withSortableTestsSubsetId subsetId eval))

        let storeEvaluations partitionEvals poolSet =
            partitionEvals
            |> Map.fold (fun updated _ evaluations ->
                SorterPoolSet_Soss.updateSorterEvals evaluations updated) poolSet
            |> SorterPoolSet_Soss.updateSorterEvals (CommonSgdSoss.mergePartitionEvals partitionEvals)

        // Seed a newly selected partition before ranking parents, including on the first step.
        let preparedPoolSet =
            let hasMissingSelectionEval =
                poolSetForSelection.SorterPools
                |> Map.values
                |> Seq.collect (fun pool -> pool.SorterPoolMembers)
                |> Seq.exists (fun memberValue -> memberValue.SorterEvalForSelection.IsNone)
            if hasMissingSelectionEval then
                match Map.tryFind selectionPartitionId sortableTestPartitions.SortableTests with
                | Some tests ->
                    let evaluations =
                        PoolRunner_Soss.evaluatePoolSet tests prefix sorterEvalType false
                            collectNewSortableTests poolSetForSelection
                        |> Map.map (fun _ eval -> SorterEval.withSortableTestsSubsetId subsetIdForSelection eval)
                    SorterPoolSet_Soss.updateSorterEvals evaluations poolSetForSelection
                | None when subsetIdForSelection = CommonSgdSoss.mergedSubsetId ->
                    storeEvaluations (evaluatePartitions false poolSetForSelection) poolSetForSelection
                | None -> invalidArg "subsetIdForSelection" (sprintf "Unknown selection subset '%s'." %subsetIdForSelection)
            else
                poolSetForSelection

        // --- Step 1a: Mutate / Expand Population ---
        let mutatedPoolSet = SorterPoolSet_Soss.mutateAndTrim
                                    mutator
                                    selectedSorterCountPerPool
                                    selectionMeasure
                                    sorterChildCount
                                    preparedPoolSet

        if hasEmptyPool mutatedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if mutation resulted in an empty pool

        // --- Step 1b: Evaluate Pool Set ---

        let partitionEvals = evaluatePartitions reEvaluateParents mutatedPoolSet
        let evaluatedPoolSet = storeEvaluations partitionEvals mutatedPoolSet

        if hasEmptyPool evaluatedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if evaluation or eval update failed

        // --- Step 2: Adjust Constraint Boundaries ---
        let adjustedPoolSet =
            if reEvaluateParents then
                SorterPoolSet_Soss.adjustCeLengths sortedFractionThreshold evaluatedPoolSet
            else
                evaluatedPoolSet

        if hasEmptyPool adjustedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if length adjustment emptied a pool

        // --- Step 3: Prune Sorter Pools ---
        let prunedPoolSet =
            SorterPoolSet_Soss.pruneSorterPools
                selectionMeasure
                prioritizeNewMutants
                distinctSorterHashes
                sorterCountPerPool
                adjustedPoolSet

        if hasEmptyPool prunedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if pruning reduced a pool to zero members

        // --- Step 4: Advance Generation Counter ---
        let finalPoolSet = SorterPoolSet_Soss.advanceGeneration 1 prunedPoolSet

        finalPoolSet

