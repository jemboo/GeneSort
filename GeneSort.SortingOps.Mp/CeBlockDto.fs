namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Sorter.Mp.Sorter


[<MessagePackObject>]
type ceBlockDto = {
    [<Key(0)>]
    Ces: CeDto array
    [<Key(1)>]
    CeBlockId: Guid
}

module CeBlockDto =
    let toCeBlockDto (ceBlock: ceBlock) : ceBlockDto =
        { CeBlockId = %ceBlock.CeBlockId; Ces = ceBlock.CeArray |> Array.map CeDto.fromDomain }

    let fromCeBlockDto (dto: ceBlockDto) : ceBlock =
        ceBlock.create (dto.CeBlockId |> UMX.tag<ceBlockId>) (dto.Ces |> Array.map CeDto.toDomain)
