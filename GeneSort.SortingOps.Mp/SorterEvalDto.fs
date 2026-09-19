
namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Sorting.Mp.Sortable

type sorterEvalV1Dto = {
    SorterId : Guid
    UnsortedCount : int
    SequenceHash : int
    LastCeIndex : int
    StageLength : int
    CeLength : int
    SortingWidth : int
    ReflectionSymmetric : bool
    StageCrossingsCount : int
}

type sorterEvalV2Dto = {
    SorterId : Guid
    UnsortedCount : int
    SequenceHash : int
    StageLength : int
    CeUseArray : ceDataDto array
    SortingWidth : int
    ReflectionSymmetric : bool
    StageCrossingsCount : int
}

type sorterEvalV3Dto = {
    SorterId : Guid
    SequenceHash : int
    StageLength : int
    CeUseArray : ceDataDto array
    SortableTest : sortableTestDto
    SortingWidth : int
    ReflectionSymmetric : bool
    StageCrossingsCount : int
}

// ---------------------------------------------------------------------
// 2. Main Sorter Evaluation DTO Union
// ---------------------------------------------------------------------

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
                ReflectionSymmetric = %v1.ReflectionSymmetric
                StageCrossingsCount = %v1.StageCrossingsCount
            }
        | sorterEval.V2 v2 ->
            V2 {
                SorterId = %v2.SorterId
                UnsortedCount = %v2.UnsortedCount
                SequenceHash = %v2.SequenceHash
                StageLength = %v2.StageLength
                CeUseArray = v2.CeUseArray |> Array.map CeDataDto.fromDomain
                SortingWidth = %v2.SortingWidth
                ReflectionSymmetric = %v2.IsReflectionSymmetric
                StageCrossingsCount = %v2.StageCrossingsCount
            }
        | sorterEval.V3 v3 ->
            V3 {
                SorterId = %v3.SorterId
                SequenceHash = %v3.SequenceHash
                StageLength = %v3.StageLength
                CeUseArray = v3.CeUseArray |> Array.map CeDataDto.fromDomain
                SortableTest = SortableTestDto.fromDomain v3.SortableTest
                SortingWidth = %v3.SortingWidth
                ReflectionSymmetric = %v3.ReflectionSymmetric
                StageCrossingsCount = %v3.StageCrossingsCount
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
                (v1Dto.ReflectionSymmetric |> UMX.tag)
                (v1Dto.StageCrossingsCount |> UMX.tag)
            |> sorterEval.V1
        | V2 v2Dto ->
            sorterEvalV2.create
                (v2Dto.SorterId |> UMX.tag)
                (v2Dto.SortingWidth |> UMX.tag)
                (v2Dto.UnsortedCount |> UMX.tag)
                (v2Dto.SequenceHash |> UMX.tag)
                (v2Dto.StageLength |> UMX.tag)
                (v2Dto.CeUseArray |> Array.map CeDataDto.toDomain)
                (v2Dto.ReflectionSymmetric |> UMX.tag)
                (v2Dto.StageCrossingsCount |> UMX.tag)
            |> sorterEval.V2
        | V3 v3Dto ->
            sorterEvalV3.create
                (v3Dto.SorterId |> UMX.tag)
                (v3Dto.SortingWidth |> UMX.tag)
                (v3Dto.SequenceHash |> UMX.tag)
                (v3Dto.StageLength |> UMX.tag)
                (v3Dto.CeUseArray |> Array.map CeDataDto.toDomain)
                (SortableTestDto.toDomain v3Dto.SortableTest)
                (v3Dto.ReflectionSymmetric |> UMX.tag)
                (v3Dto.StageCrossingsCount |> UMX.tag)
            |> sorterEval.V3