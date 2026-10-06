namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

[<MessagePackObject>]
type sortableIntTestsSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortableIntTestsDtos: sortableIntTestsDto[]
}

module SortableIntTestsSetDto =

    let fromDomain (sits: sortableIntTestsSet) : sortableIntTestsSetDto =
        { Id = %sits.Id
          SortableIntTestsDtos = sits.sortableTests |> Array.map SortableIntTestsDto.fromDomain }

    let toDomain (dto: sortableIntTestsSetDto) : sortableIntTestsSet =
        sortableIntTestsSet.create
            (UMX.tag<sortableTestsSetId> dto.Id)
            (dto.SortableIntTestsDtos |> Array.map SortableIntTestsDto.toDomain)
