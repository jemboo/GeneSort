namespace GeneSort.Sorting.Mp.Sortable

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableBoolArrayDto = {
    Values: bool[]
    ChannelCount: int
}

module SortableBoolArrayDto =

    let fromDomain (sba: sortableBoolArray) : sortableBoolArrayDto =
        { Values = sba.Values; ChannelCount = int sba.ChannelCount }

    let toDomain (dto: sortableBoolArrayDto) : sortableBoolArray =
        sortableBoolArray.create(dto.Values, UMX.tag<channelCount> dto.ChannelCount)
