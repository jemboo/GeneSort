namespace GeneSort.Eval.Mp.V1.Bins

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Bins
open GeneSort.SortingOps.Mp

// ---------------------------------------------------------------------
// 1. Supporting Bin DTOs
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterEvalKeyDto = {
    [<Key(0)>] CeCount: int
    [<Key(1)>] StageLength: int
}

[<MessagePackObject>]
type sorterEvalBinDto = {
    [<Key(0)>] SorterEvalKey: sorterEvalKeyDto
    [<Key(1)>] SorterEvals: sorterEvalDto array
}

// ---------------------------------------------------------------------
// 2. Main SorterEvalBinSet DTO
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterEvalBinSetDto = {
    [<Key(0)>] SorterEvalBinSetId: Guid
    [<Key(1)>] SortableTestId: Guid
    [<Key(2)>] SorterEvalBins: sorterEvalBinDto array
}

// ---------------------------------------------------------------------
// 3. Conversion Module
// ---------------------------------------------------------------------

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
        SortableTestId = %binSet.SortableTestId
        SorterEvalBins =
            binSet.Bins
            |> Map.values
            |> Seq.map SorterEvalBinDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterEvalBinSetDto) : sorterEvalBinSet =
        let id = dto.SorterEvalBinSetId |> UMX.tag
        let testId = dto.SortableTestId |> UMX.tag

        let evals =
            dto.SorterEvalBins
            |> Seq.collect (fun binDto -> binDto.SorterEvals |> Seq.map SorterEvalDto.toDomain)

        sorterEvalBinSet.createFromSorterEvals id testId evals
