namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Sorting


[<MessagePackObject>]
type sorterEvalDtoOld = {
    [<Key(0)>]
    SorterId: Guid
    [<Key(1)>]
    CeBlockEvalDto: ceBlockEvalDto
}

module SorterEvalDtoOld =

    let toSorterEvalDto (sorterEval: sorterEvalOld) : sorterEvalDtoOld =
        { 
            SorterId = %sorterEval.SorterId
            CeBlockEvalDto = sorterEval.CeBlockEval |> CeBlockEvalDto.fromDomain
        }

    let fromSorterEvalDto (dto: sorterEvalDtoOld) : sorterEvalOld =
        sorterEvalOld.create
            (UMX.tag<sorterId> dto.SorterId)  
            (CeBlockEvalDto.toDomain dto.CeBlockEvalDto)