namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Sorter
open GeneSort.Sorter.Sortable


[<MessagePackObject>]
type sorterTestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: sortableArrayDto[]
}

module SorterTestDto =
    let toDto (sorterTest: SorterTest) : sorterTestDto =
        { Id = %sorterTest.Id
          SortingWidth = %sorterTest.sortingWidth
          SortableArrays = sorterTest.sortableArrays |> Array.map SortableArrayDto.toDto }

    let fromDto (dto: sorterTestDto) : SorterTest =
        let sortableArrays = dto.SortableArrays |> Array.map SortableArrayDto.fromDto
        let id = UMX.tag<sorterTestId> dto.Id
        SorterTest.create id sortableArrays