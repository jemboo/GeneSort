namespace GeneSort.SortingResults

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type mutationSegmentSetEvals =
    private
        { 
          sortingMutationSegments : sortingMutationSegment []
          sortingSegmentResults: Dictionary<Guid<sortingMutationSegmentId>, mutationSegmentEvals>
          // Routing lookups
          mutantSorterToSegmentMap: Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>
          parentSorterToSegmentMap: Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>
        }
    with
    static member create (sortingMutationSegments: sortingMutationSegment []) =
        let sortingSegmentResults = Dictionary<Guid<sortingMutationSegmentId>, mutationSegmentEvals>()
        let mutantMap = Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>()
        let parentMap = Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>()

        for segment in sortingMutationSegments do
            // 1. Create the segment result object
            let segmentResult = mutationSegmentEvals.create segment
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

    member this.MutantSorterToSegmentMap with get() = this.mutantSorterToSegmentMap
    member this.ParentSorterToSegmentMap with get() = this.parentSorterToSegmentMap
    member this.SortingMutationSegments with get() = this.sortingMutationSegments
    member this.SortingSegmentResults with get() = this.sortingSegmentResults

    /// Automatically routes the evaluation to the correct Mutant result
    member this.UpdateSortingResultsMutant (newEval: sorterEval) =
        match this.mutantSorterToSegmentMap.TryGetValue(newEval.SorterId) with
        | true, segmentId -> 
            this.sortingSegmentResults.[segmentId].UpdateSortingEvalSetMapMutants newEval
        | false, _ -> failwithf "Mutant SorterId %A not found in any segment." newEval.SorterId

    /// Automatically routes the evaluation to the correct Parent result
    member this.UpdateSortingResultParent (newEval: sorterEval) =
        match this.parentSorterToSegmentMap.TryGetValue(newEval.SorterId) with
        | true, segmentId -> 
            this.sortingSegmentResults.[segmentId].UpdateSortingEvalMapParent newEval
        | false, _ -> failwithf "Parent SorterId %A not found in any segment." newEval.SorterId

    member this.GetSegmentResults (segmentId: Guid<sortingMutationSegmentId>) =
        match this.sortingSegmentResults.TryGetValue(segmentId) with
        | true, segmentResults -> segmentResults
        | false, _ -> failwithf "SegmentId %A not found." segmentId

    member this.UpdateAllSortingResultsMutant (newEvals: sorterEval []) =
        newEvals |> Array.iter(this.UpdateSortingResultsMutant)

    member this.UpdateAllSortingResultsParent (newEvals: sorterEval []) =
        newEvals |> Array.iter(this.UpdateSortingResultParent)

    member this.GetAllParentSorterEvals () : (sorterEval * modelSetTag) seq =
        this.sortingSegmentResults.Values
        |> Seq.collect (fun segmentResult -> segmentResult.GetAllTaggedParentSorterEvals())
        
    // modelSuperSetTag.Id is the sortingId of the sorting that the mutants were made from.
    member this.GetAllMutantSorterEvals () : (sorterEval * modelSuperSetTag) seq =
        this.sortingSegmentResults
        |> Seq.collect (
            fun segmentResult -> 
                let sortingMutationSegment = this.SortingMutationSegments |> Array.find(fun seg -> seg.Id = segmentResult.Key)
                segmentResult.Value.GetAllTaggedMutantSorterEvals()
                |> Seq.map (fun (eval, modelSetTag) ->
                    eval, modelSuperSetTag.create sortingMutationSegment.ParentSortingId modelSetTag)
            )



module MutationSegmentSetEvals = 
    
    let load 
           (sortingMutationSegments : sortingMutationSegment []) 
           (sortingSegmentResults: Dictionary<Guid<sortingMutationSegmentId>, mutationSegmentEvals>) 
           (mutantSorterToSegmentMap: Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>) 
           (parentSorterToSegmentMap: Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>) 
                : mutationSegmentSetEvals =
           {
                sortingMutationSegments = sortingMutationSegments
                sortingSegmentResults = sortingSegmentResults
                mutantSorterToSegmentMap = mutantSorterToSegmentMap
                parentSorterToSegmentMap = parentSorterToSegmentMap
           }
