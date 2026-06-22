namespace GeneSort.Eval.V1

open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1

module SorterPipeline =

    /// Executes one full generational iteration of the algorithm suite
    let runGenerationStep
            (mutator: sorterModelMutator)
            (mutantsPerSorter: int<sorterCount>)
            (sortableTest: sortableTest)
            (sorterEvalType: sorterEvalType)
            (collectNewSortableTests: bool)
            (selectionMeasure: sorterEvalMeasure)
            (prunedSize: int<sorterCountPerPool>)
            (currentPoolSet: sorterPoolSet) : sorterPoolSet =

        currentPoolSet
        // Step 1: Expand the population across all sub-pools
        |> SorterPoolSet.mutate mutator mutantsPerSorter
        
        // Step 2: Extract, evaluate, and transform the operational scores back into context
        |> (fun expandedPoolSet ->
            let computedEvals = 
                expandedPoolSet 
                |> SorterPoolRunner.evaluatePoolSet sortableTest sorterEvalType collectNewSortableTests
            
            expandedPoolSet |> SorterPoolSet.updateSorterEvals computedEvals
        )
        
        // Step 3: Trim out defective or un-optimized sorters down to baseline target capacities
        |> SorterPoolSet.pruneSorterPools selectionMeasure prunedSize
        |> SorterPoolSet.advanceGeneration 1