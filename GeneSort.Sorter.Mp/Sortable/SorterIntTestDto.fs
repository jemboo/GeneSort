namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open MessagePack


[<MessagePackObject>]
type sorterIntTestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: sortableIntArrayDto[]
}

module SorterIntTestDto =
    let toDto (sit: sorterIntTests) : sorterIntTestDto =
        { Id = %sit.Id
          SortingWidth = int sit.SortingWidth
          SortableArrays = sit.SortableArrays |> Array.map SortableIntArrayDto.toDtoIntArray }

    let fromDto (dto: sorterIntTestDto) : sorterIntTests =
        sorterIntTests.create
            (UMX.tag<sorterTestIsd> dto.Id)
            (dto.SortableArrays |> Array.map SortableIntArrayDto.fromDtoIntArray)