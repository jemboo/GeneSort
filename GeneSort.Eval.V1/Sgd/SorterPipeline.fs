namespace GeneSort.Eval.V1

open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1

module SorterPipeline =

    /// Executes one full generational iteration of the algorithm suite
    let runGenerationStep
            (mutator: sorterModelMutator)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterCount>)
            (sortableTest: sortableTest)
            (sorterEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (reEvaluateParents: bool)
            (currentPoolSet: sorterPoolSet) : sorterPoolSet =

        currentPoolSet
        // Step 1: Expand the population across all sub-pools
        |> SorterPoolSet.mutate mutator sorterChildCount
        
        // Step 2: Extract, evaluate, and transform the operational scores back into context
        |> (fun expandedPoolSet ->
                let computedEvals = 
                    expandedPoolSet 
                    |> SorterPoolRunner.evaluatePoolSet 
                                        sortableTest 
                                        sorterEvalType 
                                        reEvaluateParents
            
                expandedPoolSet 
                |> SorterPoolSet.updateSorterEvals computedEvals
        )
        
        // Step 3: Trim out defective or un-optimized sorters down to baseline target capacities
        |> SorterPoolSet.pruneSorterPools selectionMeasure sorterCountPerPool
        |> SorterPoolSet.advanceGeneration 1