namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair
open GeneSort.SortingOps


type mutationSegmentResults =
    private
        { 
          sortingMutationSegment : sortingMutationSegment
          sortingResultMapParent : sortingResultMap
          sortingResultSetMapMutants : sortingResultSetMap
        }
    with
    static member create (sortingMutationSegment: sortingMutationSegment) =
        let mutantSortingResults =
            match sortingMutationSegment.SortingMutator with
            | sortingMutator.Single _ ->
                sortingMutationSegment.GetMutantSortingIds
                |> Array.map (fun id -> singleSortingResult.create id |> sortingResult.Single)
            | sortingMutator.Pair _ ->
                sortingMutationSegment.GetMutantSortingIds
                |> Array.map (fun id -> splitPairsSortingResult.create id 
                                        |> pairsSortingResult.SplitPairs
                                        |> sortingResult.Pairs)

        let parentSortingResult = 
            match sortingMutationSegment.SortingMutator with
            | sortingMutator.Single smm -> 
                let parentSortingId = %(smm |> SorterModelMutator.getParentSorterModelId) |> UMX.tag<sortingId>
                singleSortingResult.create parentSortingId |> sortingResult.Single
            | sortingMutator.Pair spmm ->
                let parentSortingId = %(spmm |> SorterPairModelMutator.getParentSorterPairId) |> UMX.tag<sortingId>
                splitPairsSortingResult.create parentSortingId |> pairsSortingResult.SplitPairs |> sortingResult.Pairs

        { 
            sortingMutationSegment = sortingMutationSegment
            sortingResultSetMapMutants = sortingResultSetMap.create 
                                        mutantSortingResults 
                                        sortingMutationSegment.MakeMutantSorterIdsWithSortingTags

            sortingResultMapParent = sortingResultMap.create 
                                        parentSortingResult 
                                        sortingMutationSegment.MakeParentSorterIdsWithModelTags
           
        }

    member this.SortingMutationSegment with get() = this.sortingMutationSegment
    member this.SortingResultMapParent with get() = this.sortingResultMapParent
    member this.SortingResultSetMapMutants with get() = this.sortingResultSetMapMutants

    member this.UpdateSortingResultsMutant (newEval: sorterEval) =
        this.sortingResultSetMapMutants.UpdateSortingResults newEval

    member this.UpdateSortingResultParent (newEval: sorterEval) =
        this.sortingResultMapParent.UpdateSortingResult newEval

    member this.GetAllTaggedParentSorterEvals () : (sorterEval * modelSetTag) seq =
        this.sortingResultMapParent.GetAllTaggedSorterEvals ()

    member this.GetAllTaggedMutantSorterEvals () : (sorterEval * modelSetTag) seq =
        this.sortingResultSetMapMutants.GetAllTaggedSorterEvals ()



module MutationSegmentResults = 

    let load (sortingMutationSegment: sortingMutationSegment) 
             (sortingResultMapParent: sortingResultMap) 
             (sortingResultSetMapMutants: sortingResultSetMap) : mutationSegmentResults =
        {
            sortingMutationSegment = sortingMutationSegment
            sortingResultMapParent = sortingResultMapParent
            sortingResultSetMapMutants = sortingResultSetMapMutants
        }