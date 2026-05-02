namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Eval.V1.Bins
open GeneSort.SortingOps

[<MessagePackObject>]
type sorterScoreV1Dto = {
    [<Key(0)>] SorterId: Guid
    [<Key(1)>] UnsortedCount: int
    [<Key(2)>] UnsortedGroupCount: Nullable<int> 
    [<Key(3)>] SequenceHash: int
    [<Key(4)>] LastCeIndex: int
}

module SorterScoreV1Dto =
    
    let toDto (domain: sorterScoreV1) : sorterScoreV1Dto =
        {
            SorterId = UMX.untag domain.SorterId
            UnsortedCount = UMX.untag domain.UnsortedCount
            UnsortedGroupCount = 
                match domain.UnsortedGroupCount with 
                | Some c -> Nullable(UMX.untag c) 
                | None -> Nullable()
            SequenceHash = UMX.untag domain.StageSequenceHash
            LastCeIndex = UMX.untag domain.LastCeIndex
        }

    let fromDto (dto: sorterScoreV1Dto) : sorterScoreV1 =
        let groupCount = 
            if dto.UnsortedGroupCount.HasValue 
            then Some (UMX.tag<sortableCount> dto.UnsortedGroupCount.Value) 
            else None
            
        sorterScoreV1.create
            (UMX.tag<sorterId> dto.SorterId)
            (UMX.tag<sortableCount> dto.UnsortedCount)
            groupCount
            (UMX.tag<sequenceHash> dto.SequenceHash)
            (UMX.tag<ceIndex> dto.LastCeIndex)


[<MessagePackObject>]
type sorterScoreDto = 
    | [<Key(0)>] V1 of sorterScoreV1Dto
    | [<Key(1)>] Unknown

module SorterScoreDto =
    
    let toDto (score: sorterScore) : sorterScoreDto =
        match score with
        | sorterScore.V1 v1 -> sorterScoreDto.V1 (SorterScoreV1Dto.toDto v1)
        | sorterScore.Unknown -> sorterScoreDto.Unknown

    let fromDto (dto: sorterScoreDto) : sorterScore =
        match dto with
        | sorterScoreDto.V1 v1Dto -> sorterScore.V1 (SorterScoreV1Dto.fromDto v1Dto)
        | sorterScoreDto.Unknown -> sorterScore.Unknown