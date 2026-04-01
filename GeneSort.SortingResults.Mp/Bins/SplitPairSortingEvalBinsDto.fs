namespace GeneSort.SortingResults.Mp.Bins

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingResults.Bins
open GeneSort.SortingResults

[<MessagePackObject>]
type splitPairSortingEvalBinsDto = {
    [<Key(0)>]
    sortingEvalBinsId: Guid
    [<Key(1)>]
    sorterEvalBinsFirstFirst: sorterEvalBinsDto
    [<Key(2)>]
    sorterEvalBinsFirstSecond: sorterEvalBinsDto
    [<Key(3)>]
    sorterEvalBinsSecondFirst: sorterEvalBinsDto
    [<Key(4)>]
    sorterEvalBinsSecondSecond: sorterEvalBinsDto
}

module SplitPairSortingEvalBinsDto =

    /// Maps the Domain type to the DTO
    let fromDomain (domain: splitPairSortingEvalBins) : splitPairSortingEvalBinsDto =
        {
            sortingEvalBinsId = %domain.SortingEvalBinsId
            sorterEvalBinsFirstFirst = SorterEvalBinsDto.fromDomain domain.SorterEvalBinsFirstFirst
            sorterEvalBinsFirstSecond = SorterEvalBinsDto.fromDomain domain.SorterEvalBinsFirstSecond
            sorterEvalBinsSecondFirst = SorterEvalBinsDto.fromDomain domain.SorterEvalBinsSecondFirst
            sorterEvalBinsSecondSecond = SorterEvalBinsDto.fromDomain domain.SorterEvalBinsSecondSecond
        }

    /// Maps the DTO back to the Domain type using the fromStorage helper
    let toDomain (dto: splitPairSortingEvalBinsDto) : splitPairSortingEvalBins =
        SplitPairSortingEvalBins.fromStorage
            (UMX.tag<sortingEvalBinsId> dto.sortingEvalBinsId)
            (SorterEvalBinsDto.toDomain dto.sorterEvalBinsFirstFirst)
            (SorterEvalBinsDto.toDomain dto.sorterEvalBinsFirstSecond)
            (SorterEvalBinsDto.toDomain dto.sorterEvalBinsSecondFirst)
            (SorterEvalBinsDto.toDomain dto.sorterEvalBinsSecondSecond)

    let serialize (options: MessagePackSerializerOptions)
                  (domain: splitPairSortingEvalBins) : byte array =
        domain
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    let deserialize (options: MessagePackSerializerOptions)
                    (data: byte array) : splitPairSortingEvalBins =
        MessagePackSerializer.Deserialize<splitPairSortingEvalBinsDto>(data, options)
        |> toDomain