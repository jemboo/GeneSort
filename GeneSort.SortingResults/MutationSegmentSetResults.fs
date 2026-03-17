namespace GeneSort.SortingResults

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type mutationSegmentSetResults =
    private
        { 
          sortingMutationSegments : sortingMutationSegment []
          sortingSegmentResults: Dictionary<Guid<sortingMutationSegmentId>, mutationSegmentResults>
          // Routing lookups
          mutantSorterToSegmentMap: Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>
          parentSorterToSegmentMap: Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>
        }
    with
    static member create (sortingMutationSegments: sortingMutationSegment []) =
        let sortingSegmentResults = Dictionary<Guid<sortingMutationSegmentId>, mutationSegmentResults>()
        let mutantMap = Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>()
        let parentMap = Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>()

        for segment in sortingMutationSegments do
            // 1. Create the segment result object
            let segmentResult = mutationSegmentResults.create segment
            sortingSegmentResults.[segment.Id] <- segmentResult

            // 2. Map Mutant IDs to this SegmentId
            for (sorterId, _) in segment.MakeMutantSorterIdsWithSortingTags do
                mutantMap.[sorterId] <- segment.Id

            // 3. Map Parent IDs to this SegmentId
            for (sorterId, _) in segment.MakeParentSorterIdsWithModelTags do
                parentMap.[sorterId] <- segment.Id

        {
            sortingMutationSegments = sortingMutationSegments
            sortingSegmentResults = sortingSegmentResults
            mutantSorterToSegmentMap = mutantMap
            parentSorterToSegmentMap = parentMap
        }

    member this.SortingMutationSegments with get() = this.sortingMutationSegments
    member this.SortingSegmentResults with get() = this.sortingSegmentResults

    /// Automatically routes the evaluation to the correct Mutant result
    member this.UpdateSortingResultsMutant (newEval: sorterEval) =
        match this.mutantSorterToSegmentMap.TryGetValue(newEval.SorterId) with
        | true, segmentId -> 
            this.sortingSegmentResults.[segmentId].UpdateSortingResultsMutant newEval
        | false, _ -> failwithf "Mutant SorterId %A not found in any segment." newEval.SorterId

    /// Automatically routes the evaluation to the correct Parent result
    member this.UpdateSortingResultParent (newEval: sorterEval) =
        match this.parentSorterToSegmentMap.TryGetValue(newEval.SorterId) with
        | true, segmentId -> 
            this.sortingSegmentResults.[segmentId].UpdateSortingResultParent newEval
        | false, _ -> failwithf "Parent SorterId %A not found in any segment." newEval.SorterId

    member this.GetSegmentResults (segmentId: Guid<sortingMutationSegmentId>) =
        match this.sortingSegmentResults.TryGetValue(segmentId) with
        | true, segmentResults -> segmentResults
        | false, _ -> failwithf "SegmentId %A not found." segmentId

    member this.UpdateAllSortingResultsMutant (newEvals: sorterEval []) =
        newEvals |> Array.iter(this.UpdateSortingResultsMutant)

    member this.UpdateAllSortingResultsParent (newEvals: sorterEval []) =
        newEvals |> Array.iter(this.UpdateSortingResultParent)




module MutationSegmentSetResults = 
    let create segments = mutationSegmentSetResults.create segments