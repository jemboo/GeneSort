namespace GeneSort.SortingOps.Mp

open MessagePack
open GeneSort.SortingOps


[<MessagePackObject>]
type ceBlockUsageDto = {
    [<Key(0)>]
    CeBlock: ceBlockDto
    [<Key(1)>]
    UseCounts: int array
}

module CeBlockUsageDto =
    let toCeBlockUsageDto (ceBlockUsage: ceBlockUsage) : ceBlockUsageDto =
        { 
            CeBlock = CeBlockDto.toCeBlockDto ceBlockUsage.CeBlock
            UseCounts = ceBlockUsage.UseCounts
        }

    let fromCeBlockUsageDto (dto: ceBlockUsageDto) : ceBlockUsage =
        ceBlockUsage.create 
            (CeBlockDto.fromCeBlockDto dto.CeBlock)
            dto.UseCounts
