namespace GeneSort.SortingOps.Mp

open MessagePack
open GeneSort.SortingOps
open GeneSort.Sorter.Mp.Sorter


[<MessagePackObject>]
type CeBlockDto = {
    [<Key(0)>]
    Ces: CeDto array
}

module CeBlockDto =
    let toCeBlockDto (ceBlock: ceBlock) : CeBlockDto =
        { Ces = ceBlock.CeArray |> Array.map CeDto.toCeDto }

    let fromCeBlockDto (dto: CeBlockDto) : ceBlock =
        ceBlock.create (dto.Ces |> Array.map CeDto.fromCeDto)
