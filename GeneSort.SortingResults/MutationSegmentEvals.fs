namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair
open GeneSort.SortingOps


type mutationSegmentEvals =
    private
        { 
          sortingMutationSegment : sortingMutationSegment
          sortingEvalMapParent : sortingEvalMap
          sortingEvalSetMapMutants : sortingEvalSetMap
        }
    with
    static member create (sortingMutationSegment: sortingMutationSegment) =
        let mutantSortingResults =
            match sortingMutationSegment.SortingMutator with
            | sortingMutator.Single _ ->
                sortingMutationSegment.GetMutantSortingIds
                |> Array.map (fun id -> singleSortingEval.create id |> sortingEval.Single)
            | sortingMutator.Pair _ ->
                sortingMutationSegment.GetMutantSortingIds
                |> Array.map (fun id -> splitPairsSortingEval.create id 
                                        |> pairsSortingEval.SplitPairs
                                        |> sortingEval.Pairs)

        let parentSortingResult = 
            match sortingMutationSegment.SortingMutator with
            | sortingMutator.Single smm -> 
                let parentSortingId = %(smm |> SorterModelMutator.getParentSorterModelId) |> UMX.tag<sortingId>
                singleSortingEval.create parentSortingId |> sortingEval.Single
            | sortingMutator.Pair spmm ->
                let parentSortingId = %(spmm |> SorterPairModelMutator.getParentSorterPairId) |> UMX.tag<sortingId>
                splitPairsSortingEval.create parentSortingId |> pairsSortingEval.SplitPairs |> sortingEval.Pairs

        { 
            sortingMutationSegment = sortingMutationSegment
            sortingEvalSetMapMutants = sortingEvalSetMap.create 
                                        mutantSortingResults 
                                        sortingMutationSegment.MakeMutantSorterIdsWithSortingTags

            sortingEvalMapParent = sortingEvalMap.create 
                                        parentSortingResult 
                                        sortingMutationSegment.MakeParentSorterIdsWithModelTags
           
        }

    member this.SortingMutationSegment with get() = this.sortingMutationSegment
    member this.SortingEvalMapParent with get() = this.sortingEvalMapParent
    member this.SortingEvalSetMapMutants with get() = this.sortingEvalSetMapMutants

    member this.UpdateSortingEvalSetMapMutants (newEval: sorterEvalOld) =
        this.sortingEvalSetMapMutants.UpdateSortingEvals newEval

    member this.UpdateSortingEvalMapParent (newEval: sorterEvalOld) =
        this.sortingEvalMapParent.UpdateSortingEval newEval

    member this.GetAllTaggedParentSorterEvals () : (sorterEvalOld * modelSetTag) seq =
        this.sortingEvalMapParent.GetAllTaggedSorterEvals ()

    member this.GetAllTaggedMutantSorterEvals () : (sorterEvalOld * modelSetTag) seq =
        this.sortingEvalSetMapMutants.GetAllTaggedSorterEvals ()



module MutationSegmentEvals = 

    let load (sortingMutationSegment: sortingMutationSegment) 
             (sortingResultMapParent: sortingEvalMap) 
             (sortingResultSetMapMutants: sortingEvalSetMap) : mutationSegmentEvals =
        {
            sortingMutationSegment = sortingMutationSegment
            sortingEvalMapParent = sortingResultMapParent
            sortingEvalSetMapMutants = sortingResultSetMapMutants
        }