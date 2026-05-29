namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Sorting.Mp.Sortable



[<MessagePackObject>]
type sorterEvalV1Dto = {
    [<Key(0)>] SorterId : Guid
    [<Key(1)>] UnsortedCount : int
    [<Key(2)>] SequenceHash : int
    [<Key(3)>] LastCeIndex : int
    [<Key(4)>] StageLength : int
    [<Key(5)>] CeLength : int
    [<Key(5)>] SortingWidth : int
}

[<MessagePackObject>]
type sorterEvalV2Dto = {
    [<Key(0)>] SorterId : Guid
    [<Key(1)>] UnsortedCount : int
    [<Key(2)>] SequenceHash : int
    [<Key(3)>] StageLength : int
    [<Key(4)>] CeDataSequence : ceDataDto array
    [<Key(5)>] SortingWidth : int
}

[<MessagePackObject>]
type sorterEvalV3Dto = {
    [<Key(0)>] SorterId : Guid
    [<Key(1)>] SequenceHash : int
    [<Key(2)>] StageLength : int
    [<Key(3)>] CeDataSequence : ceDataDto array
    [<Key(4)>] SortableTest : sortableTestDto
    [<Key(5)>] SortingWidth : int
}


// ---------------------------------------------------------------------
// 2. Main Sorter Evaluation DTO Union
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterEvalDto =
    | V1 of sorterEvalV1Dto
    | V2 of sorterEvalV2Dto
    | V3 of sorterEvalV3Dto


// ---------------------------------------------------------------------
// 3. Conversion Module
// ---------------------------------------------------------------------

module SorterEvalDto =

    let fromDomain (domain: sorterEval) : sorterEvalDto =
        match domain with
        | sorterEval.V1 v1 ->
            V1 {
                SorterId = %v1.SorterId
                UnsortedCount = %v1.UnsortedCount
                SequenceHash = %v1.SequenceHash
                LastCeIndex = %v1.LastCeIndex
                StageLength = %v1.StageLength
                CeLength = %v1.CeLength
                SortingWidth = %v1.SortingWidth
            }
        | sorterEval.V2 v2 ->
            V2 {
                SorterId = %v2.SorterId
                UnsortedCount = %v2.UnsortedCount
                SequenceHash = %v2.SequenceHash
                StageLength = %v2.StageLength
                CeDataSequence = v2.CeDataSequence |> Array.map CeDataDto.fromDomain
                SortingWidth = %v2.SortingWidth
            }
        | sorterEval.V3 v3 ->
            V3 {
                SorterId = %v3.SorterId
                SequenceHash = %v3.SequenceHash
                StageLength = %v3.StageLength
                CeDataSequence = v3.CeDataSequence |> Array.map CeDataDto.fromDomain
                SortableTest = SortableTestDto.fromDomain v3.SortableTest
                SortingWidth = %v3.SortingWidth
            }

    let toDomain (dto: sorterEvalDto) : sorterEval =
        match dto with
        | V1 v1Dto ->
            sorterEvalV1.create
                (v1Dto.SorterId |> UMX.tag)
                (v1Dto.SortingWidth |> UMX.tag)
                (v1Dto.UnsortedCount |> UMX.tag)
                (v1Dto.SequenceHash |> UMX.tag)
                (v1Dto.LastCeIndex |> UMX.tag)
                (v1Dto.StageLength |> UMX.tag)
                (v1Dto.CeLength |> UMX.tag)
            |> sorterEval.V1
        | V2 v2Dto ->
            sorterEvalV2.create
                (v2Dto.SorterId |> UMX.tag)
                (v2Dto.SortingWidth |> UMX.tag)
                (v2Dto.UnsortedCount |> UMX.tag)
                (v2Dto.SequenceHash |> UMX.tag)
                (v2Dto.StageLength |> UMX.tag)
                (v2Dto.CeDataSequence |> Array.map CeDataDto.toDomain)
            |> sorterEval.V2
        | V3 v3Dto ->
            sorterEvalV3.create
                (v3Dto.SorterId |> UMX.tag)
                (v3Dto.SortingWidth |> UMX.tag)
                (v3Dto.SequenceHash |> UMX.tag)
                (v3Dto.StageLength |> UMX.tag)
                (v3Dto.CeDataSequence |> Array.map CeDataDto.toDomain)
                (SortableTestDto.toDomain v3Dto.SortableTest)
            |> sorterEval.V3