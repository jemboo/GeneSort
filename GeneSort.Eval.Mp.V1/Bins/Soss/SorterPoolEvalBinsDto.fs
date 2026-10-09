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

type sorterPoolEvalBinsDto = {
    SorterPoolEvalBinsId: Guid
    SorterPoolId: Guid
    SorterEvalBins: sorterEvalBinDto array
}

// ---------------------------------------------------------------------
// 2. Conversion Module
// ---------------------------------------------------------------------

module SorterPoolEvalBinsDto =

    let fromDomain (poolBins: sorterPoolBins_Soss) : sorterPoolEvalBinsDto = {
        SorterPoolEvalBinsId = %poolBins.SorterPoolEvalBinsId
        SorterPoolId = %poolBins.SorterPoolId
        SorterEvalBins =
            poolBins.Bins
            |> Map.values
            |> Seq.map SorterEvalBinDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterPoolEvalBinsDto) : sorterPoolBins_Soss =
        let id = dto.SorterPoolEvalBinsId |> UMX.tag
        let poolId = dto.SorterPoolId |> UMX.tag

        let bins =
            dto.SorterEvalBins
            |> Seq.map (fun binDto -> 
                let bin = SorterEvalBinDto.toDomain binDto
                (bin.SorterEvalKey, bin))
            |> Map.ofSeq

        sorterPoolBins_Soss.recreate id poolId bins