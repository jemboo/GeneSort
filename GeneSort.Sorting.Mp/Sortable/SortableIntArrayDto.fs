namespace GeneSort.Sorting.Mp.Sortable

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableIntArrayDto = {
    Values: int[]
    SortingWidth: int
    SymbolSetSize: int
}

module SortableIntArrayDto =

    let fromDomain (sia: sortableIntArray) : sortableIntArrayDto =
        { Values = sia.Values; SortingWidth = int sia.SortingWidth; SymbolSetSize = %sia.SymbolSetSize }

    let toDomain (dto: sortableIntArrayDto) : sortableIntArray =
        sortableIntArray.create(dto.Values, UMX.tag<sortingWidth> dto.SortingWidth, UMX.tag<symbolSetSize> dto.SymbolSetSize)