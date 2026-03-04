namespace GeneSort.SortingOps.SortingResult

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type mutationSegmentResults =
    private
        { 
          sortingMutationSegment : sortingMutationSegment
          sortingResults: Dictionary<Guid<sortingId>, sortingResult>
          evalMap: Dictionary<Guid<sorterId>, sortingTag>
        }
    with
    static member create (sortingMutationSegment: sortingMutationSegment) =

        let evalMap = Dictionary<Guid<sorterId>, sortingTag>()
        for (sorterId, sortingTag) in sortingMutationSegment.MakeSorterIdsWithSortingTags do
            evalMap.[sorterId] <- sortingTag

        let sortingResults = Dictionary<Guid<sortingId>, sortingResult>()
        match sortingMutationSegment.SortingMutator with
        | sortingMutator.Single _ ->
            for id in sortingMutationSegment.GetSortingIds do
                sortingResults.[id] <- (singleSortingResult.create id |> sortingResult.Single)
        | sortingMutator.Pair _ ->
            for id in sortingMutationSegment.GetSortingIds do
                sortingResults.[id] <- (splitPairsSortingResult.create id |> sortingResult.SplitPairs)

        { 
            sortingMutationSegment = sortingMutationSegment
            sortingResults = sortingResults
            evalMap = evalMap
        }

    member this.SortingMutationSegment with get() = this.sortingMutationSegment
    member this.SortingResults with get() = this.sortingResults
    member this.EvalMap with get() = this.evalMap

    member this.UpdateSortingResults (newEval: sorterEval) =
        match this.evalMap.TryGetValue(newEval.SorterId) with
        | false, _ -> failwithf "SorterId %A not found in evalMap." newEval.SorterId
        | true, sortingTag ->
            let sortingId = SortingTag.getSortingParentId sortingTag
            let modelTag  = SortingTag.getModelTag sortingTag
            match this.sortingResults.TryGetValue(sortingId) with
            | false, _ -> failwithf "SortingId %A not found in sortingResults." sortingId
            | true, sortingResult ->
                SortingResult.UpdateSorterEval modelTag newEval sortingResult

module MutationSegmentResults = ()