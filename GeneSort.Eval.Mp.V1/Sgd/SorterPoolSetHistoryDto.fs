namespace GeneSort.Eval.Mp.V1.Sgd

open System
open MessagePack
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp
open GeneSort.Eval.V1.Sgd

// ----------------------------------------------------------------------------
// sorterPoolMemberHistoryDto
// ----------------------------------------------------------------------------

type sorterPoolMemberHistoryDto = {
    SorterPoolId: Guid
    SorterPoolMemberId: Guid
    SorterModelId: Guid
    Birthday: int
    SaveGeneration: int
    MutationIndex: int
    MutationMod: int
    
    // Lineage Details
    ParentSorterPoolMemberId: Nullable<Guid>
    ParentSorterPoolId: Nullable<Guid>
    MutatorId: Nullable<Guid>
    ParentMutationIndex: Nullable<int>
    
    // Evaluation at V2 level
    EvalV2: sorterEvalV2Dto option
}

module SorterPoolMemberHistoryDto =

    let fromDomain (domain: sorterPoolMemberHistory) : sorterPoolMemberHistoryDto =
        let v2Dto =
            domain.EvalV2
            |> Option.map (fun v2 ->
                {
                    SorterId = %v2.SorterId
                    UnsortedCount = %v2.UnsortedCount
                    SequenceHash = %v2.SequenceHash
                    StageLength = %v2.StageLength
                    CeUseArray = v2.CeUseArray |> Array.map CeDataDto.fromDomain
                    SortingWidth = %v2.SortingWidth
                    ReflectionSymmetric = %v2.IsReflectionSymmetric
                    StageCrossingsCount = %v2.StageCrossingsCount
                })

        {
            SorterPoolId = %domain.SorterPoolId
            SorterPoolMemberId = %domain.SorterPoolMemberId
            SorterModelId = %domain.SorterModelId
            Birthday = %domain.Birthday
            SaveGeneration = %domain.SaveGeneration
            MutationIndex = %domain.MutationIndex
            MutationMod = %domain.MutationMod
            
            ParentSorterPoolMemberId = domain.ParentSorterPoolMemberId |> Option.map (fun id -> %id) |> Option.toNullable
            ParentSorterPoolId = domain.ParentSorterPoolId |> Option.map (fun id -> %id) |> Option.toNullable
            MutatorId = domain.MutatorId |> Option.map (fun id -> %id) |> Option.toNullable
            ParentMutationIndex = domain.ParentMutationIndex |> Option.map (fun idx -> %idx) |> Option.toNullable
            
            EvalV2 = v2Dto
        }

    let toDomain (dto: sorterPoolMemberHistoryDto) : sorterPoolMemberHistory =
        let v2Domain =
            dto.EvalV2
            |> Option.map (fun v2Dto ->
                sorterEvalV2.create
                    (v2Dto.SorterId |> UMX.tag)
                    (v2Dto.SortingWidth |> UMX.tag)
                    (v2Dto.UnsortedCount |> UMX.tag)
                    (v2Dto.SequenceHash |> UMX.tag)
                    (v2Dto.StageLength |> UMX.tag)
                    (v2Dto.CeUseArray |> Array.map CeDataDto.toDomain)
                    (v2Dto.ReflectionSymmetric |> UMX.tag)
                    (v2Dto.StageCrossingsCount |> UMX.tag)
            )

        sorterPoolMemberHistory.create
            (UMX.tag dto.SorterPoolId)
            (UMX.tag dto.SorterPoolMemberId)
            (UMX.tag dto.SorterModelId)
            (UMX.tag dto.Birthday)
            (UMX.tag dto.SaveGeneration)
            (UMX.tag dto.MutationIndex)
            (UMX.tag dto.MutationMod)
            (dto.ParentSorterPoolMemberId |> Option.ofNullable |> Option.map UMX.tag)
            (dto.ParentSorterPoolId |> Option.ofNullable |> Option.map UMX.tag)
            (dto.MutatorId |> Option.ofNullable |> Option.map UMX.tag)
            (dto.ParentMutationIndex |> Option.ofNullable |> Option.map UMX.tag)
            v2Domain

// ----------------------------------------------------------------------------
// sorterPoolHistoryDto
// ----------------------------------------------------------------------------

type sorterPoolHistoryDto = {
    SorterPoolId: Guid
    SaveGeneration: int
    MemberHistories: sorterPoolMemberHistoryDto list
}

module SorterPoolHistoryDto =

    let fromDomain (domain: sorterPoolHistory) : sorterPoolHistoryDto =
        {
            SorterPoolId = %domain.SorterPoolId
            SaveGeneration = %domain.SaveGeneration
            MemberHistories = domain.MemberHistories |> List.map SorterPoolMemberHistoryDto.fromDomain
        }

    let toDomain (dto: sorterPoolHistoryDto) : sorterPoolHistory =
        sorterPoolHistory.create(
            sorterPoolId = UMX.tag dto.SorterPoolId,
            saveGeneration = UMX.tag dto.SaveGeneration,
            memberHistories = (dto.MemberHistories |> List.map SorterPoolMemberHistoryDto.toDomain)
        )

// ----------------------------------------------------------------------------
// sorterPoolSetHistoryDto
// ----------------------------------------------------------------------------

type sorterPoolSetHistoryDto = {
    SorterPoolSetId: Guid
    SaveGeneration: int
    PoolHistories: sorterPoolHistoryDto list
}

module SorterPoolSetHistoryDto =

    let fromDomain (domain: sorterPoolSetHistory) : sorterPoolSetHistoryDto =
        {
            SorterPoolSetId = %domain.SorterPoolSetId
            SaveGeneration = %domain.SaveGeneration
            PoolHistories = domain.PoolHistories |> List.map SorterPoolHistoryDto.fromDomain
        }

    let toDomain (dto: sorterPoolSetHistoryDto) : sorterPoolSetHistory =
        sorterPoolSetHistory.create(
            sorterPoolSetId = UMX.tag dto.SorterPoolSetId,
            saveGeneration = UMX.tag dto.SaveGeneration,
            poolHistories = (dto.PoolHistories |> List.map SorterPoolHistoryDto.toDomain)
        )