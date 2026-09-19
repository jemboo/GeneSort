namespace GeneSort.Eval.Mp.V1.Bins

open System
open FSharp.UMX
open GeneSort.Eval.V1.Bins
open GeneSort.SortingOps.Mp

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

    let fromDomain (bin: sorterEvalBin) : sorterEvalBinDto = {
        SorterEvalKey = SorterEvalKeyDto.fromDomain bin.SorterEvalKey
        SorterEvals = bin.SorterEvals |> Seq.map SorterEvalDto.fromDomain |> Seq.toArray
    }

    let toDomain (dto: sorterEvalBinDto) : sorterEvalBin =
        let key = SorterEvalKeyDto.toDomain dto.SorterEvalKey
        let evals = dto.SorterEvals |> Seq.map SorterEvalDto.toDomain
        sorterEvalBin.createWithSorterEvals evals key

module SorterEvalBinSetDto =

    let fromDomain (binSet: sorterEvalBinSet) : sorterEvalBinSetDto = {
        SorterEvalBinSetId = %binSet.SorterEvalBinSetId
        SorterSetEvalId = %binSet.SorterSetEvalId
        SorterEvalBins =
            binSet.Bins
            |> Map.values
            |> Seq.map SorterEvalBinDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterEvalBinSetDto) : sorterEvalBinSet =
        let id = dto.SorterEvalBinSetId |> UMX.tag
        let setEvalId = dto.SorterSetEvalId |> UMX.tag

        let bins =
            dto.SorterEvalBins
            |> Seq.map (fun binDto -> 
                let bin = SorterEvalBinDto.toDomain binDto
                (bin.SorterEvalKey, bin))
            |> Map.ofSeq

        sorterEvalBinSet.recreate id setEvalId bins