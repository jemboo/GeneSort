namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable


[<MessagePackObject>]
type sorterTestSetDto =
    | Ints of sorterIntTestSetDto
    | Bools of sorterBoolTestSetDto

module SorterTestSetDto =
    let toDto (sorterTestSet: sorterTestSet) : sorterTestSetDto =
        match sorterTestSet with
        | sorterTestSet.Ints intTestSet -> Ints (SorterIntTestSetDto.toDto intTestSet)
        | sorterTestSet.Bools boolTestSet -> Bools (SorterBoolTestSetDto.toDto boolTestSet)

    let fromDto (dto: sorterTestSetDto) : sorterTestSet =
        match dto with
        | Ints intTestSetDto -> sorterTestSet.Ints (SorterIntTestSetDto.fromDto intTestSetDto)
        | Bools boolTestSetDto -> sorterTestSet.Bools (SorterBoolTestSetDto.fromDto boolTestSetDto)