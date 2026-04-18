namespace GeneSort.SortingResults.Mp

open System
open System.Collections.Generic
open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorting
open GeneSort.SortingResults

[<MessagePackObject>]
type mutationSegmentSetResultsDto = {
    [<Key(0)>]
    SortingMutationSegments: sortingMutationSegmentDto []
    
    [<Key(1)>]
    SortingSegmentResults: IDictionary<string, mutationSegmentResultsDto>
    
    [<Key(2)>]
    MutantSorterToSegmentMap: IDictionary<string, string>
    
    [<Key(3)>]
    ParentSorterToSegmentMap: IDictionary<string, string>
}

module MutationSegmentSetResultsDto =

    let fromDomain (domain: mutationSegmentSetEvals) : mutationSegmentSetResultsDto =
        // 1. Map Mutation Segments
        let segmentsDto = 
            domain.SortingMutationSegments 
            |> Array.map SortingMutationSegmentDto.fromDomain

        // 2. Map Segment Results (Dictionary<Guid<Id>, mutationSegmentResults>)
        let segmentResultsDto = Dictionary<string, mutationSegmentResultsDto>()
        for kvp in domain.SortingSegmentResults do
            segmentResultsDto.Add((%kvp.Key).ToString(), MutationSegmentResultsDto.fromDomain kvp.Value)

        // 3. Map Mutant Routing (Dictionary<Guid<sorterId>, Guid<segmentId>>)
        let mutantMapDto = Dictionary<string, string>()
        for kvp in domain.MutantSorterToSegmentMap do
            mutantMapDto.Add((%kvp.Key).ToString(), (%kvp.Value).ToString())

        // 4. Map Parent Routing (Dictionary<Guid<sorterId>, Guid<segmentId>>)
        let parentMapDto = Dictionary<string, string>()
        for kvp in domain.ParentSorterToSegmentMap do
            parentMapDto.Add((%kvp.Key).ToString(), (%kvp.Value).ToString())

        {
            SortingMutationSegments = segmentsDto
            SortingSegmentResults = segmentResultsDto
            MutantSorterToSegmentMap = mutantMapDto
            ParentSorterToSegmentMap = parentMapDto
        }

    let toDomain (dto: mutationSegmentSetResultsDto) : mutationSegmentSetEvals =
        // 1. Rebuild Segments
        let segments = 
            dto.SortingMutationSegments 
            |> Array.map SortingMutationSegmentDto.toDomain

        // 2. Rebuild Segment Results Dictionary
        let segmentResults = Dictionary<Guid<sortingMutationSegmentId>, mutationSegmentEvals>()
        for kvp in dto.SortingSegmentResults do
            let segmentId = Guid.Parse(kvp.Key) |> UMX.tag<sortingMutationSegmentId>
            segmentResults.Add(segmentId, MutationSegmentResultsDto.toDomain kvp.Value)

        // 3. Rebuild Mutant Routing Dictionary
        let mutantMap = Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>()
        for kvp in dto.MutantSorterToSegmentMap do
            let sorterId = Guid.Parse(kvp.Key) |> UMX.tag<sorterId>
            let segmentId = Guid.Parse(kvp.Value) |> UMX.tag<sortingMutationSegmentId>
            mutantMap.Add(sorterId, segmentId)

        // 4. Rebuild Parent Routing Dictionary
        let parentMap = Dictionary<Guid<sorterId>, Guid<sortingMutationSegmentId>>()
        for kvp in dto.ParentSorterToSegmentMap do
            let sorterId = Guid.Parse(kvp.Key) |> UMX.tag<sorterId>
            let segmentId = Guid.Parse(kvp.Value) |> UMX.tag<sortingMutationSegmentId>
            parentMap.Add(sorterId, segmentId)

        // Use the 'load' function to bypass expensive regeneration logic
        MutationSegmentSetEvals.load
            segments
            segmentResults
            mutantMap
            parentMap