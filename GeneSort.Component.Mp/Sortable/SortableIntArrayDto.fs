namespace GeneSort.Component.Mp.Sortable

open FSharp.UMX
open GeneSort.Component
open GeneSort.Component.Sortable
open MessagePack


[<MessagePackObject>]
type sortableIntArrayDto = {
    [<Key(0)>] Values: int[]
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SymbolSetSize: int
}

module SortableIntArrayDto =

    let fromDomain (sia: sortableIntArray) : sortableIntArrayDto =
        { Values = sia.Values; SortingWidth = int sia.SortingWidth; SymbolSetSize = %sia.SymbolSetSize }

    let toDomain (dto: sortableIntArrayDto) : sortableIntArray =
        sortableIntArray.create(dto.Values, UMX.tag<sortingWidth> dto.SortingWidth, UMX.tag<symbolSetSize> dto.SymbolSetSize)

