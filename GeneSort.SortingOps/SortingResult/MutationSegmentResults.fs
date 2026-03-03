namespace GeneSort.SortingOps.SortingResult

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting


type mutationSegmentResults =
    private
        { 
          sortingMutationSegment : sortingMutationSegment
          sortingResults: Map<Guid<sortingId>, sortingResult>
          evalMap: Map<Guid<sorterId>, sortingResult>
        }
    with
    static member create (sortingMutationSegment: sortingMutationSegment) =

        match sortingMutationSegment.SortingMutator with
        | sortingMutator.Single _ -> 
            failwith "MutationResultSet can only be created with a sortingSetMutator that contains a SplitPairs sortingMutator."
        | sortingMutator.Pair _ ->
            failwith "MutationResultSet can only be created with a sortingSetMutator that contains a SplitPairs sortingMutator."
        { 
            sortingMutationSegment = sortingMutationSegment; 
            sortingResults = Map.empty ;
            evalMap = Map.empty
        }


module MutationSegmentResults = ()

