namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Bins
open GeneSort.Sorting

[<MessagePackObject>]
type binEntryDto = {
    [<Key(0)>] Key: sorterEvalKeyDto
    [<Key(1)>] Bin: sorterEvalBinV1Dto
}

[<MessagePackObject>]
type sorterEvalBinsV1Dto = {
    [<Key(0)>] SorterEvalBinsId: Guid
    [<Key(1)>] SortableTestId: Guid
    [<Key(2)>] Bins: binEntryDto array
}

[<MessagePackObject>]
type sorterEvalBinsDto =
    | [<Key(0)>] V1 of sorterEvalBinsV1Dto
    | [<Key(1)>] Unknown

module SorterEvalBinsDto =

    let toV1Dto (domain: sorterEvalBinsV1) : sorterEvalBinsV1Dto =
        {
            SorterEvalBinsId = UMX.untag domain.Id
            SortableTestId = UMX.untag domain.SortableTestId
            Bins = 
                domain.Bins 
                |> Map.toSeq 
                |> Seq.map (fun (k, v) -> 
                    { Key = SorterEvalKeyDto.toDto k
                      Bin = SorterEvalBinDto.toV1Dto v })
                |> Seq.toArray
        }

    let fromV1Dto (dto: sorterEvalBinsV1Dto) : sorterEvalBinsV1 =
        let binMap = 
            dto.Bins 
            |> Array.map (fun entry -> 
                SorterEvalKeyDto.fromDto entry.Key, 
                SorterEvalBinDto.fromV1Dto entry.Bin)
            |> Map.ofArray


        sorterEvalBinsV1.create 
            (UMX.tag<sorterEvalBinsId> dto.SorterEvalBinsId) 
            (UMX.tag<sortableTestId> dto.SortableTestId) 
            binMap
            

    let toDto (domain: sorterEvalBins) : sorterEvalBinsDto =
        match domain with
        | sorterEvalBins.V1 v1 -> sorterEvalBinsDto.V1 (toV1Dto v1)
        | sorterEvalBins.Unknown -> sorterEvalBinsDto.Unknown

    let fromDto (dto: sorterEvalBinsDto) : sorterEvalBins =
        match dto with
        | sorterEvalBinsDto.V1 v1Dto -> sorterEvalBins.V1 (fromV1Dto v1Dto)
        | sorterEvalBinsDto.Unknown -> sorterEvalBins.Unknown