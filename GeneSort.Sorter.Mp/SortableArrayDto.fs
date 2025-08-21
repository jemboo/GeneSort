namespace GeneSort.Sorter.Mp

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open MessagePack


[<MessagePackObject>]
type sortableBoolArrayDto = {
    [<Key(0)>] Values: bool[]
    [<Key(1)>] SortingWidth: int
}

[<MessagePackObject>]
type sortableIntArrayDto = {
    [<Key(0)>] Values: int[]
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] SymbolSetSize: int
}

[<MessagePackObject>]
type SortableArrayDto = {
    [<Key(0)>] Type: string
    [<Key(1)>] IntArray: sortableIntArrayDto option
    [<Key(2)>] BoolArray: sortableBoolArrayDto option
}

module SortableArrayDto =
    let toDtoBoolArray (sba: sortableBoolArray) : sortableBoolArrayDto =
        { Values = sba.Values; SortingWidth = int sba.SortingWidth }

    let fromDtoBoolArray (dto: sortableBoolArrayDto) : sortableBoolArray =
        sortableBoolArray.Create(dto.Values, UMX.tag<sortingWidth> dto.SortingWidth)

    let toDtoIntArray (sia: sortableIntArray) : sortableIntArrayDto =
        { Values = sia.Values; SortingWidth = int sia.SortingWidth; SymbolSetSize = %sia.SymbolSetSize }

    let fromDtoIntArray (dto: sortableIntArrayDto) : sortableIntArray =
        sortableIntArray.Create(dto.Values, UMX.tag<sortingWidth> dto.SortingWidth, UMX.tag<symbolSetSize> dto.SymbolSetSize)

    let toDto (sortable: SortableArray) : SortableArrayDto =
        match sortable with
        | Ints intArray -> { Type = "Ints"; IntArray = Some (toDtoIntArray intArray); BoolArray = None }
        | Bools boolArray -> { Type = "Bools"; IntArray = None; BoolArray = Some (toDtoBoolArray boolArray) }

    let fromDto (dto: SortableArrayDto) : SortableArray =
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
