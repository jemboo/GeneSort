namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp

[<MessagePackObject>]
type sorterSetEvalDto = {
    [<Key(0)>]
    SorterSetEvalId: Guid
    [<Key(1)>]
    SorterSetId: Guid
    [<Key(2)>]
    SorterTestsId: Guid
    [<Key(3)>]
    CeLength: int
    [<Key(4)>]
    SorterEvals: sorterEvalDto array
}

module SorterSetEvalDto =

    let fromDomain (sorterSetEval: sorterSetEval) : sorterSetEvalDto =
        { 
            SorterSetEvalId = %sorterSetEval.SorterSetEvalId
            SorterSetId = %sorterSetEval.SorterSetId
            SorterTestsId = %sorterSetEval.SorterTestId
            SorterEvals = sorterSetEval.SorterEvals |> Array.map SorterEvalDto.toSorterEvalDto
            CeLength = %sorterSetEval.CeLength
        }

    let toDomain (dto: sorterSetEvalDto) : sorterSetEval =
        if dto.SorterSetEvalId = Guid.Empty then
            failwith "SorterSetEvalId must not be empty"
        if dto.SorterSetId = Guid.Empty then
            failwith "SorterSetId must not be empty"
        if dto.SorterTestsId = Guid.Empty then
            failwith "SorterTestsId must not be empty"
        sorterSetEval.create
            (UMX.tag<sorterSetId> dto.SorterSetId)
            (UMX.tag<sorterTestId> dto.SorterTestsId)
            (dto.SorterEvals |> Array.map SorterEvalDto.fromSorterEvalDto)
            (UMX.tag<ceLength> dto.CeLength)
