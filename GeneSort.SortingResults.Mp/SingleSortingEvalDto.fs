namespace GeneSort.SortingResults.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps.Mp
open GeneSort.SortingResults
open GeneSort.Model.Sorting

[<MessagePackObject>]
type singleSortingEvalDto = {
    [<Key(0)>]
    SortingId: Guid
    [<Key(1)>]
    SorterEval: sorterEvalDto option
}

module SingleSortingEvalDto =

    let fromDomain (result: singleSortingEval) : singleSortingEvalDto =
        {
            SortingId  = %result.SortingId
            SorterEval = result.SorterEval |> Option.map SorterEvalDto.toSorterEvalDto
        }

    let toDomain (dto: singleSortingEvalDto) : singleSortingEval =
        let result = singleSortingEval.create (UMX.tag<sortingId> dto.SortingId)
        result.SorterEval <- dto.SorterEval |> Option.map SorterEvalDto.fromSorterEvalDto
        result