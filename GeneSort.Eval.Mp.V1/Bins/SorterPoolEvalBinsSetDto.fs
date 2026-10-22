namespace GeneSort.Eval.Mp.V1.Bins

open System
open FSharp.UMX
open MessagePack
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Bins.Standard
open GeneSort.Eval.Mp.V1.Bins

// ---------------------------------------------------------------------
// 1. DTO Definition
// ---------------------------------------------------------------------

type sorterPoolEvalBinsSetDto = {
    SorterPoolEvalBinsSetId: Guid
    SorterPoolSetId: Guid
    GenerationNumber: int
    SorterPoolEvalBins: sorterPoolEvalBinsDto array
}

// ---------------------------------------------------------------------
// 2. Conversion Module
// ---------------------------------------------------------------------

module SorterPoolEvalBinsSetDto =

    let fromDomain (binsSet: sorterPoolBinsSet_Standard) : sorterPoolEvalBinsSetDto = {
        SorterPoolEvalBinsSetId = %binsSet.SorterPoolEvalBinsSetId
        SorterPoolSetId = %binsSet.SorterPoolSetId
        GenerationNumber = %binsSet.GenerationNumber
        SorterPoolEvalBins =
            binsSet.SorterPoolEvalBinsMap
            |> Map.values
            |> Seq.map SorterPoolEvalBinsDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterPoolEvalBinsSetDto) : sorterPoolBinsSet_Standard =
        let id = dto.SorterPoolEvalBinsSetId |> UMX.tag
        let poolSetId = dto.SorterPoolSetId |> UMX.tag
        let genNum = dto.GenerationNumber |> UMX.tag

        let evalBinsMap =
            dto.SorterPoolEvalBins
            |> Seq.map (fun binDto ->
                let poolBins = SorterPoolEvalBinsDto.toDomain binDto
                (poolBins.SorterPoolEvalBinsId, poolBins))
            |> Map.ofSeq

        sorterPoolBinsSet_Standard.recreate id poolSetId genNum evalBinsMap