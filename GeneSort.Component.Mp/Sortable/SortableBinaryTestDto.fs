namespace GeneSort.Component.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Component
open GeneSort.Component.Sortable
open MessagePack

[<MessagePackObject>]
type sortableBoolTestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: sortableBoolArrayDto[]
}

module SortableBoolTestDto =

    let fromDomain (sbt: sortableBinaryTest) : sortableBoolTestDto =
        { Id = %sbt.Id
          SortingWidth = int sbt.SortingWidth
          SortableArrays = sbt.SortableBinaryArrays |> Array.map SortableBoolArrayDto.fromDomain }

    let toDomain (dto: sortableBoolTestDto) : sortableBinaryTest =
        sortableBinaryTest.create
            (UMX.tag<sorterTestId> dto.Id)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (dto.SortableArrays |> Array.map SortableBoolArrayDto.toDomain)
