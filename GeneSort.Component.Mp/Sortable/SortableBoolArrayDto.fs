namespace GeneSort.Component.Mp.Sortable

open FSharp.UMX
open GeneSort.Component
open GeneSort.Component.Sortable
open MessagePack


[<MessagePackObject>]
type sortableBoolArrayDto = {
    [<Key(0)>] Values: bool[]
    [<Key(1)>] SortingWidth: int
}

module SortableBoolArrayDto =

    let fromDomain (sba: sortableBoolArray) : sortableBoolArrayDto =
        { Values = sba.Values; SortingWidth = int sba.SortingWidth }

    let toDomain (dto: sortableBoolArrayDto) : sortableBoolArray =
        sortableBoolArray.create(dto.Values, UMX.tag<sortingWidth> dto.SortingWidth)

