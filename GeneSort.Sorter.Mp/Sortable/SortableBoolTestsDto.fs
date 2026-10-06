namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open MessagePack

[<MessagePackObject>]
type sortableBoolTestsDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SortableArrays: sortableBoolArrayDto[]
}

module SortableBoolTestsDto =

    let fromDomain (sbt: sortableBoolTests) : sortableBoolTestsDto =
        { Id = %sbt.Id
          SortingWidth = int sbt.SortingWidth
          SortableArrays = sbt.SortableBoolArrays |> Array.map SortableBoolArrayDto.fromDomain }

    let toDomain (dto: sortableBoolTestsDto) : sortableBoolTests =
        sortableBoolTests.create
            (UMX.tag<sorterTestId> dto.Id)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (dto.SortableArrays |> Array.map SortableBoolArrayDto.toDomain)
