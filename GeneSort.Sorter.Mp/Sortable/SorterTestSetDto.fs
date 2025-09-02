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
    let fromDomain (sorterTestSet: sorterTestSet) : sorterTestSetDto =
        match sorterTestSet with
        | sorterTestSet.Ints intTestSet -> Ints (SorterIntTestSetDto.fromDomain intTestSet)
        | sorterTestSet.Bools boolTestSet -> Bools (SorterBoolTestSetDto.fromDomain boolTestSet)

    let toDomain (dto: sorterTestSetDto) : sorterTestSet =
        match dto with
        | Ints intTestSetDto -> sorterTestSet.Ints (SorterIntTestSetDto.toDomain intTestSetDto)
        | Bools boolTestSetDto -> sorterTestSet.Bools (SorterBoolTestSetDto.toDomain boolTestSetDto)