namespace GeneSort.SortingResult

open GeneSort.Model.Sorting
open GeneSort.SortingOps


type mutationSegmentResults =
    private
        { 
          sortingMutationSegment : sortingMutationSegment
          sortingResultSetMutants : sortingResultSet
          sortingResultSetParent : sortingResultSet
        }
    with
    static member create (sortingMutationSegment: sortingMutationSegment) =
        let sortingResults =
            match sortingMutationSegment.SortingMutator with
            | sortingMutator.Single _ ->
                sortingMutationSegment.GetSortingIds
                |> Array.map (fun id -> singleSortingResult.create id |> sortingResult.Single)
            | sortingMutator.Pair _ ->
                sortingMutationSegment.GetSortingIds
                |> Array.map (fun id -> splitPairsSortingResult.create id |> sortingResult.SplitPairs)
        { 
            sortingMutationSegment = sortingMutationSegment
            sortingResultSetMutants = sortingResultSet.create 
                                        sortingResults 
                                        sortingMutationSegment.MakeSorterIdsWithSortingTags

            sortingResultSetParent = 
            
                            sortingResultSet.create 
                                        [||] 
                                        [||]  //(SortingMutator.getMutatorSeedSorterIdsWithTags sortingMutationSegment.SortingMutator)
        }

    member this.SortingMutationSegment with get() = this.sortingMutationSegment
    member this.SortingResultSetParent with get() = this.sortingResultSetParent
    member this.SortingResultSetMutants with get() = this.sortingResultSetMutants

    member this.UpdateSortingResultsMutant (newEval: sorterEval) =
        this.sortingResultSetMutants.UpdateSorterEval newEval



module MutationSegmentResults = ()