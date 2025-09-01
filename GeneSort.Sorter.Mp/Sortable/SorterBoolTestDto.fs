namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open MessagePack

[<MessagePackObject>]
type sorterBoolTestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: sortableBoolArrayDto[]
}

module SorterBoolTestDto =
    let toDto (sbt: sorterBoolTests) : sorterBoolTestDto =
        { Id = %sbt.Id
          SortingWidth = int sbt.SortingWidth
          SortableArrays = sbt.sortableBoolArrays |> Array.map SortableBoolArrayDto.toDtoBoolArray }

    let fromDto (dto: sorterBoolTestDto) : sorterBoolTests =
        sorterBoolTests.create
            (UMX.tag<sorterTestsId> dto.Id)
            (dto.SortableArrays |> Array.map SortableBoolArrayDto.fromDtoBoolArray)
