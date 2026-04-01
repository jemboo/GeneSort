namespace GeneSort.SortingResults.Mp.Bins

open System
open MessagePack
open GeneSort.SortingResults.Bins
open GeneSort.SortingResults.Mp

[<MessagePackObject>]
type mutationSegmentEvalBinsDto = {
    [<Key(0)>]
    parentSortingResult: sortingResultDto
    [<Key(1)>]
    mutantSortingEvalBins: sortingEvalBinsDto
}

module MutationSegmentEvalBinsDto =

    /// Maps the Domain type to the DTO
    let fromDomain (domain: mutationSegmentEvalBins) : mutationSegmentEvalBinsDto =
        {
            parentSortingResult = SortingResultDto.fromDomain domain.ParentSortingResult
            mutantSortingEvalBins = SortingEvalBinsDto.fromDomain domain.MutantSortingEvalBins
        }

    /// Maps the DTO back to the Domain type
    let toDomain (dto: mutationSegmentEvalBinsDto) : mutationSegmentEvalBins =
        let parent = SortingResultDto.toDomain dto.parentSortingResult
        let mutantBins = SortingEvalBinsDto.toDomain dto.mutantSortingEvalBins
        
        MutationSegmentEvalBins.makeFromStorage parent mutantBins

    /// Full serialization to byte array
    let serialize (options: MessagePackSerializerOptions)
                  (domain: mutationSegmentEvalBins) : byte array =
        domain
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    /// Full deserialization from byte array
    let deserialize (options: MessagePackSerializerOptions)
                    (data: byte array) : mutationSegmentEvalBins =
        MessagePackSerializer.Deserialize<mutationSegmentEvalBinsDto>(data, options)
        |> toDomain