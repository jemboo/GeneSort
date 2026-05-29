namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp

[<MessagePackObject>]
type sorterSetEvalDtoOld = {
    [<Key(0)>]
    SorterSetEvalId: Guid
    [<Key(1)>]
    SorterSetId: Guid
    [<Key(2)>]
    SorterTestsId: Guid
    [<Key(3)>]
    SorterEvals: sorterEvalDtoOld array
}

module SorterSetEvalDtoOld =

    let fromDomain (sorterSetEval: sorterSetEvalOld) : sorterSetEvalDtoOld =
        { 
            SorterSetEvalId = %sorterSetEval.SorterSetEvalId
            SorterSetId = %sorterSetEval.SorterSetId
            SorterTestsId = %sorterSetEval.SorterTestId
            SorterEvals = sorterSetEval.SorterEvals |> Array.map SorterEvalDtoOld.toSorterEvalDto
        }

    let toDomain (dto: sorterSetEvalDtoOld) : sorterSetEvalOld =
        if dto.SorterSetEvalId = Guid.Empty then
            failwith "SorterSetEvalId must not be empty"
        if dto.SorterSetId = Guid.Empty then
            failwith "SorterSetId must not be empty"
        if dto.SorterTestsId = Guid.Empty then
            failwith "SorterTestsId must not be empty"
        sorterSetEvalOld.create
            (UMX.tag<sorterSetEvalId> dto.SorterSetEvalId)
            (UMX.tag<sorterSetId> dto.SorterSetId)
            (UMX.tag<sortableTestId> dto.SorterTestsId)
            (dto.SorterEvals |> Array.map SorterEvalDtoOld.fromSorterEvalDto)
