namespace GeneSort.SortingResult

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair
open GeneSort.SortingOps


type mutationSegmentResults =
    private
        { 
          sortingMutationSegment : sortingMutationSegment
          sortingResultSetMapMutants : sortingResultSetMap
          sortingResultMapParent : sortingResult
        }
    with
    static member create (sortingMutationSegment: sortingMutationSegment) =
        let mutantSortingResults =
            match sortingMutationSegment.SortingMutator with
            | sortingMutator.Single _ ->
                sortingMutationSegment.GetSortingIds
                |> Array.map (fun id -> singleSortingResult.create id |> sortingResult.Single)
            | sortingMutator.Pair _ ->
                sortingMutationSegment.GetSortingIds
                |> Array.map (fun id -> splitPairsSortingResult.create id |> sortingResult.SplitPairs)

        let parentSortingResult = 
            match sortingMutationSegment.SortingMutator with
            | sortingMutator.Single smm -> 
                let parentSortingId = %(smm |> SorterModelMutator.getParentSorterModelId) |> UMX.tag<sortingId>
                singleSortingResult.create parentSortingId |> sortingResult.Single
            | sortingMutator.Pair spmm ->
                let parentSortingId = %(spmm |> SorterPairModelMutator.getParentSorterPairId) |> UMX.tag<sortingId>
                splitPairsSortingResult.create parentSortingId |> sortingResult.SplitPairs

        { 
            sortingMutationSegment = sortingMutationSegment
            sortingResultSetMapMutants = sortingResultSetMap.create 
                                        mutantSortingResults 
                                        sortingMutationSegment.MakeSorterIdsWithSortingTags

            sortingResultMapParent = parentSortingResult
           
        }

    member this.SortingMutationSegment with get() = this.sortingMutationSegment
    member this.SortingResultMapParent with get() = this.sortingResultMapParent
    member this.SortingResultSetMapMutants with get() = this.sortingResultSetMapMutants

    member this.UpdateSortingResultsMutant (newEval: sorterEval) =
        this.sortingResultSetMapMutants.UpdateSorterEval newEval



module MutationSegmentResults = ()