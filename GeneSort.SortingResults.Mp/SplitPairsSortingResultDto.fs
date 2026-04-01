namespace GeneSort.SortingResults.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps.Mp
open GeneSort.SortingResults
open GeneSort.Model.Sorting

[<MessagePackObject>]
type splitPairsSortingResultDto = {
    [<Key(0)>]
    SortingId: Guid
    [<Key(1)>]
    SorterEvalFirstFirst: sorterEvalDto option
    [<Key(2)>]
    SorterEvalFirstSecond: sorterEvalDto option
    [<Key(3)>]
    SorterEvalSecondFirst: sorterEvalDto option
    [<Key(4)>]
    SorterEvalSecondSecond: sorterEvalDto option
}

module SplitPairsSortingResultDto =

    let fromDomain (result: splitPairsSortingResult) : splitPairsSortingResultDto =
        {
            SortingId              = %result.SortingId
            SorterEvalFirstFirst   = result.SorterEvalFirstFirst   |> Option.map SorterEvalDto.toSorterEvalDto
            SorterEvalFirstSecond  = result.SorterEvalFirstSecond  |> Option.map SorterEvalDto.toSorterEvalDto
            SorterEvalSecondFirst  = result.SorterEvalSecondFirst  |> Option.map SorterEvalDto.toSorterEvalDto
            SorterEvalSecondSecond = result.SorterEvalSecondSecond |> Option.map SorterEvalDto.toSorterEvalDto
        }

    let toDomain (dto: splitPairsSortingResultDto) : splitPairsSortingResult =
        let result = splitPairsSortingResult.create (UMX.tag<sortingId> dto.SortingId)
        result.SorterEvalFirstFirst   <- dto.SorterEvalFirstFirst   |> Option.map SorterEvalDto.fromSorterEvalDto
        result.SorterEvalFirstSecond  <- dto.SorterEvalFirstSecond  |> Option.map SorterEvalDto.fromSorterEvalDto
        result.SorterEvalSecondFirst  <- dto.SorterEvalSecondFirst  |> Option.map SorterEvalDto.fromSorterEvalDto
        result.SorterEvalSecondSecond <- dto.SorterEvalSecondSecond |> Option.map SorterEvalDto.fromSorterEvalDto
        result