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
type sorterPoolEvalBinsDto = {
    [<Key(0)>] SorterPoolEvalBinsId: Guid
    [<Key(1)>] SorterPoolId: Guid
    [<Key(2)>] SorterEvalBins: sorterEvalBinDto array
}

// ---------------------------------------------------------------------
// 2. Conversion Module
// ---------------------------------------------------------------------

module SorterPoolEvalBinsDto =

    let fromDomain (poolBins: sorterPoolEvalBins) : sorterPoolEvalBinsDto = {
        SorterPoolEvalBinsId = %poolBins.SorterPoolEvalBinsId
        SorterPoolId = %poolBins.SorterPoolId
        SorterEvalBins =
            poolBins.Bins
            |> Map.values
            |> Seq.map SorterEvalBinDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterPoolEvalBinsDto) : sorterPoolEvalBins =
        let id = dto.SorterPoolEvalBinsId |> UMX.tag
        let poolId = dto.SorterPoolId |> UMX.tag

        let bins =
            dto.SorterEvalBins
            |> Seq.map (fun binDto -> 
                let bin = SorterEvalBinDto.toDomain binDto
                (bin.SorterEvalKey, bin))
            |> Map.ofSeq

        sorterPoolEvalBins.recreate id poolId bins