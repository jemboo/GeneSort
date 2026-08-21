namespace GeneSort.Eval.V1.Sgd

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1

type sorterPoolMemberHistory = 
    private {
        sorterPoolId: Guid<sorterPoolId>
        sorterPoolMemberId: Guid<sorterPoolMemberId>
        sorterModelId: Guid<sorterModelId>
        birthday: int<generationNumber>
        saveGeneration: int<generationNumber>
        mutationIndex: int<mutationIndex>
        mutationMod: int<mutationMod>
        parentSorterPoolMemberId: Guid<sorterPoolMemberId> option
        parentSorterModelId: Guid<sorterModelId> option
        mutatorId: Guid<sorterModelMutatorId> option
        parentMutationIndex: int<mutationIndex> option
        evalV2: sorterEvalV2 option
    }

    static member create
            (sorterPoolId: Guid<sorterPoolId>,
             sorterPoolMemberId: Guid<sorterPoolMemberId>,
             sorterModelId: Guid<sorterModelId>,
             birthday: int<generationNumber>,
             saveGeneration: int<generationNumber>,
             mutationIndex: int<mutationIndex>,
             mutationMod: int<mutationMod>,
             parentSorterPoolMemberId: Guid<sorterPoolMemberId> option,
             parentSorterModelId: Guid<sorterModelId> option,
             mutatorId: Guid<sorterModelMutatorId> option,
             parentMutationIndex: int<mutationIndex> option,
             evalV2: sorterEvalV2 option) : sorterPoolMemberHistory =
        {
            sorterPoolId = sorterPoolId
            sorterPoolMemberId = sorterPoolMemberId
            sorterModelId = sorterModelId
            birthday = birthday
            saveGeneration = saveGeneration
            mutationIndex = mutationIndex
            mutationMod = mutationMod
            parentSorterPoolMemberId = parentSorterPoolMemberId
            parentSorterModelId = parentSorterModelId
            mutatorId = mutatorId
            parentMutationIndex = parentMutationIndex
            evalV2 = evalV2
        }

    member this.SorterPoolId with get() = this.sorterPoolId
    member this.SorterPoolMemberId with get() = this.sorterPoolMemberId
    member this.SorterModelId with get() = this.sorterModelId
    member this.Birthday with get() = this.birthday
    member this.SaveGeneration with get() = this.saveGeneration
    member this.MutationIndex with get() = this.mutationIndex
    member this.MutationMod with get() = this.mutationMod
    member this.ParentSorterPoolMemberId with get() = this.parentSorterPoolMemberId
    member this.ParentSorterModelId with get() = this.parentSorterModelId
    member this.MutatorId with get() = this.mutatorId
    member this.ParentMutationIndex with get() = this.parentMutationIndex
    member this.EvalV2 with get() = this.evalV2


module SorterPoolMemberHistory =

    /// Extracts a snapshot from a surviving pool member, ensuring SorterEval is forced to V2
    let fromPoolMember 
            (poolId: Guid<sorterPoolId>) 
            (parentSorterPoolMemberId: Guid<sorterPoolMemberId> option)
            (currentGen: int<generationNumber>) 
            (spm: sorterPoolMember) : sorterPoolMemberHistory =
            
        let v2Eval = 
            spm.SorterEval 
            |> Option.map (SorterEval.downgradeTo sorterEvalType.V2)
            |> Option.bind (function V2 evalV2 -> Some evalV2 | _ -> None)

        sorterPoolMemberHistory.create(
            sorterPoolId = poolId,
            sorterPoolMemberId = spm.SorterPoolMemberId,
            sorterModelId = SorterModel.getId spm.SorterModel,
            birthday = spm.Birthday,
            saveGeneration = currentGen,
            mutationIndex = spm.MutationIndex,
            mutationMod = spm.MutationMod,
            parentSorterPoolMemberId = parentSorterPoolMemberId,
            parentSorterModelId = (spm.SorterMutationSource |> Option.map (fun src -> src.SorterModelId)),
            mutatorId = (spm.SorterMutationSource |> Option.map (fun src -> src.SorterModelMutatorId)),
            parentMutationIndex = (spm.SorterMutationSource |> Option.map (fun src -> src.SorterMutationIndex)),
            evalV2 = v2Eval
        )

    let toDataTableRecord (snapshot: sorterPoolMemberHistory) : dataTableRecord =
        let baseRecord = 
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "SorterPoolId" (string %snapshot.SorterPoolId)
            |> dataTableRecord.addData "SorterPoolMemberId" (string %snapshot.SorterPoolMemberId)
            |> dataTableRecord.addData "SorterModelId" (string %snapshot.SorterModelId)
            |> dataTableRecord.addData "Birthday" (string %snapshot.Birthday)
            |> dataTableRecord.addData "SaveGeneration" (string %snapshot.SaveGeneration)
            |> dataTableRecord.addData "MutationIndex" (string %snapshot.MutationIndex)
            |> dataTableRecord.addData "MutationMod" (string %snapshot.MutationMod)
            
            // Parent Lineage fields
            |> dataTableRecord.addData "ParentSorterPoolMemberId" (snapshot.ParentSorterPoolMemberId |> Option.map (fun id -> string %id) |> Option.defaultValue "")
            |> dataTableRecord.addData "ParentSorterModelId" (snapshot.ParentSorterModelId |> Option.map (fun id -> string %id) |> Option.defaultValue "")
            |> dataTableRecord.addData "MutatorId" (snapshot.MutatorId |> Option.map (fun id -> string %id) |> Option.defaultValue "")
            |> dataTableRecord.addData "ParentMutationIndex" (snapshot.ParentMutationIndex |> Option.map (fun idx -> string %idx) |> Option.defaultValue "")

        // Merge V2 Evaluation fields with prefix
        match snapshot.EvalV2 with
        | Some eval -> 
            let evalRecord = eval.ToDataTableRecordWithPrefix("Eval_")
            dataTableRecord.combine baseRecord evalRecord
        | None -> baseRecord