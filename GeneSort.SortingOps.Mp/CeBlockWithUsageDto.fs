namespace GeneSort.SortingOps.Mp

open MessagePack
open GeneSort.SortingOps


[<MessagePackObject>]
type ceBlockWithUsageDto = {
    [<Key(0)>]
    CeBlock: ceBlockDto
    [<Key(1)>]
    UseCounts: int array
}

module CeBlockWithUsageDto =

    let toCeBlockUsageDto (ceBlockUsage: ceBlockWithUsage) : ceBlockWithUsageDto =
        { 
            CeBlock = CeBlockDto.toCeBlockDto ceBlockUsage.CeBlock
            UseCounts = ceBlockUsage.UseCounts.ToArray()
        }

    let fromCeBlockUsageDto (dto: ceBlockWithUsageDto) : ceBlockWithUsage =
        ceBlockWithUsage.create 
            (CeBlockDto.fromCeBlockDto dto.CeBlock)
            (ceUseCounts.CreateFromArray dto.UseCounts)
