namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable


[<MessagePackObject>]
type sortableTestsSetDto =
    | Ints of sortableIntTestsSetDto
    | Bools of sortableBoolTestsSetDto

module SortableTestsSetDto =

    let fromDomain (sorterTestSet: sortableTestsSet) : sortableTestsSetDto =
        match sorterTestSet with
        | sortableTestsSet.Ints intTestSet -> Ints (SortableIntTestsSetDto.fromDomain intTestSet)
        | sortableTestsSet.Bools boolTestSet -> Bools (SortableBoolTestsSetDto.fromDomain boolTestSet)

    let toDomain (dto: sortableTestsSetDto) : sortableTestsSet =
        match dto with
        | Ints intTestSetDto -> sortableTestsSet.Ints (SortableIntTestsSetDto.toDomain intTestSetDto)
        | Bools boolTestSetDto -> sortableTestsSet.Bools (SortableBoolTestsSetDto.toDomain boolTestSetDto)