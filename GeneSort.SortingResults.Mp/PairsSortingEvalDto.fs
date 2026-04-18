namespace GeneSort.SortingResults.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps.Mp
open GeneSort.SortingResults
open GeneSort.Model.Sorting

[<MessagePackObject>]
type pairsSortingEvalDto = {
    [<Key(0)>]
    Tag: int   // 0 = SplitPairs, 1 = SplitPairs2
    [<Key(1)>]
    Value: splitPairsSortingEvalDto
}

module PairsSortingEvalDto =

    let fromDomain (result: pairsSortingEval) : pairsSortingEvalDto =
        match result with
        | SplitPairs  inner -> { Tag = 0; Value = SplitPairsSortingEvalDto.fromDomain inner }
        | SplitPairs2 inner -> { Tag = 1; Value = SplitPairsSortingEvalDto.fromDomain inner }

    let toDomain (dto: pairsSortingEvalDto) : pairsSortingEval =
        let inner = SplitPairsSortingEvalDto.toDomain dto.Value
        match dto.Tag with
        | 0 -> SplitPairs  inner
        | 1 -> SplitPairs2 inner
        | n -> failwithf "Unknown pairsSortingResult tag: %d" n