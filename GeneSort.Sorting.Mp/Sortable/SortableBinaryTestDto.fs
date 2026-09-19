namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableBoolTestDto = {
    Id: Guid
    SortingWidth: int
    SortableArrays: sortableBoolArrayDto[]
}

module SortableBoolTestDto =

    let fromDomain (sbt: sortableBinaryTest) : sortableBoolTestDto =
        { Id = %sbt.Id
          SortingWidth = int sbt.SortingWidth
          SortableArrays = sbt.SortableBinaryArrays |> Array.map SortableBoolArrayDto.fromDomain }

    let toDomain (dto: sortableBoolTestDto) : sortableBinaryTest =
        sortableBinaryTest.create
            (UMX.tag<sortableTestId> dto.Id)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (dto.SortableArrays |> Array.map SortableBoolArrayDto.toDomain)