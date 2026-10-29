namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableBoolTestsDto = {
    Id: Guid
    ChannelCount: int
    SortableArrays: sortableBoolArrayDto[]
}

module SortableBoolTestsDto =

    let fromDomain (sbt: sortableBinaryTests) : sortableBoolTestsDto =
        { Id = %sbt.Id
          ChannelCount = int sbt.ChannelCount
          SortableArrays = sbt.SortableBinaryArrays |> Array.map SortableBoolArrayDto.fromDomain }

    let toDomain (dto: sortableBoolTestsDto) : sortableBinaryTests =
        sortableBinaryTests.create
            (UMX.tag<sortableTestsId> dto.Id)
            (UMX.tag<channelCount> dto.ChannelCount)
            (dto.SortableArrays |> Array.map SortableBoolArrayDto.toDomain)