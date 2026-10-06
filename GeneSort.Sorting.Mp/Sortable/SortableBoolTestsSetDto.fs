namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableBoolTestsSetDto = {
    Id: Guid
    SortableBoolTestsDtos: sortableBoolTestsDto[]
}

module SortableBoolTestsSetDto =

    let fromDomain (sbts: sortableBoolTestsSet) : sortableBoolTestsSetDto =
        { Id = %sbts.Id
          SortableBoolTestsDtos = sbts.sortableTests |> Array.map SortableBoolTestsDto.fromDomain }

    let toDomain (dto: sortableBoolTestsSetDto) : sortableBoolTestsSet =
        sortableBoolTestsSet.create
            (UMX.tag<sortableTestsSetId> dto.Id)
            (dto.SortableBoolTestsDtos |> Array.map SortableBoolTestsDto.toDomain)