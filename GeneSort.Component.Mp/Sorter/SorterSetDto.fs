namespace GeneSort.Component.Mp.Sorter

open System
open FSharp.UMX
open GeneSort.Component
open GeneSort.Component.Sorter
open MessagePack


[<MessagePackObject>]
type sorterSetDto = {
    [<Key(0)>]
    SorterSetId: Guid
    [<Key(1)>]
    CeLength: int
    [<Key(2)>]
    Sorters: SorterDto array
}

module SorterSetDto =
    let fromDomain (sorterSet: sorterSet) : sorterSetDto =
        { SorterSetId = %sorterSet.Id
          CeLength = %sorterSet.CeLength
          Sorters = sorterSet.Sorters |> Array.map SorterDto.toSorterDto }

    let toDomain (dto: sorterSetDto) : sorterSet =
        if dto.SorterSetId = Guid.Empty then
            failwith "SorterSet ID must not be empty"
        if Array.isEmpty dto.Sorters then
            failwith "Sorter set must contain at least one sorter"
        sorterSet.create
            (UMX.tag<sorterSetId> dto.SorterSetId)
            (UMX.tag<ceLength> dto.CeLength)
            (dto.Sorters |> Array.map SorterDto.fromSorterDto)