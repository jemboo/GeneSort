namespace GeneSort.SortingResults.Bins


open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.SortingResults


type mutationSegmentEvalBins =
    private {
        parentSortingResult: sortingResult
        mutantSortingEvalBins: sortingEvalBins
    }

    member this.AddMutantSorterEval (sorterEval: sorterEval) (modelTag:modelTag) =
        SortingEvalBins.addSorterEval this.mutantSortingEvalBins sorterEval modelTag

    member this.AddParentSorterEval (sorterEval: sorterEval) (modelTag:modelTag) =
        SortingResult.addSorterEval modelTag sorterEval this.parentSortingResult

    member this.ParentSortingResult with get() = this.parentSortingResult
    member this.MutantSortingEvalBins with get() = this.mutantSortingEvalBins


module MutationSegmentEvalBins =

    let makeFromStorage 
                (parentSortingResult: sortingResult) 
                (mutantSortingEvalBins: sortingEvalBins) =

        {
            parentSortingResult = parentSortingResult
            mutantSortingEvalBins = mutantSortingEvalBins
        }

    let makeFromSorting (ting: sorting) : mutationSegmentEvalBins =
        let mutantSortingEvalBins = SortingEvalBins.makeFromSorting ting
        let sortingResult = SortingResult.makeFromSorting ting
        {
            parentSortingResult = sortingResult
            mutantSortingEvalBins = mutantSortingEvalBins
        }