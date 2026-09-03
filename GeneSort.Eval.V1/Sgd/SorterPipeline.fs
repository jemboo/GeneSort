namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open System.Diagnostics

module SorterPipeline =

    /// Executes one full generational iteration of the algorithm suite
    let runGenerationStep
            (mutator: sorterModelMutator)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (selectedSorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterChildCount>)
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sortableTest: sortableTest)
            (prefix: ceBlock)
            (sorterEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (reEvaluateParents: bool)
            (currentPoolSet: sorterPoolSet) 
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (sortedFractionThreshold: float<sortedFraction>) : sorterPoolSet =

        currentPoolSet
        // Step 1: Expand the population across all sub-pools
        |> SorterPoolSet.mutateAndTrim mutator selectedSorterCountPerPool selectionMeasure sorterChildCount
        
        |> (fun (expandedPoolSet: sorterPoolSet) ->
                let (computedEvals: Map<Guid<sorterPoolMemberId>, sorterEval>) = 
                    expandedPoolSet
                    |> SorterPoolRunner.evaluatePoolSet 
                                        sortableTest 
                                        prefix
                                        sorterEvalType
                                        reEvaluateParents
                                        collectNewSortableTests
            
                expandedPoolSet 
                |> SorterPoolSet.updateSorterEvals computedEvals
        )
        
        // Step 2b: Adjust the constraint boundaries based on performance thresholds
        |> SorterPoolSet.adjustCeLengths sortedFractionThreshold
        
        // Step 3: Trim out defective or un-optimized sorters down to baseline target capacities
        |> SorterPoolSet.pruneSorterPools 
                    selectionMeasure
                    prioritizeNewMutants
                    distinctSorterHashes 
                    sorterCountPerPool
        |> SorterPoolSet.advanceGeneration 1



    /// Debug version of runGenerationStep with explicit intermediate variables and debug breakpoints
    let runGenerationStepDebug
            (mutator: sorterModelMutator)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (selectedSorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterChildCount>)
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sortableTest: sortableTest)
            (prefix: ceBlock)
            (sorterEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (reEvaluateParents: bool)
            (currentPoolSet: sorterPoolSet) 
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (sortedFractionThreshold: float<sortedFraction>) : sorterPoolSet =

        // Helper to check if any pool in a poolSet has dropped to 0 members
        let hasEmptyPool (poolSet: sorterPoolSet) =
            poolSet.SorterPools 
            |> Map.exists (fun _ pool -> Seq.isEmpty pool.SorterPoolMembers)

        // --- Step 1a: Mutate / Expand Population ---
        let mutatedPoolSet = SorterPoolSet.mutateAndTrim 
                                    mutator 
                                    selectedSorterCountPerPool
                                    selectionMeasure
                                    sorterChildCount 
                                    currentPoolSet

        if hasEmptyPool mutatedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if mutation resulted in an empty pool

        // --- Step 1b: Evaluate Pool Set ---
        let computedEvals = 
            SorterPoolRunner.evaluatePoolSet 
                sortableTest 
                prefix
                sorterEvalType 
                reEvaluateParents
                collectNewSortableTests
                mutatedPoolSet

        let evaluatedPoolSet = SorterPoolSet.updateSorterEvals computedEvals mutatedPoolSet

        if hasEmptyPool evaluatedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if evaluation or eval update failed

        // --- Step 2: Adjust Constraint Boundaries ---
        let adjustedPoolSet = 
            if reEvaluateParents then
                SorterPoolSet.adjustCeLengths sortedFractionThreshold evaluatedPoolSet
            else
                evaluatedPoolSet

        if hasEmptyPool adjustedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if length adjustment emptied a pool

        // --- Step 3: Prune Sorter Pools ---
        let prunedPoolSet = 
            SorterPoolSet.pruneSorterPools 
                selectionMeasure 
                prioritizeNewMutants 
                distinctSorterHashes 
                sorterCountPerPool
                adjustedPoolSet

        if hasEmptyPool prunedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if pruning reduced a pool to zero members

        // --- Step 4: Advance Generation Counter ---
        let finalPoolSet = SorterPoolSet.advanceGeneration 1 prunedPoolSet

        finalPoolSet

