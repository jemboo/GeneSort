namespace GeneSort.Eval.V1

open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1

module SorterEvolutionEngine =

    /// Holds the combined results of the historical optimization run
    type EvolutionRunResult = {
        IntermediateHistory: sorterPoolSetDescription array
        FinalPoolSet: sorterPoolSet
    }

    /// Recursively runs the generation loop step N times. 
    /// Saves optimized lightweight history tokens for intermediate steps, 
    /// and preserves the fully realized heavy pool data structure on the final step.
    let runEvolution 
            (n: int)
            (mutator: sorterModelMutator)
            (mutantsPerSorter: int<sorterCount>)
            (sortableTest: sortableTest)
            (sorterEvalType: sorterEvalType)
            (collectNewSortableTests: bool)
            (selectionMeasure: sorterEvalMeasure)
            (prunedSize: int<sorterCount>)
            (initialPoolSet: sorterPoolSet) : EvolutionRunResult =

        let rec loop (remainingSteps: int) (currentSet: sorterPoolSet) (historyAcc: sorterPoolSetDescription list) =
            if remainingSteps <= 0 then
                {
                    // Convert accumulated list to array for structural permanence
                    IntermediateHistory = historyAcc |> List.rev |> List.toArray
                    FinalPoolSet = currentSet
                }
            else
                // 1. Take a lightweight snapshot of the current generation state before transitioning
                let currentSnapshot = SorterPoolSetDescription.fromPoolSet currentSet
                let updatedHistory = currentSnapshot :: historyAcc

                // 2. Advance the heavy state across the pipeline axis
                let nextSet = 
                    currentSet 
                    |> SorterPipeline.runGenerationStep 
                        mutator 
                        mutantsPerSorter 
                        sortableTest 
                        sorterEvalType 
                        collectNewSortableTests 
                        selectionMeasure 
                        prunedSize

                loop (remainingSteps - 1) nextSet updatedHistory

        loop n initialPoolSet []