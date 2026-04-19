namespace GeneSort.SortingResults.Mp

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorting
open GeneSort.SortingResults

[<MessagePackObject>]
type mutationSegmentResultsDto = {
    [<Key(0)>]
    SortingMutationSegment: sortingMutationSegmentDto
    
    [<Key(1)>]
    SortingEvalMapParent: sortingEvalMapDto
    
    [<Key(2)>]
    SortingEvalSetMapMutants: sortingEvalSetMapDto
}

module MutationSegmentResultsDto =

    /// Converts the domain object to a serializable DTO
    let fromDomain (domain: mutationSegmentEvals) : mutationSegmentResultsDto =
        {
            SortingMutationSegment = SortingMutationSegmentDto.fromDomain domain.SortingMutationSegment
            SortingEvalMapParent = SortingEvalMapDto.fromDomain domain.SortingEvalMapParent
            SortingEvalSetMapMutants = SortingEvalSetMapDto.fromDomain domain.SortingEvalSetMapMutants
        }

    /// Reconstructs the domain object from the DTO
    /// Note: This relies on the internal 'create' logic of mutationSegmentResults
    let toDomain (dto: mutationSegmentResultsDto) : mutationSegmentEvals =
        let segment = SortingMutationSegmentDto.toDomain dto.SortingMutationSegment
        let parentMap = SortingEvalMapDto.toDomain dto.SortingEvalMapParent
        let mutantsMap = SortingEvalSetMapDto.toDomain dto.SortingEvalSetMapMutants

        MutationSegmentEvals.load segment parentMap mutantsMap

    /// Helper for direct serialization
    let serialize (options: MessagePackSerializerOptions) (results: mutationSegmentEvals) =
        results |> fromDomain |> fun d -> MessagePackSerializer.Serialize(d, options)

    /// Helper for direct deserialization
    let deserialize (options: MessagePackSerializerOptions) (bytes: byte array) =
        MessagePackSerializer.Deserialize<mutationSegmentResultsDto>(bytes, options) |> toDomain