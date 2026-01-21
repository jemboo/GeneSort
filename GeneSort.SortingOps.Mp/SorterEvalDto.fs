namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Sorter


[<MessagePackObject>]
type sorterEvalDto = {
    [<Key(0)>]
    SorterId: Guid
    [<Key(1)>]
    CeBlockEvalDto: ceBlockEvalDto
}

module SorterEvalDto =

    let toSorterEvalDto (sorterEval: sorterEval) : sorterEvalDto =
        { 
            SorterId = %sorterEval.SorterId
            CeBlockEvalDto = sorterEval.CeBlockEval |> CeBlockEvalDto.fromDomain
        }

    let fromSorterEvalDto (dto: sorterEvalDto) : sorterEval =
        sorterEval.create
            (UMX.tag<sorterId> dto.SorterId)  
            (CeBlockEvalDto.toDomain dto.CeBlockEvalDto)