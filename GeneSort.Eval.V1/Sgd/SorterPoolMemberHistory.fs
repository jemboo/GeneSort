namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1

type sorterPoolMemberHistory = {
    SorterPoolId: Guid<sorterPoolId>
    SorterPoolMemberId: Guid<sorterPoolMemberId>
    SorterModelId: Guid<sorterModelId>
    Birthday: int<generationNumber>
    SaveGeneration: int<generationNumber>
    MutationIndex: int<mutationIndex>
    MutationMod: int<mutationMod>
    
    // Lineage Details
    ParentSorterPoolMemberId: Guid<sorterPoolMemberId> option
    ParentSorterModelId: Guid<sorterModelId> option
    MutatorId: Guid<sorterModelMutatorId> option
    ParentMutationIndex: int<mutationIndex> option
    
    // Evaluation at V2 level
    EvalV2: sorterEvalV2 option
}

module SorterPoolMemberHistory =

    /// Extracts a snapshot from a surviving pool member, ensuring SorterEval is forced to V2
    let fromPoolMember 
            (poolId: Guid<sorterPoolId>) 
            (currentGen: int<generationNumber>) 
            (spm: sorterPoolMember) : sorterPoolMemberHistory =
            
        let v2Eval = 
            spm.SorterEval 
            |> Option.map (SorterEval.downgradeTo sorterEvalType.V2)
            |> Option.bind (function V2 evalV2 -> Some evalV2 | _ -> None)

        {
            SorterPoolId = poolId
            SorterPoolMemberId = spm.SorterPoolMemberId
            SorterModelId = SorterModel.getId spm.SorterModel
            Birthday = spm.Birthday
            SaveGeneration = currentGen
            MutationIndex = spm.MutationIndex
            MutationMod = spm.MutationMod
            
            ParentSorterPoolMemberId = None // If populated via extended sorterMutationSource
            ParentSorterModelId = spm.SorterMutationSource |> Option.map (fun src -> src.SorterModelId)
            MutatorId = spm.SorterMutationSource |> Option.map (fun src -> src.SorterModelMutatorId)
            ParentMutationIndex = spm.SorterMutationSource |> Option.map (fun src -> src.SorterMutationIndex)
            
            EvalV2 = v2Eval
        }

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
                |> dataTableRecord.addData "ParentSorterModelId" (snapshot.ParentSorterModelId |> Option.map (fun id -> string %id) |> Option.defaultValue "")
                |> dataTableRecord.addData "MutatorId" (snapshot.MutatorId |> Option.map (fun id -> string %id) |> Option.defaultValue "")
                |> dataTableRecord.addData "ParentMutationIndex" (snapshot.ParentMutationIndex |> Option.map (fun idx -> string %idx) |> Option.defaultValue "")

            // Merge V2 Evaluation fields with prefix
            match snapshot.EvalV2 with
            | Some eval -> 
                let evalRecord = eval.ToDataTableRecordWithPrefix("Eval_")
                // Merge helper depending on dataTableRecord implementation
                dataTableRecord.combine baseRecord evalRecord
            | None -> baseRecord