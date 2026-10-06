namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable


[<MessagePackObject>]
type sortableTestsSetDto =
    | Ints of sortableIntTestSetDto
    | Bools of sortableBoolTestSetDto

module SortableTestsSetDto =
    let fromDomain (sorterTestSet: sortableTestsSet) : sortableTestsSetDto =
        match sorterTestSet with
        | sortableTestsSet.Ints intTestSet -> Ints (SortableIntTestSetDto.fromDomain intTestSet)
        | sortableTestsSet.Bools boolTestSet -> Bools (SortableBoolTestSetDto.fromDomain boolTestSet)

    let toDomain (dto: sortableTestsSetDto) : sortableTestsSet =
        match dto with
        | Ints intTestSetDto -> sortableTestsSet.Ints (SortableIntTestSetDto.toDomain intTestSetDto)
        | Bools boolTestSetDto -> sortableTestsSet.Bools (SortableBoolTestSetDto.toDomain boolTestSetDto)