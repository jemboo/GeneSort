namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1

module SorterPipeline =

    /// Executes one full generational iteration of the algorithm suite
    let runGenerationStep
            (mutator: sorterModelMutator)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterChildCount>)
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sortableTest: sortableTest)
            (sorterEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (reEvaluateParents: bool)
            (currentPoolSet: sorterPoolSet) 
            (sortedFractionThreshold: float<sortedFraction>) : sorterPoolSet =

        currentPoolSet
        // Step 1: Expand the population across all sub-pools
        |> SorterPoolSet.mutate mutator sorterChildCount
        
        |> (fun (expandedPoolSet: sorterPoolSet) ->
                let (computedEvals: Map<Guid<sorterPoolMemberId>, sorterEval>) = 
                    expandedPoolSet
                    |> SorterPoolRunner.evaluatePoolSet 
                                        sortableTest 
                                        sorterEvalType
                                        reEvaluateParents
            
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