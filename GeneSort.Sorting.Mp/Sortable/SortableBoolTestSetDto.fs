namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableBoolTestSetDto = {
    Id: Guid
    SortableBoolTestDtos: sortableBoolTestDto[]
}

module SortableBoolTestSetDto =

    let fromDomain (sbts: sortableBoolTestSet) : sortableBoolTestSetDto =
        { Id = %sbts.Id
          SortableBoolTestDtos = sbts.sortableTests |> Array.map SortableBoolTestDto.fromDomain }

    let toDomain (dto: sortableBoolTestSetDto) : sortableBoolTestSet =
        sortableBoolTestSet.create
            (UMX.tag<sortableTestSetId> dto.Id)
            (dto.SortableBoolTestDtos |> Array.map SortableBoolTestDto.toDomain)