namespace GeneSort.Eval.Mp.V1.Bins.Soss

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.Mp.V1.Bins.Soss
open GeneSort.Eval.V1.Sgd.Bins.Soss

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

    let fromDomain (binsSet: sorterPoolBinsSet_Soss) : sorterPoolEvalBinsSetDto = {
        SorterPoolEvalBinsSetId = %binsSet.SorterPoolEvalBinsSetId
        SorterPoolSetId = %binsSet.SorterPoolSetId
        GenerationNumber = %binsSet.GenerationNumber
        SorterPoolEvalBins =
            binsSet.SorterPoolEvalBinsMap
            |> Map.values
            |> Seq.map SorterPoolEvalBinsDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterPoolEvalBinsSetDto) : sorterPoolBinsSet_Soss =
        let id = dto.SorterPoolEvalBinsSetId |> UMX.tag
        let poolSetId = dto.SorterPoolSetId |> UMX.tag
        let genNum = dto.GenerationNumber |> UMX.tag

        let evalBinsMap =
            dto.SorterPoolEvalBins
            |> Seq.map (fun binDto ->
                let poolBins = SorterPoolEvalBinsDto.toDomain binDto
                (poolBins.SorterPoolEvalBinsId, poolBins))
            |> Map.ofSeq

        sorterPoolBinsSet_Soss.recreate id poolSetId genNum evalBinsMap