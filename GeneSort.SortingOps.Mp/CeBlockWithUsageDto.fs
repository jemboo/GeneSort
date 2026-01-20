namespace GeneSort.SortingOps.Mp

open MessagePack
open GeneSort.SortingOps
open FSharp.UMX
open GeneSort.Sorter


[<MessagePackObject>]
type ceBlockWithUsageDto = {
    [<Key(0)>]
    CeBlock: ceBlockDto
    [<Key(1)>]
    UseCounts: int array    
    [<Key(2)>]
    UnsortedCount: int
}

module CeBlockWithUsageDto =

    let toCeBlockUsageDto (ceBlockUsage: ceBlockWithUsage) : ceBlockWithUsageDto =
        { 
            CeBlock = CeBlockDto.toCeBlockDto ceBlockUsage.CeBlock
            UseCounts = ceBlockUsage.UseCounts.ToArray()
            UnsortedCount = %ceBlockUsage.UnsortedCount
        }

    let fromCeBlockUsageDto (dto: ceBlockWithUsageDto) : ceBlockWithUsage =
        ceBlockWithUsage.create 
            (CeBlockDto.fromCeBlockDto dto.CeBlock)
            (ceUseCounts.CreateFromArray dto.UseCounts)
            (dto.UnsortedCount |> UMX.tag<sortableCount>)
