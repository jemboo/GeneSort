namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open MessagePack

[<MessagePackObject>]
type sortableBoolTestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: sortableBoolArrayDto[]
}

module SortableBoolTestDto =

    let fromDomain (sbt: sortableBoolTests) : sortableBoolTestDto =
        { Id = %sbt.Id
          SortingWidth = int sbt.SortingWidth
          SortableArrays = sbt.sortableBoolArrays |> Array.map SortableBoolArrayDto.fromDomain }

    let toDomain (dto: sortableBoolTestDto) : sortableBoolTests =
        sortableBoolTests.create
            (UMX.tag<sortableTestsId> dto.Id)
            (dto.SortableArrays |> Array.map SortableBoolArrayDto.toDomain)
