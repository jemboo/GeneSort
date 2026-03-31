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
    SortingResultMapParent: sortingResultMapDto
    
    [<Key(2)>]
    SortingResultSetMapMutants: sortingResultSetMapDto
}

module MutationSegmentResultsDto =

    /// Converts the domain object to a serializable DTO
    let toDto (domain: mutationSegmentResults) : mutationSegmentResultsDto =
        {
            SortingMutationSegment = SortingMutationSegmentDto.fromDomain domain.SortingMutationSegment
            SortingResultMapParent = SortingResultMapDto.toDto domain.SortingResultMapParent
            SortingResultSetMapMutants = SortingResultSetMapDto.toDto domain.SortingResultSetMapMutants
        }

    /// Reconstructs the domain object from the DTO
    /// Note: This relies on the internal 'create' logic of mutationSegmentResults
    let fromDto (dto: mutationSegmentResultsDto) : mutationSegmentResults =
        let segment = SortingMutationSegmentDto.toDomain dto.SortingMutationSegment
        let parentMap = SortingResultMapDto.fromDto dto.SortingResultMapParent
        let mutantsMap = SortingResultSetMapDto.fromDto dto.SortingResultSetMapMutants

        MutationSegmentResults.load segment parentMap mutantsMap

    /// Helper for direct serialization
    let serialize (options: MessagePackSerializerOptions) (results: mutationSegmentResults) =
        results |> toDto |> fun d -> MessagePackSerializer.Serialize(d, options)

    /// Helper for direct deserialization
    let deserialize (options: MessagePackSerializerOptions) (bytes: byte array) =
        MessagePackSerializer.Deserialize<mutationSegmentResultsDto>(bytes, options) |> fromDto