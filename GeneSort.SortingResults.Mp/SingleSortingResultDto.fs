namespace GeneSort.SortingResults.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps.Mp
open GeneSort.SortingResults
open GeneSort.Model.Sorting

[<MessagePackObject>]
type singleSortingResultDto = {
    [<Key(0)>]
    SortingId: Guid
    [<Key(1)>]
    SorterEval: sorterEvalDto option
}

module SingleSortingResultDto =

    let fromDomain (result: singleSortingResult) : singleSortingResultDto =
        {
            SortingId  = %result.SortingId
            SorterEval = result.SorterEval |> Option.map SorterEvalDto.toSorterEvalDto
        }

    let toDomain (dto: singleSortingResultDto) : singleSortingResult =
        let result = singleSortingResult.create (UMX.tag<sortingId> dto.SortingId)
        result.SorterEval <- dto.SorterEval |> Option.map SorterEvalDto.fromSorterEvalDto
        result