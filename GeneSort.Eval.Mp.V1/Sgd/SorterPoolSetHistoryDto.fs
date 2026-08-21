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

[<MessagePackObject>]
type sorterPoolMemberHistoryDto = {
    [<Key(0)>] SorterPoolId: Guid
    [<Key(1)>] SorterPoolMemberId: Guid
    [<Key(2)>] SorterModelId: Guid
    [<Key(3)>] Birthday: int
    [<Key(4)>] SaveGeneration: int
    [<Key(5)>] MutationIndex: int
    [<Key(6)>] MutationMod: int
    
    // Lineage Details
    [<Key(7)>] ParentSorterPoolMemberId: Nullable<Guid>
    [<Key(8)>] ParentSorterModelId: Nullable<Guid>
    [<Key(9)>] MutatorId: Nullable<Guid>
    [<Key(10)>] ParentMutationIndex: Nullable<int>
    
    // Evaluation at V2 level
    [<Key(11)>] EvalV2: sorterEvalV2Dto option
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
            ParentSorterModelId = domain.ParentSorterModelId |> Option.map (fun id -> %id) |> Option.toNullable
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

        sorterPoolMemberHistory.create(
            sorterPoolId = UMX.tag dto.SorterPoolId,
            sorterPoolMemberId = UMX.tag dto.SorterPoolMemberId,
            sorterModelId = UMX.tag dto.SorterModelId,
            birthday = UMX.tag dto.Birthday,
            saveGeneration = UMX.tag dto.SaveGeneration,
            mutationIndex = UMX.tag dto.MutationIndex,
            mutationMod = UMX.tag dto.MutationMod,
            parentSorterPoolMemberId = (dto.ParentSorterPoolMemberId |> Option.ofNullable |> Option.map UMX.tag),
            parentSorterModelId = (dto.ParentSorterModelId |> Option.ofNullable |> Option.map UMX.tag),
            mutatorId = (dto.MutatorId |> Option.ofNullable |> Option.map UMX.tag),
            parentMutationIndex = (dto.ParentMutationIndex |> Option.ofNullable |> Option.map UMX.tag),
            evalV2 = v2Domain
        )

// ----------------------------------------------------------------------------
// sorterPoolHistoryDto
// ----------------------------------------------------------------------------
[<MessagePackObject>]
type sorterPoolHistoryDto = {
    [<Key(0)>] SorterPoolId: Guid
    [<Key(1)>] SaveGeneration: int
    [<Key(2)>] MemberHistories: sorterPoolMemberHistoryDto list
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

[<MessagePackObject>]
type sorterPoolSetHistoryDto = {
    [<Key(0)>] SorterPoolSetId: Guid
    [<Key(1)>] SaveGeneration: int
    [<Key(2)>] PoolHistories: sorterPoolHistoryDto list
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

// ----------------------------------------------------------------------------
// sorterPoolSetHistoryCollectionDto
// ----------------------------------------------------------------------------

//[<MessagePackObject>]
//type sorterPoolSetHistoryCollectionDto = {
//    [<Key(0)>] CollectionId: Guid
//    [<Key(1)>] Histories: sorterPoolSetHistoryDto list
//}

//module SorterPoolSetHistoryCollectionDto =

//    let fromDomain (domain: sorterPoolSetHistoryCollection) : sorterPoolSetHistoryCollectionDto =
//        {
//            CollectionId = %domain.CollectionId
//            Histories = domain.Histories |> List.map SorterPoolSetHistoryDto.fromDomain
//        }

//    let toDomain (dto: sorterPoolSetHistoryCollectionDto) : sorterPoolSetHistoryCollection =
//        {
//            CollectionId = UMX.tag dto.CollectionId
//            Histories = dto.Histories |> List.map SorterPoolSetHistoryDto.toDomain
//        }