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
    Sorters: SorterDto array
}

module SorterSetDto =
    let fromDomain (sorterSet: SorterSet) : SorterSetDto =
        { SorterSetId = %sorterSet.SorterSetId
          Sorters = sorterSet.Sorters |> Array.map SorterDto.toSorterDto }

    let fromSorterSetDto (dto: SorterSetDto) : SorterSet =
        if dto.SorterSetId = Guid.Empty then
            failwith "SorterSet ID must not be empty"
        if Array.isEmpty dto.Sorters then
            failwith "Sorter set must contain at least one sorter"
        SorterSet.create
            (UMX.tag<sorterSetId> dto.SorterSetId)
            (dto.Sorters |> Array.map SorterDto.fromSorterDto)