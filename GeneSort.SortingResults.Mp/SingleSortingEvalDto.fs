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
    SorterEval: sorterEvalDtoOld option
}

module SingleSortingEvalDto =

    let fromDomain (result: singleSortingEval) : singleSortingEvalDto =
        {
            SortingId  = %result.SortingId
            SorterEval = result.SorterEval |> Option.map SorterEvalDtoOld.toSorterEvalDto
        }

    let toDomain (dto: singleSortingEvalDto) : singleSortingEval =
        let result = singleSortingEval.create (UMX.tag<sortingId> dto.SortingId)
        result.SorterEval <- dto.SorterEval |> Option.map SorterEvalDtoOld.fromSorterEvalDto
        result