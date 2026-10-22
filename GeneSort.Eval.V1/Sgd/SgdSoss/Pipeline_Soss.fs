namespace GeneSort.Eval.V1.Sgd.Soss

open System.Diagnostics
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1

module Pipeline_Soss =

    /// Evaluates the selected partition, or all partitions when Merge or reporting needs them.
    let evaluateRequiredPartitions
            (sortableTestPartitions: partitionOfSortableTests)
            (prefix: ceBlock)
            (sorterEvalType: sorterEvalType)
            (reEvaluateParents: bool)
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (subsetIdForSelection: string<sortableTestsSubsetId>)
            (evaluateAllPartitionsForReport: bool)
            (poolSet: sorterPoolSet_Soss) =
        let evaluateAll = evaluateAllPartitionsForReport || subsetIdForSelection = CommonSgdSoss.mergedSubsetId
        let selectedPartitionId = UMX.cast<sortableTestsSubsetId, sortableTestsPartitionId> subsetIdForSelection
        let requiredPartitions =
            sortableTestPartitions.SortableTests
            |> Map.filter (fun partitionId _ -> evaluateAll || partitionId = selectedPartitionId)
        if Map.isEmpty requiredPartitions then
            invalidArg "subsetIdForSelection" (sprintf "Unknown selection subset '%s'." %subsetIdForSelection)
        let partitionEvals =
            requiredPartitions
            |> Map.map (fun partitionId tests ->
                let subsetId = UMX.cast<sortableTestsPartitionId, sortableTestsSubsetId> partitionId
                PoolRunner_Soss.evaluatePoolSet tests prefix sorterEvalType reEvaluateParents
                    collectNewSortableTests
                    (poolSet |> SorterPoolSet_Soss.withSubsetIdForSelection subsetId)
                |> Map.map (fun _ eval -> SorterEval.withSortableTestsSubsetId subsetId eval))
        let evaluatedPoolSet =
            partitionEvals
            |> Map.fold (fun updated _ evaluations ->
                SorterPoolSet_Soss.updateSorterEvals evaluations updated) poolSet
        if evaluateAll && (sortableTestPartitions.Count > 1 || subsetIdForSelection = CommonSgdSoss.mergedSubsetId) then
            evaluatedPoolSet
            |> SorterPoolSet_Soss.updateSorterEvals (CommonSgdSoss.mergePartitionEvals partitionEvals)
        else
            evaluatedPoolSet

    /// Soss generation step with per-partition and merged evaluations and debug breakpoints
    let runGenerationStepSoss
            (mutator: sorterModelMutator)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (selectedSorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterChildCount>)
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sortableTestPartitions: partitionOfSortableTests)
            (generationsPerSubset: int<generationsPerSubset>)
            (generationsPerMergedSet: int<generationsPerSubset>)
            (prefix: ceBlock)
            (sorterEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (currentPoolSet: sorterPoolSet_Soss)
            (evaluateAllPartitionsForReport: bool)
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
            |> CommonSgdSoss.subsetIdForGeneration generationsPerSubset generationsPerMergedSet currentPoolSet.GenerationNumber

        // Members retain the selection subset used by the preceding generation step.
        let selectionSubsetChanged =
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

        let evaluateForSelection reEvaluate poolSet : sorterPoolSet_Soss =
            evaluateRequiredPartitions sortableTestPartitions prefix sorterEvalType reEvaluate
                collectNewSortableTests subsetIdForSelection false poolSet

        // Seed a newly selected partition before ranking parents, including on the first step.
        let preparedPoolSet =
            let hasMissingSelectionEval =
                poolSetForSelection.SorterPools
                |> Map.values
                |> Seq.collect (fun pool -> pool.SorterPoolMembers)
                |> Seq.exists (fun memberValue -> memberValue.SorterEvalForSelection.IsNone)
            if hasMissingSelectionEval then
                evaluateForSelection false poolSetForSelection
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

        // Transitions need fresh evaluations of every partition for merged length adjustment.
        let evaluatedPoolSet =
            if selectionSubsetChanged then
                mutatedPoolSet
                |> SorterPoolSet_Soss.withSubsetIdForSelection CommonSgdSoss.mergedSubsetId
                |> evaluateRequiredPartitions sortableTestPartitions prefix sorterEvalType true
                    collectNewSortableTests CommonSgdSoss.mergedSubsetId false
            else
                evaluateForSelection false mutatedPoolSet

        if hasEmptyPool evaluatedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if evaluation or eval update failed

        // --- Step 2: Adjust Constraint Boundaries ---
        let adjustedPoolSet =
            if selectionSubsetChanged then
                SorterPoolSet_Soss.adjustCeLengths sortedFractionThreshold evaluatedPoolSet
                |> SorterPoolSet_Soss.withSubsetIdForSelection subsetIdForSelection
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
        // Report-only partitions are needed for survivors, so defer them until after pruning.
        let poolSetForReport =
            if evaluateAllPartitionsForReport then
                evaluateRequiredPartitions sortableTestPartitions prefix sorterEvalType false
                    collectNewSortableTests subsetIdForSelection true prunedPoolSet
            else
                prunedPoolSet
        let finalPoolSet = SorterPoolSet_Soss.advanceGeneration 1 poolSetForReport

        finalPoolSet

