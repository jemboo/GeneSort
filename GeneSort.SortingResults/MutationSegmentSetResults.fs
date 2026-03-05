namespace GeneSort.SortingResult

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type mutationSegmentSetResults =
    private
        { 
          sortingMutationSegments : sortingMutationSegment []
          sortingSegmentResults: Dictionary<Guid<sortingMutationSegmentId>, Dictionary<Guid<sortingId>, sortingResult>>
          evalMap: Dictionary<Guid<sorterId>, sortingMutationSetTag>
        }
    with
    static member create (sortingMutationSegments: sortingMutationSegment []) =

        let evalMap = Dictionary<Guid<sorterId>, sortingMutationSetTag>()
        let sortingSegmentResults = Dictionary<Guid<sortingMutationSegmentId>, Dictionary<Guid<sortingId>, sortingResult>>()

        for segment in sortingMutationSegments do
            let segmentResults = Dictionary<Guid<sortingId>, sortingResult>()
            sortingSegmentResults.[segment.Id] <- segmentResults

            for (sorterId, sortingTag) in segment.MakeSorterIdsWithSortingTags do
                let mutationSetTag = SortingMutationSetTag.create segment.Id sortingTag
                evalMap.[sorterId] <- mutationSetTag

            match segment.SortingMutator with
            | sortingMutator.Single _ ->
                for id in segment.GetSortingIds do
                    segmentResults.[id] <- (singleSortingResult.create id |> sortingResult.Single)
            | sortingMutator.Pair _ ->
                for id in segment.GetSortingIds do
                    segmentResults.[id] <- (splitPairsSortingResult.create id |> sortingResult.SplitPairs)

        {
            sortingMutationSegments = sortingMutationSegments
            sortingSegmentResults = sortingSegmentResults
            evalMap = evalMap
        }

    member this.SortingMutationSegments with get() = this.sortingMutationSegments
    member this.SortingSegmentResults with get() = this.sortingSegmentResults
    member this.EvalMap with get() = this.evalMap

    member this.UpdateSortingResults (newEval: sorterEval) =
        match this.evalMap.TryGetValue(newEval.SorterId) with
        | false, _ -> failwithf "SorterId %A not found in evalMap." newEval.SorterId
        | true, mutationSetTag ->
            let segmentId = SortingMutationSetTag.getMutationSegmentId mutationSetTag
            let sortingId = SortingMutationSetTag.getSortingParentId mutationSetTag
            let modelTag  = SortingMutationSetTag.getModelTag mutationSetTag
            match this.sortingSegmentResults.TryGetValue(segmentId) with
            | false, _ -> failwithf "SegmentId %A not found in sortingSegmentResults." segmentId
            | true, segmentResults ->
                match segmentResults.TryGetValue(sortingId) with
                | false, _ -> failwithf "SortingId %A not found in segment %A." sortingId segmentId
                | true, sortingResult ->
                    SortingResult.UpdateSorterEval modelTag newEval sortingResult


    member this.UpdateAllSortingResults (newEvals: sorterEval []) =
        newEvals |> Array.iter(this.UpdateSortingResults)


    member this.GetSegmentResults (segmentId: Guid<sortingMutationSegmentId>) =
        match this.sortingSegmentResults.TryGetValue(segmentId) with
        | false, _ -> failwithf "SegmentId %A not found in sortingSegmentResults." segmentId
        | true, segmentResults -> segmentResults

module MutationSegmentSetResults = ()