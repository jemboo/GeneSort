namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Sorter


[<MessagePackObject>]
type sorterEvalDto = {
    [<Key(0)>]
    SorterId: Guid
    [<Key(1)>]
    SorterTestsId: Guid
    [<Key(2)>]
    SortingWidth: int
    [<Key(3)>]
    CeBlockUsage: ceBlockWithUsageDto
    [<Key(4)>]
    UnsortedCount: int
}

module SorterEvalDto =

    let toSorterEvalDto (sorterEval: sorterEval) : sorterEvalDto =
        { 
            SorterId = %sorterEval.SorterId
            SorterTestsId = %sorterEval.SorterTestId
            SortingWidth = %sorterEval.SortingWidth
            CeBlockUsage = CeBlockWithUsageDto.toCeBlockUsageDto sorterEval.CeBlockUsage
            UnsortedCount = %sorterEval.UnsortedCount
        }

    let fromSorterEvalDto (dto: sorterEvalDto) : sorterEval =
        if dto.SorterId = Guid.Empty then
            failwith "Sorter ID must not be empty"
        if dto.SortingWidth < 1 then
            failwith "SortingWidth must be at least 1"
        sorterEval.create
            (UMX.tag<sorterId> dto.SorterId)  
            (UMX.tag<sorterTestId> dto.SorterTestsId)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (CeBlockWithUsageDto.fromCeBlockUsageDto dto.CeBlockUsage)
            (dto.UnsortedCount |> UMX.tag<sortableCount>)