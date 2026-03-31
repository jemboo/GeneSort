namespace GeneSort.SortingResults.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps.Mp
open GeneSort.SortingResults
open GeneSort.Model.Sorting

[<MessagePackObject>]
type pairsSortingResultDto = {
    [<Key(0)>]
    Tag: int   // 0 = SplitPairs, 1 = SplitPairs2
    [<Key(1)>]
    Value: splitPairsSortingResultDto
}

module PairsSortingResultDto =

    let toDto (result: pairsSortingResult) : pairsSortingResultDto =
        match result with
        | SplitPairs  inner -> { Tag = 0; Value = SplitPairsSortingResultDto.toDto inner }
        | SplitPairs2 inner -> { Tag = 1; Value = SplitPairsSortingResultDto.toDto inner }

    let fromDto (dto: pairsSortingResultDto) : pairsSortingResult =
        let inner = SplitPairsSortingResultDto.fromDto dto.Value
        match dto.Tag with
        | 0 -> SplitPairs  inner
        | 1 -> SplitPairs2 inner
        | n -> failwithf "Unknown pairsSortingResult tag: %d" n