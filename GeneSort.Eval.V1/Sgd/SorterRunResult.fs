namespace GeneSort.Eval.V1

open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1


/// Holds the combined results of the historical optimization run
type sorterRunResult = 
    private {
        _intermediateHistory: sorterPoolSetDescription array
        _finalPoolSet: sorterPoolSet
    }
    member this.FinalPoolSet with get() = this._finalPoolSet
    member this.IntermediateHistory with get() = this._intermediateHistory
    static member create 
                    (finalPoolSet:sorterPoolSet) 
                    (intermediateHistory:sorterPoolSetDescription []) =
            {
                _intermediateHistory = intermediateHistory
                _finalPoolSet = finalPoolSet
            }

module SorterRunResult =

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
            (initialPoolSet: sorterPoolSet) : sorterRunResult =

        let rec loop (remainingSteps: int) (currentSet: sorterPoolSet) (historyAcc: sorterPoolSetDescription list) =
            if remainingSteps <= 0 then
                sorterRunResult.create
                        currentSet
                        (historyAcc |> List.rev |> List.toArray)
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