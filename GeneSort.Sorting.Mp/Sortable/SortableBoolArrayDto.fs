namespace GeneSort.Sorting.Mp.Sortable

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableBoolArrayDto = {
    Values: bool[]
    SortingWidth: int
}

module SortableBoolArrayDto =

    let fromDomain (sba: sortableBoolArray) : sortableBoolArrayDto =
        { Values = sba.Values; SortingWidth = int sba.SortingWidth }

    let toDomain (dto: sortableBoolArrayDto) : sortableBoolArray =
        sortableBoolArray.create(dto.Values, UMX.tag<sortingWidth> dto.SortingWidth)