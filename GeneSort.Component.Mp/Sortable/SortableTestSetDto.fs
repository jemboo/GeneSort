namespace GeneSort.Component.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Component
open GeneSort.Component.Sortable


[<MessagePackObject>]
type sortableTestSetDto =
    | Ints of sortableIntTestSetDto
    | Bools of sortableBoolTestSetDto

module SortableTestSetDto =
    let fromDomain (sorterTestSet: sortableTestSet) : sortableTestSetDto =
        match sorterTestSet with
        | sortableTestSet.Ints intTestSet -> Ints (SortableIntTestSetDto.fromDomain intTestSet)
        | sortableTestSet.Bools boolTestSet -> Bools (SortableBoolTestSetDto.fromDomain boolTestSet)

    let toDomain (dto: sortableTestSetDto) : sortableTestSet =
        match dto with
        | Ints intTestSetDto -> sortableTestSet.Ints (SortableIntTestSetDto.toDomain intTestSetDto)
        | Bools boolTestSetDto -> sortableTestSet.Bools (SortableBoolTestSetDto.toDomain boolTestSetDto)