namespace GeneSort.SortingOps.SortingResult

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


type sortingResult=
     | Single of singleSortingResult
     | SplitPairs of splitPairsSortingResult


module SortingResult =

    let getSortingId (sortingResult: sortingResult) : Guid<sortingId> =
        match sortingResult with
        | Single singleResult -> %(singleResult.SorterModelId)  |> UMX.tag<sortingId>
        | SplitPairs splitPairsResult -> %(splitPairsResult.SorterModelId) |> UMX.tag<sortingId>


