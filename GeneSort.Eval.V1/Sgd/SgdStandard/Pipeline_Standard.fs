namespace GeneSort.Eval.V1.Sgd.Standard

open System.Diagnostics
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd

module Pipeline_Standard =

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
            (adjustCeLengthFrequency: int<adjustCeLengthFrequency>)
            (currentPoolSet: sorterPoolSet_Standard) 
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (sortedFractionThreshold: float<sortedFraction>) : sorterPoolSet_Standard =

        let adjustCeLengthsThisGeneration =
            GenerationSchedule.shouldAdjustCeLengths adjustCeLengthFrequency currentPoolSet.GenerationNumber

        currentPoolSet
        // Step 1: Expand the population across all sub-pools
        |> SorterPoolSet_Standard.mutateAndTrim mutator selectedSorterCountPerPool selectionMeasure sorterChildCount
        
        |> (fun (expandedPoolSet: sorterPoolSet_Standard) ->
                let (computedEvals: Map<Guid<sorterPoolMemberId>, sorterEval>) = 
                    expandedPoolSet
                    |> PoolRunner_Standard.evaluatePoolSet 
                                        sortableTests 
                                        prefix
                                        sorterEvalType
                                        adjustCeLengthsThisGeneration
                                        collectNewSortableTests
            
                expandedPoolSet 
                |> SorterPoolSet_Standard.updateSorterEvals computedEvals
        )
        
        // Step 2b: Adjust the constraint boundaries based on performance thresholds
        |> (fun evaluatedPoolSet ->
            if adjustCeLengthsThisGeneration then
                SorterPoolSet_Standard.adjustCeLengths sortedFractionThreshold evaluatedPoolSet
            else
                evaluatedPoolSet)
        
        // Step 3: Trim out defective or un-optimized sorters down to baseline target capacities
        |> SorterPoolSet_Standard.pruneSorterPools 
                    selectionMeasure
                    prioritizeNewMutants
                    distinctSorterHashes 
                    sorterCountPerPool
        |> SorterPoolSet_Standard.advanceGeneration 1



    /// Debug version of runGenerationStep with explicit intermediate variables and debug breakpoints
    let runGenerationStepDebug
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
            (adjustCeLengthFrequency: int<adjustCeLengthFrequency>)
            (currentPoolSet: sorterPoolSet_Standard) 
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (sortedFractionThreshold: float<sortedFraction>) : sorterPoolSet_Standard =

        let adjustCeLengthsThisGeneration =
            GenerationSchedule.shouldAdjustCeLengths adjustCeLengthFrequency currentPoolSet.GenerationNumber

        // Helper to check if any pool in a poolSet has dropped to 0 members
        let hasEmptyPool (poolSet: sorterPoolSet_Standard) =
            poolSet.SorterPools 
            |> Map.exists (fun _ pool -> Seq.isEmpty pool.SorterPoolMembers)

        // --- Step 1a: Mutate / Expand Population ---
        let mutatedPoolSet = SorterPoolSet_Standard.mutateAndTrim 
                                    mutator 
                                    selectedSorterCountPerPool
                                    selectionMeasure
                                    sorterChildCount 
                                    currentPoolSet

        if hasEmptyPool mutatedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if mutation resulted in an empty pool

        // --- Step 1b: Evaluate Pool Set ---
        let computedEvals = 
            PoolRunner_Standard.evaluatePoolSet 
                sortableTests 
                prefix
                sorterEvalType 
                adjustCeLengthsThisGeneration
                collectNewSortableTests
                mutatedPoolSet

        let evaluatedPoolSet = SorterPoolSet_Standard.updateSorterEvals computedEvals mutatedPoolSet

        if hasEmptyPool evaluatedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if evaluation or eval update failed

        // --- Step 2: Adjust Constraint Boundaries ---
        let adjustedPoolSet = 
            if adjustCeLengthsThisGeneration then
                SorterPoolSet_Standard.adjustCeLengths sortedFractionThreshold evaluatedPoolSet
            else
                evaluatedPoolSet

        if hasEmptyPool adjustedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if length adjustment emptied a pool

        // --- Step 3: Prune Sorter Pools ---
        let prunedPoolSet = 
            SorterPoolSet_Standard.pruneSorterPools 
                selectionMeasure 
                prioritizeNewMutants 
                distinctSorterHashes 
                sorterCountPerPool
                adjustedPoolSet

        if hasEmptyPool prunedPoolSet && Debugger.IsAttached then
            Debugger.Break() // Pause if pruning reduced a pool to zero members

        // --- Step 4: Advance Generation Counter ---
        let finalPoolSet = SorterPoolSet_Standard.advanceGeneration 1 prunedPoolSet

        finalPoolSet

