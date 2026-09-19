namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableIntTestSetDto = {
    Id: Guid
    SortableIntTestDtos: sortableIntTestDto[]
}

module SortableIntTestSetDto =

    let fromDomain (sits: sortableIntTestSet) : sortableIntTestSetDto =
        { Id = %sits.Id
          SortableIntTestDtos = sits.sortableTests |> Array.map SortableIntTestDto.fromDomain }

    let toDomain (dto: sortableIntTestSetDto) : sortableIntTestSet =
        sortableIntTestSet.create
            (UMX.tag<sortableTestSetId> dto.Id)
            (dto.SortableIntTestDtos |> Array.map SortableIntTestDto.toDomain)