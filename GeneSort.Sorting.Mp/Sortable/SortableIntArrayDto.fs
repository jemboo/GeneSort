namespace GeneSort.Sorting.Mp.Sortable

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableIntArrayDto = {
    Values: int[]
    ChannelCount: int
    SymbolSetSize: int
}

module SortableIntArrayDto =

    let fromDomain (sia: sortableIntArray) : sortableIntArrayDto =
        { Values = sia.Values; ChannelCount = int sia.ChannelCount; SymbolSetSize = %sia.SymbolSetSize }

    let toDomain (dto: sortableIntArrayDto) : sortableIntArray =
        sortableIntArray.create(dto.Values, UMX.tag<channelCount> dto.ChannelCount, UMX.tag<symbolSetSize> dto.SymbolSetSize)
