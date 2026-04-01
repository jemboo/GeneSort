namespace GeneSort.SortingResults.Mp.Bins

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.SortingResults.Bins
open GeneSort.SortingResults

[<MessagePackObject>]
type singleSortingEvalBinsDto = {
    [<Key(0)>]
    sortingEvalBinsId: Guid
    [<Key(1)>]
    sorterEvalBins: sorterEvalBinsDto
}

module SingleSortingEvalBinsDto =

    /// Maps the Domain type to the DTO
    let fromDomain (domain: singleSortingEvalBins) : singleSortingEvalBinsDto =
        {
            sortingEvalBinsId = %domain.SortingEvalBinsId
            sorterEvalBins = SorterEvalBinsDto.fromDomain domain.SorterEvalBins
        }

    /// Maps the DTO back to the Domain type
    let toDomain (dto: singleSortingEvalBinsDto) : singleSortingEvalBins =
        let id = UMX.tag<sortingEvalBinsId> dto.sortingEvalBinsId
        let bins = SorterEvalBinsDto.toDomain dto.sorterEvalBins
 
        SingleSortingEvalBins.fromStorage id bins


    let serialize (options: MessagePackSerializerOptions)
                  (domain: singleSortingEvalBins) : byte array =
        domain
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    let deserialize (options: MessagePackSerializerOptions)
                    (data: byte array) : singleSortingEvalBins =
        MessagePackSerializer.Deserialize<singleSortingEvalBinsDto>(data, options)
        |> toDomain