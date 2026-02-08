namespace GeneSort.Sorting.Mp.Sorter

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open MessagePack


[<MessagePackObject>]
type sorterSetDto = {
    [<Key(0)>]
    SorterSetId: Guid
    [<Key(1)>]
    Sorters: SorterDto array
}

module SorterSetDto =
    let fromDomain (sorterSet: sorterSet) : sorterSetDto =
        { SorterSetId = %sorterSet.Id
          Sorters = sorterSet.Sorters |> Array.map SorterDto.toSorterDto }

    let toDomain (dto: sorterSetDto) : sorterSet =
        if dto.SorterSetId = Guid.Empty then
            failwith "SorterSet ID must not be empty"
        if Array.isEmpty dto.Sorters then
            failwith "Sorter set must contain at least one sorter"
        sorterSet.create
            (UMX.tag<sorterSetId> dto.SorterSetId)
            (dto.Sorters |> Array.map SorterDto.fromSorterDto)