namespace GeneSort.Eval.Project.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Eval.V1.Bins
open GeneSort.SortingOps

[<MessagePackObject>]
type SorterScoreV1Dto = {
    [<Key(0)>] SorterId: Guid
    [<Key(1)>] UnsortedCount: int
    [<Key(2)>] UnsortedGroupCount: Nullable<int> 
    [<Key(3)>] SequenceHash: int
    [<Key(4)>] LastCeIndex: int
}

module SorterScoreV1Mapping =
    
    let toDto (domain: sorterScoreV1) : SorterScoreV1Dto =
        {
            SorterId = UMX.untag domain.SorterId
            UnsortedCount = UMX.untag domain.UnsortedCount
            UnsortedGroupCount = 
                match domain.UnsortedGroupCount with 
                | Some c -> Nullable(UMX.untag c) 
                | None -> Nullable()
            SequenceHash = UMX.untag domain.SequenceKey
            LastCeIndex = UMX.untag domain.LastCeIndex
        }

    let fromDto (dto: SorterScoreV1Dto) : sorterScoreV1 =
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
type SorterScoreDto = 
    | [<Key(0)>] V1 of SorterScoreV1Dto
    | [<Key(1)>] Unknown

module SorterScoreMapping =
    
    let toDto (score: sorterScore) : SorterScoreDto =
        match score with
        | sorterScore.V1 v1 -> SorterScoreDto.V1 (SorterScoreV1Mapping.toDto v1)
        | sorterScore.Unknown -> SorterScoreDto.Unknown

    let fromDto (dto: SorterScoreDto) : sorterScore =
        match dto with
        | SorterScoreDto.V1 v1Dto -> sorterScore.V1 (SorterScoreV1Mapping.fromDto v1Dto)
        | SorterScoreDto.Unknown -> sorterScore.Unknown