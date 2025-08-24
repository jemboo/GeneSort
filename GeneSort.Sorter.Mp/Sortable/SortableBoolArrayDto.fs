namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open MessagePack


[<MessagePackObject>]
type sortableBoolArrayDto = {
    [<Key(0)>] Values: bool[]
    [<Key(1)>] SortingWidth: int
}

module SortableBoolArrayDto =
    let toDtoBoolArray (sba: sortableBoolArray) : sortableBoolArrayDto =
        { Values = sba.Values; SortingWidth = int sba.SortingWidth }

    let fromDtoBoolArray (dto: sortableBoolArrayDto) : sortableBoolArray =
        sortableBoolArray.Create(dto.Values, UMX.tag<sortingWidth> dto.SortingWidth)

