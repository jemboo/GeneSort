namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableIntTestsSetDto = {
    Id: Guid
    SortableIntTestsDtos: sortableIntTestsDto[]
}

module SortableIntTestsSetDto =

    let fromDomain (sits: sortableIntTestsSet) : sortableIntTestsSetDto =
        { Id = %sits.Id
          SortableIntTestsDtos = sits.sortableTests |> Array.map SortableIntTestsDto.fromDomain }

    let toDomain (dto: sortableIntTestsSetDto) : sortableIntTestsSet =
        sortableIntTestsSet.create
            (UMX.tag<sortableTestsSetId> dto.Id)
            (dto.SortableIntTestsDtos |> Array.map SortableIntTestsDto.toDomain)