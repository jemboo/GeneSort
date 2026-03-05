namespace GeneSort.SortingResult

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


type sortingResult=
     | Single of singleSortingResult
     | SplitPairs of splitPairsSortingResult


module SortingResult =

    let getSortingId (sortingResult: sortingResult) : Guid<sortingId> =
        match sortingResult with
        | Single singleResult -> singleResult.SortingId
        | SplitPairs splitPairsResult -> splitPairsResult.SortingId


    let UpdateSorterEval (modelTag: modelTag) 
                         (newEval: sorterEval) 
                         (sortingResult: sortingResult) : unit =
        match sortingResult with
        | Single ssr -> ssr.UpdateSorterEval modelTag newEval
        | SplitPairs spsr -> spsr.UpdateSorterEval modelTag newEval
            