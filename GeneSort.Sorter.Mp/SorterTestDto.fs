namespace GeneSort.Sorter.Mp

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open MessagePack



[<MessagePackObject>]
type SorterTestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: SortableArrayDto[]
}

module SorterTestDto =
    let toDto (sorterTest: SorterTest) : SorterTestDto =
        { Id = %sorterTest.Id
          SortingWidth = %sorterTest.sortingWidth
          SortableArrays = sorterTest.sortableArrays |> Array.map SortableArrayDto.toDto }

    let fromDto (dto: SorterTestDto) : SorterTest =
        let sortableArrays = dto.SortableArrays |> Array.map SortableArrayDto.fromDto
        let id = UMX.tag<sorterTestId> dto.Id
        SorterTest.create id sortableArrays