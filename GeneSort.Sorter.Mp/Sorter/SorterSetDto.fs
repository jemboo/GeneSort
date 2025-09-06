namespace GeneSort.Sorter.Mp.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open MessagePack


[<MessagePackObject>]
type SorterSetDto = {
    [<Key(0)>]
    SorterSetId: Guid
    [<Key(1)>]
    CeLength: int
    [<Key(2)>]
    Sorters: SorterDto array
}

module SorterSetDto =
    let fromDomain (sorterSet: sorterSet) : SorterSetDto =
        { SorterSetId = %sorterSet.SorterSetId
          CeLength = %sorterSet.CeLength
          Sorters = sorterSet.Sorters |> Array.map SorterDto.toSorterDto }

    let fromSorterSetDto (dto: SorterSetDto) : sorterSet =
        if dto.SorterSetId = Guid.Empty then
            failwith "SorterSet ID must not be empty"
        if Array.isEmpty dto.Sorters then
            failwith "Sorter set must contain at least one sorter"
        sorterSet.create
            (UMX.tag<sorterSetId> dto.SorterSetId)
            (UMX.tag<ceLength> dto.CeLength)
            (dto.Sorters |> Array.map SorterDto.fromSorterDto)