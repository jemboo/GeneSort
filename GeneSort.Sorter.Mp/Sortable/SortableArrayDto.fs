namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open MessagePack


[<MessagePackObject>]
type sortableArrayDto = {
    [<Key(0)>] Type: string
    [<Key(1)>] IntArray: sortableIntArrayDto option
    [<Key(2)>] BoolArray: sortableBoolArrayDto option
}

module SortableArrayDto =

    let toDto (sortable: SortableArray) : sortableArrayDto =
        match sortable with
        | Ints intArray -> { Type = "Ints"; IntArray = Some (SortableIntArrayDto.toDtoIntArray intArray); BoolArray = None }
        | Bools boolArray -> { Type = "Bools"; IntArray = None; BoolArray = Some (SortableBoolArrayDto.toDtoBoolArray boolArray) }

    let fromDto (dto: sortableArrayDto) : SortableArray =
        match dto.Type with
        | "Ints" ->
            match dto.IntArray with
            | Some intDto -> SortableArray.createInts intDto.Values (UMX.tag<sortingWidth> intDto.SortingWidth) (UMX.tag<symbolSetSize> intDto.SymbolSetSize)
            | None -> invalidArg "IntArray" "IntArray must be present for Type 'Ints'."
        | "Bools" ->
            match dto.BoolArray with
            | Some boolDto -> SortableArray.createBools boolDto.Values (UMX.tag<sortingWidth> boolDto.SortingWidth)
            | None -> invalidArg "BoolArray" "BoolArray must be present for Type 'Bools'."
        | _ -> invalidArg "Type" $"Invalid Type: {dto.Type}. Expected 'Ints' or 'Bools'."
