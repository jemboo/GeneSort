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
type CeBlockUsageDto = {
    [<Key(0)>]
    CeBlock: CeBlockDto
    [<Key(1)>]
    UseCounts: int array
}

module CeBlockUsageDto =
    let toCeBlockUsageDto (ceBlockUsage: ceBlockUsage) : CeBlockUsageDto =
        { 
            CeBlock = CeBlockDto.toCeBlockDto ceBlockUsage.CeBlock
            UseCounts = ceBlockUsage.UseCounts
        }

    let fromCeBlockUsageDto (dto: CeBlockUsageDto) : ceBlockUsage =
        ceBlockUsage.create 
            (CeBlockDto.fromCeBlockDto dto.CeBlock)
            dto.UseCounts
