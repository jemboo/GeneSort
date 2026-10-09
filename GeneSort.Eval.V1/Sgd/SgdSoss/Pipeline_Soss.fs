namespace GeneSort.Eval.V1.Sgd.Soss

open FSharp.UMX
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open System.Diagnostics

module Pipeline_Soss =

    /// Executes one full generational iteration of the algorithm suite
    let runGenerationStep
            (mutator: sorterModelMutator)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (selectedSorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterChildCount>)
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sortableTests: sortableTests)
            (prefix: ceBlock)
            (sorterEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (reEvaluateParents: bool)
            (currentPoolSet: sorterPoolSet_Soss) 
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (sortedFractionThreshold: float<sortedFraction>) : sorterPoolSet_Soss =

        currentPoolSet
        // Step 1: Expand the population across all sub-pools
        |> SorterPoolSet_Soss.mutateAndTrim mutator selectedSorterCountPerPool selectionMeasure sorterChildCount
        
        |> (fun (expandedPoolSet: sorterPoolSet_Soss) ->
                let (computedEvals: Map<Guid<sorterPoolMemberId>, sorterEval>) = 
                    expandedPoolSet
                    |> PoolRunner_Soss.evaluatePoolSet 
                                        sortableTests 
                                        prefix
                                        sorterEvalType
                                        reEvaluateParents
                                        collectNewSortableTests
            
                expandedPoolSet 
                |> SorterPoolSet_Soss.updateSorterEvals computedEvals
        )
        
        // Step 2b: Adjust the constraint boundaries based on performance thresholds
        |> SorterPoolSet_Soss.adjustCeLengths sortedFractionThreshold
        
        // Step 3: Trim out defective or un-optimized sorters down to baseline target capacities
        |> SorterPoolSet_Soss.pruneSorterPools 
                    selectionMeasure
                    prioritizeNewMutants
                    distinctSorterHashes 
                    sorterCountPerPool
        |> SorterPoolSet_Soss.advanceGeneration 1


    /// Soss generation step with per-partition and merged evaluations and debug breakpoints
    let runGenerationStepSossDebug
            (mutator: sorterModelMutator)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (selectedSorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterChildCount>)
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sortableTestPartitions: setOfSortableTests)
            (prefix: ceBlock)
            (sorterEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (reEvaluateParents: bool)
            (currentPoolSet: sorterPoolSet_Soss)
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (sortedFractionThreshold: float<sortedFraction>) : sorterPoolSet_Soss =

        // Helper to check if any pool in a poolSet has dropped to 0 members
        let hasEmptyPool (poolSet: sorterPoolSet_Soss) =
            poolSet.SorterPools
            |> Map.exists (fun _ pool -> Seq.isEmpty pool.SorterPoolMembers)

        // --- Step 1a: Mutate / Expand Population ---
        let mutatedPoolSet = SorterPoolSet_Soss.mutateAndTrim
                                    mutator
                                    selectedSorterCountPerPool
                                    selectionMeasure
                                    sorterChildCount
                                    currentPoolSet

        if hasEmptyPool mutatedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if mutation resulted in an empty pool

        // --- Step 1b: Evaluate Pool Set ---

        let partitionEvals =
            sortableTestPartitions.SortableTests
            |> Map.map (fun subsetId tests ->
                PoolRunner_Soss.evaluatePoolSet
                    tests
                    prefix
                    sorterEvalType
                    reEvaluateParents
                    collectNewSortableTests
                    mutatedPoolSet
                |> Map.map (fun _ eval -> SorterEval.withSortableTestsSubsetId subsetId eval))

        // Keep each partition evaluation and the merged evaluation together per member.
        let computedEvals =
            let partitionMaps = partitionEvals |> Map.toArray
            let _, firstEvals = partitionMaps.[0]
            firstEvals
            |> Map.map (fun memberId firstEval ->
                let memberPartitionEvals =
                    partitionMaps
                    |> Array.map (fun (subsetId, evals) -> subsetId, evals.[memberId])
                    |> Map.ofArray
                let mergedEval =
                    if partitionMaps.Length = 1 then firstEval
                    else SorterEval.merge firstEval (snd partitionMaps.[1]).[memberId]
                {| PartitionEvals = memberPartitionEvals; MergedEval = mergedEval |})

        let mergedEvals = computedEvals |> Map.map (fun _ evals -> evals.MergedEval)
        let evaluatedPoolSet = SorterPoolSet_Soss.updateSorterEvals mergedEvals mutatedPoolSet

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

