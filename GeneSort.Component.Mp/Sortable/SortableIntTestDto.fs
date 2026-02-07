namespace GeneSort.Component.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Component
open GeneSort.Component.Sortable
open MessagePack


[<MessagePackObject>]
type sortableIntTestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: sortableIntArrayDto[]
}

module SortableIntTestDto =

    let fromDomain (sit: sortableIntTest) : sortableIntTestDto =
        { Id = %sit.Id
          SortingWidth = int sit.SortingWidth
          SortableArrays = sit.SortableIntArrays |> Array.map SortableIntArrayDto.fromDomain }

    let toDomain (dto: sortableIntTestDto) : sortableIntTest =
        sortableIntTest.create
            (UMX.tag<sorterTestId> dto.Id)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (dto.SortableArrays |> Array.map SortableIntArrayDto.toDomain)