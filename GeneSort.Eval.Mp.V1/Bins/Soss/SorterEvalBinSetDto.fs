namespace GeneSort.Eval.Mp.V1.Bins.Soss

open System
open FSharp.UMX
open GeneSort.Eval.V1.Bins
open GeneSort.SortingOps.Mp
open GeneSort.Eval.V1.Sgd.Bins.Soss

type sorterEvalKeyDto = {
    CeCount: int
    StageLength: int
}

type sorterEvalBinDto = {
    SorterEvalKey: sorterEvalKeyDto
    SorterEvals: sorterEvalDto array
}

type sorterEvalBinSetDto = {
    SorterEvalBinSetId: Guid
    SorterSetEvalId: Guid
    SorterEvalBins: sorterEvalBinDto array
}

module SorterEvalKeyDto =

    let fromDomain (key: sorterEvalKey) : sorterEvalKeyDto = {
        CeCount = %key.CeCount
        StageLength = %key.StageLength
    }

    let toDomain (dto: sorterEvalKeyDto) : sorterEvalKey =
        sorterEvalKey.create (dto.CeCount |> UMX.tag) (dto.StageLength |> UMX.tag)

module SorterEvalBinDto =

    let fromDomain (bin: sorterEvalBin_Soss) : sorterEvalBinDto = {
        SorterEvalKey = SorterEvalKeyDto.fromDomain bin.SorterEvalKey
        SorterEvals = bin.SorterEvals |> Seq.map SorterEvalDto.fromDomain |> Seq.toArray
    }

    let toDomain (dto: sorterEvalBinDto) : sorterEvalBin_Soss =
        let key = SorterEvalKeyDto.toDomain dto.SorterEvalKey
        let evals = dto.SorterEvals |> Seq.map SorterEvalDto.toDomain
        sorterEvalBin_Soss.createWithSorterEvals evals key

module SorterEvalBinSetDto =

    let fromDomain (binSet: sorterEvalBinSet_Soss) : sorterEvalBinSetDto = {
        SorterEvalBinSetId = %binSet.SorterEvalBinSetId
        SorterSetEvalId = %binSet.SorterSetEvalId
        SorterEvalBins =
            binSet.Bins
            |> Map.values
            |> Seq.map SorterEvalBinDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterEvalBinSetDto) : sorterEvalBinSet_Soss =
        let id = dto.SorterEvalBinSetId |> UMX.tag
        let setEvalId = dto.SorterSetEvalId |> UMX.tag

        let bins =
            dto.SorterEvalBins
            |> Seq.map (fun binDto -> 
                let bin = SorterEvalBinDto.toDomain binDto
                (bin.SorterEvalKey, bin))
            |> Map.ofSeq

        sorterEvalBinSet_Soss.recreate id setEvalId bins