namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open MessagePack


[<MessagePackObject>]
type sortableIntTestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: sortableIntArrayDto[]
}

module SortableIntTestDto =

    let fromDomain (sit: sortableIntTests) : sortableIntTestDto =
        { Id = %sit.Id
          SortingWidth = int sit.SortingWidth
          SortableArrays = sit.SortableIntArrays |> Array.map SortableIntArrayDto.fromDomain }

    let toDomain (dto: sortableIntTestDto) : sortableIntTests =
        sortableIntTests.create
            (UMX.tag<sortableTestsId> dto.Id)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (dto.SortableArrays |> Array.map SortableIntArrayDto.toDomain)