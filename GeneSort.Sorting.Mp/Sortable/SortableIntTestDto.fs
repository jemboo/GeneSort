namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableIntTestDto = {
    Id: Guid
    SortingWidth: int
    SortableArrays: sortableIntArrayDto[]
}

module SortableIntTestDto =

    let fromDomain (sit: sortableIntTest) : sortableIntTestDto =
        { Id = %sit.Id
          SortingWidth = int sit.SortingWidth
          SortableArrays = sit.SortableIntArrays |> Array.map SortableIntArrayDto.fromDomain }

    let toDomain (dto: sortableIntTestDto) : sortableIntTest =
        sortableIntTest.create
            (UMX.tag<sortableTestId> dto.Id)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (dto.SortableArrays |> Array.map SortableIntArrayDto.toDomain)