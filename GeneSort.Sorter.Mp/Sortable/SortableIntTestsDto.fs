namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open MessagePack


[<MessagePackObject>]
type sortableIntTestsDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: sortableIntArrayDto[]
}

module SortableIntTestsDto =

    let fromDomain (sit: sortableIntTests) : sortableIntTestsDto =
        { Id = %sit.Id
          SortingWidth = int sit.SortingWidth
          SortableArrays = sit.SortableIntArrays |> Array.map SortableIntArrayDto.fromDomain }

    let toDomain (dto: sortableIntTestsDto) : sortableIntTests =
        sortableIntTests.create
            (UMX.tag<sorterTestId> dto.Id)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (dto.SortableArrays |> Array.map SortableIntArrayDto.toDomain)