namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Mp.Sorter



[<MessagePackObject>]
type SorterEvalDto = {
    [<Key(0)>]
    SorterId: Guid
    [<Key(1)>]
    SortingWidth: int
    [<Key(2)>]
    CeBlockUsage: CeBlockUsageDto
}

module SorterEvalDto =
    let toSorterEvalDto (sorterEval: sorterEval) : SorterEvalDto =
        { 
            SorterId = %sorterEval.SorterId
            SortingWidth = %sorterEval.SortingWidth
            CeBlockUsage = CeBlockUsageDto.toCeBlockUsageDto sorterEval.CeBlockUsage
        }

    let fromSorterEvalDto (dto: SorterEvalDto) : sorterEval =
        if dto.SorterId = Guid.Empty then
            failwith "Sorter ID must not be empty"
        if dto.SortingWidth < 1 then
            failwith "SortingWidth must be at least 1"
        sorterEval.create
            (UMX.tag<sorterId> dto.SorterId)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (CeBlockUsageDto.fromCeBlockUsageDto dto.CeBlockUsage)