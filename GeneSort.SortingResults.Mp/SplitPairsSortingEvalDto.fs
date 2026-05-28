namespace GeneSort.SortingResults.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps.Mp
open GeneSort.SortingResults
open GeneSort.Model.Sorting

[<MessagePackObject>]
type splitPairsSortingEvalDto = {
    [<Key(0)>]
    SortingId: Guid
    [<Key(1)>]
    SorterEvalFirstFirst: sorterEvalDtoOld option
    [<Key(2)>]
    SorterEvalFirstSecond: sorterEvalDtoOld option
    [<Key(3)>]
    SorterEvalSecondFirst: sorterEvalDtoOld option
    [<Key(4)>]
    SorterEvalSecondSecond: sorterEvalDtoOld option
}

module SplitPairsSortingEvalDto =

    let fromDomain (result: splitPairsSortingEval) : splitPairsSortingEvalDto =
        {
            SortingId              = %result.SortingId
            SorterEvalFirstFirst   = result.SorterEvalFirstFirst   |> Option.map SorterEvalDtoOld.toSorterEvalDto
            SorterEvalFirstSecond  = result.SorterEvalFirstSecond  |> Option.map SorterEvalDtoOld.toSorterEvalDto
            SorterEvalSecondFirst  = result.SorterEvalSecondFirst  |> Option.map SorterEvalDtoOld.toSorterEvalDto
            SorterEvalSecondSecond = result.SorterEvalSecondSecond |> Option.map SorterEvalDtoOld.toSorterEvalDto
        }

    let toDomain (dto: splitPairsSortingEvalDto) : splitPairsSortingEval =
        let result = splitPairsSortingEval.create (UMX.tag<sortingId> dto.SortingId)
        result.SorterEvalFirstFirst   <- dto.SorterEvalFirstFirst   |> Option.map SorterEvalDtoOld.fromSorterEvalDto
        result.SorterEvalFirstSecond  <- dto.SorterEvalFirstSecond  |> Option.map SorterEvalDtoOld.fromSorterEvalDto
        result.SorterEvalSecondFirst  <- dto.SorterEvalSecondFirst  |> Option.map SorterEvalDtoOld.fromSorterEvalDto
        result.SorterEvalSecondSecond <- dto.SorterEvalSecondSecond |> Option.map SorterEvalDtoOld.fromSorterEvalDto
        result