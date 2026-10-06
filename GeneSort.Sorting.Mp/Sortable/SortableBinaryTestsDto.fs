namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableBoolTestsDto = {
    Id: Guid
    SortingWidth: int
    SortableArrays: sortableBoolArrayDto[]
}

module SortableBoolTestsDto =

    let fromDomain (sbt: sortableBinaryTests) : sortableBoolTestsDto =
        { Id = %sbt.Id
          SortingWidth = int sbt.SortingWidth
          SortableArrays = sbt.SortableBinaryArrays |> Array.map SortableBoolArrayDto.fromDomain }

    let toDomain (dto: sortableBoolTestsDto) : sortableBinaryTests =
        sortableBinaryTests.create
            (UMX.tag<sortableTestsId> dto.Id)
            (UMX.tag<sortingWidth> dto.SortingWidth)
            (dto.SortableArrays |> Array.map SortableBoolArrayDto.toDomain)