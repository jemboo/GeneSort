namespace GeneSort.Eval.V1.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable


[<Struct; StructuralEquality; StructuralComparison>]
type sorterScoreV1 =
    private {
        sorterId: Guid<sorterId>
        unsortedCount: int<sortableCount>
        unsortedGroupCount: int<sortableCount> option
        stageSequenceHash: int<sequenceHash>
        lastCeIndex: int<ceIndex>
    }

    static member create 
                    (sorterId: Guid<sorterId>) 
                    (unsortedCount: int<sortableCount>) 
                    (unsortedGroupCount: int<sortableCount> option) 
                    (sequenceKey: int<sequenceHash>) 
                    (lastCeIndex: int<ceIndex>) : sorterScoreV1 =
        { 
                sorterId = sorterId; 
                unsortedCount = unsortedCount; 
                unsortedGroupCount = unsortedGroupCount;
                stageSequenceHash = sequenceKey; 
                lastCeIndex = lastCeIndex;
        }

    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.UnsortedCount with get() : int<sortableCount>  = this.unsortedCount
    member this.UnsortedGroupCount with get() : int<sortableCount> option  = this.unsortedGroupCount;
    member this.StageSequenceHash with get() : int<sequenceHash>  = this.stageSequenceHash
    member this.LastCeIndex with get() : int<ceIndex>  = this.lastCeIndex
    member this.ToDataTableRecord() : GeneSort.Core.dataTableRecord =
            let groupCountStr = 
                match this.unsortedGroupCount with 
                | Some c -> string %c 
                | None -> "" // Or "N/A" depending on your reporting preference

            let isSorted = this.unsortedCount = 0<sortableCount>
            GeneSort.Core.dataTableRecord.createEmpty()
            |> GeneSort.Core.dataTableRecord.addData "SorterId" (string %this.sorterId)
            |> GeneSort.Core.dataTableRecord.addData "UnsortedCount" (string %this.unsortedCount)
            |> GeneSort.Core.dataTableRecord.addData "IsSorted" (string isSorted)
            |> GeneSort.Core.dataTableRecord.addData "UnsortedGroupCount" groupCountStr
            |> GeneSort.Core.dataTableRecord.addData "SequenceHash" (string %this.stageSequenceHash)
            |> GeneSort.Core.dataTableRecord.addData "LastCeIndex" (string %this.lastCeIndex)



type sorterScore =
    | V1 of sorterScoreV1
    | Unknown


module SorterScore =

    let fromSorterEval (sorterEval:sorterEval) : sorterScoreV1 =
        let unsortedGroupCount = 
            match sorterEval.CeBlockEval.SortableTest with
            | Some st -> Some (SortableTests.getSortableCount st)
            | None -> None

        let stageSequence = 
            StageSequence.fromCes sorterEval.CeBlockEval.CeBlock.SortingWidth sorterEval.CeBlockEval.UsedCes

        sorterScoreV1.create 
                    sorterEval.SorterId
                    sorterEval.CeBlockEval.UnsortedCount
                    unsortedGroupCount
                    (stageSequence.GetHashCode() |> UMX.tag<sequenceHash>)
                    (sorterEval.CeBlockEval.CeUseCounts.LastUsedCeIndex)


    let isUnsorted (score: sorterScore) : bool option =
        match score with
        | V1 v1 -> Some (v1.UnsortedCount > 0<sortableCount>)
        | Unknown -> None


    /// Flattens a sorterScore into a dataTableRecord
    let toDataTableRecord (score: sorterScore) : GeneSort.Core.dataTableRecord =
        match score with
        | V1 v1 -> v1.ToDataTableRecord()
        | Unknown -> 
            GeneSort.Core.dataTableRecord.createEmpty()
            |> GeneSort.Core.dataTableRecord.addData "Status" "UnknownVersion"


