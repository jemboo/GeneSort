namespace GeneSort.Eval.Project.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Bins
open GeneSort.Sorting

[<MessagePackObject>]
type BinEntryDto = {
    [<Key(0)>] Key: SorterEvalKeyDto
    [<Key(1)>] Bin: SorterEvalBinV1Dto
}

[<MessagePackObject>]
type SorterEvalBinsV1Dto = {
    [<Key(0)>] SorterEvalBinsId: Guid
    [<Key(1)>] SortableTestId: Guid
    [<Key(2)>] Bins: BinEntryDto array
}

[<MessagePackObject>]
type SorterEvalBinsDto =
    | [<Key(0)>] V1 of SorterEvalBinsV1Dto
    | [<Key(1)>] Unknown

module SorterEvalBinsMapping =

    let toV1Dto (domain: sorterEvalBinsV1) : SorterEvalBinsV1Dto =
        {
            SorterEvalBinsId = UMX.untag domain.Id
            SortableTestId = UMX.untag domain.SortableTestId
            Bins = 
                domain.Bins 
                |> Map.toSeq 
                |> Seq.map (fun (k, v) -> 
                    { Key = SorterEvalKeyMapping.toDto k
                      Bin = SorterEvalBinMapping.toV1Dto v })
                |> Seq.toArray
        }

    let fromV1Dto (dto: SorterEvalBinsV1Dto) : sorterEvalBinsV1 =
        let binMap = 
            dto.Bins 
            |> Array.map (fun entry -> 
                SorterEvalKeyMapping.fromDto entry.Key, 
                SorterEvalBinMapping.fromV1Dto entry.Bin)
            |> Map.ofArray


        sorterEvalBinsV1.create 
            (UMX.tag<sorterEvalBinsId> dto.SorterEvalBinsId) 
            (UMX.tag<sortableTestId> dto.SortableTestId) 
            binMap
            

    let toDto (domain: sorterEvalBins) : SorterEvalBinsDto =
        match domain with
        | sorterEvalBins.V1 v1 -> SorterEvalBinsDto.V1 (toV1Dto v1)
        | sorterEvalBins.Unknown -> SorterEvalBinsDto.Unknown

    let fromDto (dto: SorterEvalBinsDto) : sorterEvalBins =
        match dto with
        | SorterEvalBinsDto.V1 v1Dto -> sorterEvalBins.V1 (fromV1Dto v1Dto)
        | SorterEvalBinsDto.Unknown -> sorterEvalBins.Unknown