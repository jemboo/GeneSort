namespace GeneSort.Sorting.Mp.Sorter

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type SorterDto = {
    SorterId: Guid
    Width: int
    Ces: ceDto array
}

module SorterDto =
    let toSorterDto (sorter: sorter) : SorterDto =
        { SorterId = %sorter.SorterId
          Width = %sorter.ChannelCount
          Ces = sorter.Ces |> Array.map CeDto.fromDomain }

    let fromSorterDto (dto: SorterDto) : sorter =
        if dto.SorterId = Guid.Empty then
            failwith "Sorter ID must not be empty"
        if dto.Width < 1 then
            failwith "Width must be at least 1"
        sorter.create
            (UMX.tag<sorterId> dto.SorterId)
            (UMX.tag<channelCount> dto.Width)
            (dto.Ces |> Array.map CeDto.toDomain)