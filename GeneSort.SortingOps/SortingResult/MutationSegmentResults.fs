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
          evalMap: Map<Guid<sorterId>, sortingTag>
        }
    with
    static member create (sortingMutationSegment: sortingMutationSegment) =

        let evalMap = 
            sortingMutationSegment.MakeSorterIdsWithSortingTags
            |> Map.ofArray

        let sortingResults = 
            match sortingMutationSegment.SortingMutator with
            | sortingMutator.Single _ ->
                sortingMutationSegment.GetSortingIds
                |> Array.map (fun id -> (id, singleSortingResult.create id |> sortingResult.Single))
                |> Map.ofArray

            | sortingMutator.Pair _ ->
                sortingMutationSegment.GetSortingIds
                |> Array.map (fun id -> (id, splitPairsSortingResult.create id |> sortingResult.SplitPairs))
                |> Map.ofArray

        { 
            sortingMutationSegment = sortingMutationSegment; 
            sortingResults = sortingResults;
            evalMap = evalMap
        }


    member this.SortingMutationSegment with get() = this.sortingMutationSegment
    member this.SortingResults with get() = this.sortingResults
    member this.EvalMap with get() = this.evalMap

    member this.UpdateSortingResults (newEval: sorterEval)  =
            let (sortingId, modelTag) = this.EvalMap.[newEval.SorterId]
            this.sortingResults.[sortingId] |> SortingResult.UpdateSorterEval modelTag newEval


module MutationSegmentResults = ()

