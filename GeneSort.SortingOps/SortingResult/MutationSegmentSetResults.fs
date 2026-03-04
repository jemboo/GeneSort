namespace GeneSort.SortingOps.SortingResult

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type mutationSegmentSetResults =
    private
        { 
          sortingMutationSegments : sortingMutationSegment []
          sortingResults: Dictionary<Guid<sortingId>, sortingResult>
          evalMap: Dictionary<Guid<sorterId>, sortingMutationSetTag>
        }
    with
    static member create (sortingMutationSegments: sortingMutationSegment []) =

        let evalMap = Dictionary<Guid<sorterId>, sortingMutationSetTag>()
        for segment in sortingMutationSegments do
            for (sorterId, sortingTag) in segment.MakeSorterIdsWithSortingTags do
                let mutationSetTag = SortingMutationSetTag.create segment.Id sortingTag
                evalMap.[sorterId] <- mutationSetTag

        let sortingResults = Dictionary<Guid<sortingId>, sortingResult>()
        for segment in sortingMutationSegments do
            match segment.SortingMutator with
            | sortingMutator.Single _ ->
                for id in segment.GetSortingIds do
                    sortingResults.[id] <- (singleSortingResult.create id |> sortingResult.Single)
            | sortingMutator.Pair _ ->
                for id in segment.GetSortingIds do
                    sortingResults.[id] <- (splitPairsSortingResult.create id |> sortingResult.SplitPairs)

        {
            sortingMutationSegments = sortingMutationSegments
            sortingResults = sortingResults
            evalMap = evalMap
        }

    member this.SortingMutationSegments with get() = this.sortingMutationSegments
    member this.SortingResults with get() = this.sortingResults
    member this.EvalMap with get() = this.evalMap

    member this.UpdateSortingResults (newEval: sorterEval) =
        match this.evalMap.TryGetValue(newEval.SorterId) with
        | false, _ -> failwithf "SorterId %A not found in evalMap." newEval.SorterId
        | true, mutationSetTag ->
            let sortingId = SortingMutationSetTag.getSortingParentId mutationSetTag
            let modelTag  = SortingMutationSetTag.getModelTag mutationSetTag
            match this.sortingResults.TryGetValue(sortingId) with
            | false, _ -> failwithf "SortingId %A not found in sortingResults." sortingId
            | true, sortingResult ->
                SortingResult.UpdateSorterEval modelTag newEval sortingResult

module MutationSegmentSetResults = ()