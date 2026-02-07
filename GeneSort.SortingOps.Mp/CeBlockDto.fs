namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Sorting.Mp.Sorter
open GeneSort.Sorting


[<MessagePackObject>]
type ceBlockDto = {
    [<Key(0)>]
    CeBlockId: Guid
    [<Key(1)>]
    SortingWidth: int
    [<Key(2)>]
    Ces: CeDto array

}

module CeBlockDto =
    let toCeBlockDto (ceBlock: ceBlock) : ceBlockDto =
        { 
            CeBlockId = %ceBlock.CeBlockId; 
            Ces = ceBlock.CeArray |> Array.map CeDto.fromDomain;
            SortingWidth = %ceBlock.SortingWidth }

    let fromCeBlockDto (dto: ceBlockDto) : ceBlock =
        ceBlock.create 
                (dto.CeBlockId |> UMX.tag<ceBlockId>) 
                (dto.SortingWidth |> UMX.tag<sortingWidth>)
                (dto.Ces |> Array.map CeDto.toDomain)
