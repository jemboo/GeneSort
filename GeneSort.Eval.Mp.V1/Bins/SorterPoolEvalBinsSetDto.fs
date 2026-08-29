namespace GeneSort.Eval.Mp.V1.Bins

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.Mp.V1.Bins


// ---------------------------------------------------------------------
// 1. DTO Definition
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterPoolEvalBinsSetDto = {
    [<Key(0)>] SorterPoolEvalBinsSetId: Guid
    [<Key(1)>] SorterPoolSetId: Guid
    [<Key(2)>] GenerationNumber: int
    [<Key(3)>] SorterPoolEvalBins: sorterPoolEvalBinsDto array
}

// ---------------------------------------------------------------------
// 2. Conversion Module
// ---------------------------------------------------------------------

module SorterPoolEvalBinsSetDto =

    let fromDomain (binsSet: sorterPoolBinsSet) : sorterPoolEvalBinsSetDto = {
        SorterPoolEvalBinsSetId = %binsSet.SorterPoolEvalBinsSetId
        SorterPoolSetId = %binsSet.SorterPoolSetId
        GenerationNumber = %binsSet.GenerationNumber
        SorterPoolEvalBins =
            binsSet.SorterPoolEvalBinsMap
            |> Map.values
            |> Seq.map SorterPoolEvalBinsDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterPoolEvalBinsSetDto) : sorterPoolBinsSet =
        let id = dto.SorterPoolEvalBinsSetId |> UMX.tag
        let poolSetId = dto.SorterPoolSetId |> UMX.tag
        let genNum = dto.GenerationNumber |> UMX.tag

        let evalBinsMap =
            dto.SorterPoolEvalBins
            |> Seq.map (fun binDto ->
                let poolBins = SorterPoolEvalBinsDto.toDomain binDto
                (poolBins.SorterPoolEvalBinsId, poolBins))
            |> Map.ofSeq

        sorterPoolBinsSet.recreate id poolSetId genNum evalBinsMap