namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

[<MessagePackObject>]
type sortableIntTestSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortableIntTestDtos: sortableIntTestDto[]
}

module SortableIntTestSetDto =

    let fromDomain (sits: sortableIntTestSet) : sortableIntTestSetDto =
        { Id = %sits.Id
          SortableIntTestDtos = sits.sortableTests |> Array.map SortableIntTestDto.fromDomain }

    let toDomain (dto: sortableIntTestSetDto) : sortableIntTestSet =
        sortableIntTestSet.create
            (UMX.tag<sortableTestSetId> dto.Id)
            (dto.SortableIntTestDtos |> Array.map SortableIntTestDto.toDomain)
