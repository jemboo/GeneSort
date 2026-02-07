namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.Component
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp

[<MessagePackObject>]
type sorterModelSetEvalDto = {
    [<Key(0)>]
    SorterSetEvalId: Guid
    [<Key(1)>]
    SorterSetId: Guid
    [<Key(2)>]
    SorterTestsId: Guid
    [<Key(3)>]
    SorterEvals: sorterEvalDto array
}

module SorterSetEvalDto =

    let fromDomain (sorterSetEval: sorterModelSetEval) : sorterModelSetEvalDto =
        { 
            SorterSetEvalId = %sorterSetEval.SorterModelSetEvalId
            SorterSetId = %sorterSetEval.SorterSetId
            SorterTestsId = %sorterSetEval.SorterTestId
            SorterEvals = sorterSetEval.SorterEvals |> Array.map SorterEvalDto.toSorterEvalDto
        }

    let toDomain (dto: sorterModelSetEvalDto) : sorterModelSetEval =
        if dto.SorterSetEvalId = Guid.Empty then
            failwith "SorterSetEvalId must not be empty"
        if dto.SorterSetId = Guid.Empty then
            failwith "SorterSetId must not be empty"
        if dto.SorterTestsId = Guid.Empty then
            failwith "SorterTestsId must not be empty"
        sorterModelSetEval.create
            (UMX.tag<sorterSetId> dto.SorterSetId)
            (UMX.tag<sorterTestId> dto.SorterTestsId)
            (dto.SorterEvals |> Array.map SorterEvalDto.fromSorterEvalDto)
