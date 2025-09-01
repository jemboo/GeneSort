namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps

[<MessagePackObject>]
type sorterSetEvalDto = {
    [<Key(0)>] SorterSetId: Guid
    [<Key(1)>] SorterTestsId: Guid
    [<Key(2)>] SorterEvals: sorterEvalDto array
}

module SorterSetEvalDto =
    let toSorterSetEvalDto (sorterSetEval: sorterSetEval) : sorterSetEvalDto =
        { 
            SorterSetId = %sorterSetEval.SorterSetId
            SorterTestsId = %sorterSetEval.SorterTestsId
            SorterEvals = sorterSetEval.SorterEvals |> Array.map SorterEvalDto.toSorterEvalDto
        }

    let fromSorterSetEvalDto (dto: sorterSetEvalDto) : sorterEval array =
        dto.SorterEvals |> Array.map SorterEvalDto.fromSorterEvalDto

