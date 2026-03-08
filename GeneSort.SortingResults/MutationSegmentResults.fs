namespace GeneSort.SortingResult

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type mutationSegmentResults =
    private
        { 
          sortingMutationSegment : sortingMutationSegment
          sortingResultSet : sortingResultSet
          evalMap: Dictionary<Guid<sorterId>, sortingTag>
        }
    with
    static member create (sortingMutationSegment: sortingMutationSegment) =
        let evalMap = Dictionary<Guid<sorterId>, sortingTag>()
        for (sorterId, sortingTag) in sortingMutationSegment.MakeSorterIdsWithSortingTags do
            evalMap.[sorterId] <- sortingTag
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
            sortingResultSet = sortingResultSet.create sortingResults
            evalMap = evalMap
        }

    member this.SortingMutationSegment with get() = this.sortingMutationSegment
    member this.SortingResultSet with get() = this.sortingResultSet
    member this.EvalMap with get() = this.evalMap

    member this.UpdateSortingResults (newEval: sorterEval) =
        match this.evalMap.TryGetValue(newEval.SorterId) with
        | false, _ -> failwithf "SorterId %A not found in evalMap." newEval.SorterId
        | true, sortingTag ->
            let sortingId = SortingTag.getSortingParentId sortingTag
            let modelTag  = SortingTag.getModelTag sortingTag
            this.sortingResultSet.UpdateSorterEval sortingId modelTag newEval

module MutationSegmentResults = ()