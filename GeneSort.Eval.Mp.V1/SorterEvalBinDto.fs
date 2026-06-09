namespace GeneSort.Eval.Mp.V1

open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Eval.V1.Bins.Old

[<MessagePackObject>]
type sorterEvalBinV1Dto = {
    [<Key(0)>] Key: sorterEvalKeyDto
    [<Key(1)>] Scores: sorterScoreDto array
}

[<MessagePackObject>]
type sorterEvalBinDto =
    | [<Key(0)>] V1 of sorterEvalBinV1Dto
    | [<Key(1)>] Unknown

module SorterEvalBinDto =

    let toV1Dto (domain: sorterEvalBinV1Old) : sorterEvalBinV1Dto =
        {
            Key = SorterEvalKeyDto.toDto domain.SorterEvalKey
            // Projects ResizeArray to a standard array for serialization
            Scores = domain.Scores |> Seq.map SorterScoreDto.toDto |> Seq.toArray
        }

    let fromV1Dto (dto: sorterEvalBinV1Dto) : sorterEvalBinV1Old =
        let scores = dto.Scores |> Seq.map SorterScoreDto.fromDto
        sorterEvalBinV1Old.createWithScores scores (SorterEvalKeyDto.fromDto dto.Key)


    let toDto (domain: sorterEvalBinOld) : sorterEvalBinDto =
        match domain with
        | sorterEvalBinOld.V1 v1 -> sorterEvalBinDto.V1 (toV1Dto v1)
        | sorterEvalBinOld.Unknown -> sorterEvalBinDto.Unknown


    let fromDto (dto: sorterEvalBinDto) : sorterEvalBinOld =
        match dto with
        | sorterEvalBinDto.V1 v1Dto -> sorterEvalBinOld.V1 (fromV1Dto v1Dto)
        | sorterEvalBinDto.Unknown -> sorterEvalBinOld.Unknown