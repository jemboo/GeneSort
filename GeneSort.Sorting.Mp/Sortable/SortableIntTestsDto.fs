namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableIntTestsDto = {
    Id: Guid
    ChannelCount: int
    SortableArrays: sortableIntArrayDto[]
}

module SortableIntTestsDto =

    let fromDomain (sit: sortableIntTests) : sortableIntTestsDto =
        { Id = %sit.Id
          ChannelCount = int sit.ChannelCount
          SortableArrays = sit.SortableIntArrays |> Array.map SortableIntArrayDto.fromDomain }

    let toDomain (dto: sortableIntTestsDto) : sortableIntTests =
        sortableIntTests.create
            (UMX.tag<sortableTestsId> dto.Id)
            (UMX.tag<channelCount> dto.ChannelCount)
            (dto.SortableArrays |> Array.map SortableIntArrayDto.toDomain)