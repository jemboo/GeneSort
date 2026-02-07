namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Component


[<MessagePackObject>]
type sorterModelEvalDto = {
    [<Key(0)>]
    SorterId: Guid
    [<Key(1)>]
    CeBlockEvalDto: ceBlockEvalDto
}

module SorterModelEvalDto =

    let toSorterEvalDto (sorterEval: sorterEval) : sorterModelEvalDto =
        { 
            SorterId = %sorterEval.SorterId
            CeBlockEvalDto = sorterEval.CeBlockEval |> CeBlockEvalDto.fromDomain
        }

    let fromSorterEvalDto (dto: sorterModelEvalDto) : sorterEval =
        sorterEval.create
            (UMX.tag<sorterId> dto.SorterId)  
            (CeBlockEvalDto.toDomain dto.CeBlockEvalDto)