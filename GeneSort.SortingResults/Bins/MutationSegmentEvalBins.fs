namespace GeneSort.SortingResults.Bins

open System.Collections.Generic
open FSharp.UMX

open GeneSort.SortingOps
open GeneSort.Model.Sorting
open GeneSort.SortingResults


type mutationSegmentEvalBins =
    private {
        parentSortingResult: sortingResult
        mutantSortingEvalBins: sortingEvalBins
    }

    member this.ParentSortingResult with get() = this.parentSortingResult
    member this.MutantSortingEvalBins with get() = this.mutantSortingEvalBins


module MutationSegmentEvalBins =

    let makeFromSorting (ting: sorting) : mutationSegmentEvalBins =
        let mutantSortingEvalBins = SortingEvalBins.makeFromSorting ting
        let sortingResult = SortingResult.makeFromSorting ting
        {
            parentSortingResult = sortingResult
            mutantSortingEvalBins = mutantSortingEvalBins
        }