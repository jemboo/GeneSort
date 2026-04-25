namespace GeneSort.Eval.Project.Mp.V1

open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Eval.V1.Bins

[<MessagePackObject>]
type SorterEvalBinV1Dto = {
    [<Key(0)>] Key: SorterEvalKeyDto
    [<Key(1)>] Scores: SorterScoreDto array
}

[<MessagePackObject>]
type SorterEvalBinDto =
    | [<Key(0)>] V1 of SorterEvalBinV1Dto
    | [<Key(1)>] Unknown

module SorterEvalBinMapping =

    let toV1Dto (domain: sorterEvalBinV1) : SorterEvalBinV1Dto =
        {
            Key = SorterEvalKeyMapping.toDto domain.SorterEvalKey
            // Projects ResizeArray to a standard array for serialization
            Scores = domain.Scores |> Seq.map SorterScoreMapping.toDto |> Seq.toArray
        }

    let fromV1Dto (dto: SorterEvalBinV1Dto) : sorterEvalBinV1 =
        let scores = dto.Scores |> Seq.map SorterScoreMapping.fromDto
        sorterEvalBinV1.createWithScores scores (SorterEvalKeyMapping.fromDto dto.Key)


    let toDto (domain: sorterEvalBin) : SorterEvalBinDto =
        match domain with
        | sorterEvalBin.V1 v1 -> SorterEvalBinDto.V1 (toV1Dto v1)
        | sorterEvalBin.Unknown -> SorterEvalBinDto.Unknown


    let fromDto (dto: SorterEvalBinDto) : sorterEvalBin =
        match dto with
        | SorterEvalBinDto.V1 v1Dto -> sorterEvalBin.V1 (fromV1Dto v1Dto)
        | SorterEvalBinDto.Unknown -> sorterEvalBin.Unknown