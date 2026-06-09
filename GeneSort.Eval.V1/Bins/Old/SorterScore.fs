namespace GeneSort.Eval.V1.Bins.Old

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable


[<Struct; StructuralEquality; StructuralComparison>]
type sorterScoreV1Old =
    private {
        sorterId: Guid<sorterId>
        unsortedCount: int<sortableCount>
        unsortedGroupCount: int<sortableCount> option
        sequenceHash: int<sequenceHash>
        lastCeIndex: int<ceIndex>
    }

    static member create 
                    (sorterId: Guid<sorterId>) 
                    (unsortedCount: int<sortableCount>) 
                    (unsortedGroupCount: int<sortableCount> option) 
                    (sequenceKey: int<sequenceHash>) 
                    (lastCeIndex: int<ceIndex>) : sorterScoreV1Old =
        { 
                sorterId = sorterId; 
                unsortedCount = unsortedCount; 
                unsortedGroupCount = unsortedGroupCount;
                sequenceHash = sequenceKey; 
                lastCeIndex = lastCeIndex;
        }

    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.UnsortedCount with get() : int<sortableCount>  = this.unsortedCount
    member this.UnsortedGroupCount with get() : int<sortableCount> option  = this.unsortedGroupCount;
    member this.SequenceHash with get() : int<sequenceHash>  = this.sequenceHash
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
            |> GeneSort.Core.dataTableRecord.addData "SequenceHash" (string %this.sequenceHash)
            |> GeneSort.Core.dataTableRecord.addData "LastCeIndex" (string %this.lastCeIndex)




type sorterScoreOld =
    | V1 of sorterScoreV1Old
    | V1Key of sorterScoreV1Old * sorterEvalKeyOld
    | Unknown


module SorterScoreOld =

    let fromSorterEval (sorterEval:sorterEvalOld) : sorterScoreV1Old =
        let unsortedGroupCount = 
            match sorterEval.CeBlockEval.SortableTest with
            | Some st -> Some (SortableTests.getSortableCount st)
            | None -> None

        let stageSequence = 
            StageBuilderSequence.toStageSequence sorterEval.CeBlockEval.CeBlock.SortingWidth 
                                  sorterEval.CeBlockEval.UsedCes
        sorterScoreV1Old.create 
                    sorterEval.SorterId
                    sorterEval.CeBlockEval.UnsortedCount
                    unsortedGroupCount
                    (stageSequence.GetHashCode() |> UMX.tag<sequenceHash>)
                    (sorterEval.CeBlockEval.CeUseCounts.LastUsedCeIndex)


    let getSorterId (score: sorterScoreOld) : Guid<sorterId> option =
        match score with
        | V1 v1 -> Some v1.SorterId
        | V1Key (v1, _) -> Some v1.SorterId
        | Unknown -> None


    let isUnsorted (score: sorterScoreOld) : bool option =
        match score with
        | V1 v1 -> Some (v1.UnsortedCount > 0<sortableCount>)
        | V1Key (v1, _) -> Some (v1.UnsortedCount > 0<sortableCount>)
        | Unknown -> None


    let isSorted (score: sorterScoreOld) : bool option =
        match score with
        | V1 v1 -> Some (v1.UnsortedCount = 0<sortableCount>)
        | V1Key (v1, _) -> Some (v1.UnsortedCount = 0<sortableCount>)
        | Unknown -> None


    let byCeCount (score: sorterScoreOld) : float =
        match score with
        | V1 v1 -> failwith "CeCount is not available in V1 scores without a key."
        | V1Key (v1, k) -> SorterEvalKeyOld.byCeCount k
        | Unknown -> failwith "CeCount is not available in V1 scores without a key."


    let byStageLength (score: sorterScoreOld) : float =
        match score with
        | V1 v1 -> failwith "StageLength is not available in V1 scores without a key."
        | V1Key (v1, k) -> SorterEvalKeyOld.byStageLength k
        | Unknown -> failwith "StageLength is not available in V1 scores without a key."


    // lower ceCounts and stageLengths sort first
    let byWeighted (ceCountWeight: float) (stageLengthWeight: float) (score: sorterScoreOld) : float =
        match score with
        | V1 v1 -> failwith "Weighted score is not available in V1 scores without a key."
        | V1Key (v1, k) -> SorterEvalKeyOld.byWeighted ceCountWeight stageLengthWeight k
        | Unknown -> failwith "Weighted score is not available in V1 scores without a key."


    let byEqualWeighted (score: sorterScoreOld) : float =
        byWeighted 0.5 0.5 score

    /// Flattens a sorterScore into a dataTableRecord
    let toDataTableRecord (score: sorterScoreOld) : GeneSort.Core.dataTableRecord =
        match score with
        | V1 v1 -> v1.ToDataTableRecord()
        | V1Key (v1, key) -> 
            v1.ToDataTableRecord()
            |> GeneSort.Core.dataTableRecord.addData "CeCount" (string %key.CeCount)
            |> GeneSort.Core.dataTableRecord.addData "StageLength" (string %key.StageLength)
        | Unknown -> 
            GeneSort.Core.dataTableRecord.createEmpty()
            |> GeneSort.Core.dataTableRecord.addData "Status" "UnknownVersion"


    let sequenceHash (score: sorterScoreOld) : int<sequenceHash> option =
        match score with
        | V1 v1 -> Some v1.SequenceHash
        | V1Key (v1, _) -> Some v1.SequenceHash
        | Unknown -> None


    let unsortedCount (score: sorterScoreOld) : int<sortableCount> option =
        match score with
        | V1 v1 -> Some v1.UnsortedCount
        | V1Key (v1, _) -> Some v1.UnsortedCount
        | Unknown -> None


    let filterAndRemoveDuplicates
                    (filter: sorterScoreOld -> sorterScoreOld option) 
                    (scores: sorterScoreOld seq) : sorterScoreOld [] =
            scores
            |> Seq.choose filter
            |> Seq.distinctBy (sequenceHash)
            |> Seq.toArray


    let evenSampleByRankedIndex 
                (ranker: sorterScoreOld -> float)
                (count: int<sorterCount>) 
                (scores: sorterScoreOld seq) : sorterScoreOld [] =
        
        // 1. Guard check for invalid input count
        if %count <= 0 then 
            failwithf "Requested sample count must be greater than zero, but was %d" count

        // 2. Filter for successful scores (unsortedCount = 0) and strip duplicates
        let filterSuccess (score: sorterScoreOld) =
            (isSorted score) |> Option.bind (fun isS -> if isS then Some score else None)

        let uniqueSuccessfulScores = filterAndRemoveDuplicates filterSuccess scores

        // 3. Fail if we don't have enough elements to satisfy 'count'
        let availableCount = uniqueSuccessfulScores.Length
        if availableCount < %count then
            failwithf "Cannot sample %d items; only %d unique successful scores are available." count availableCount

        // 4. Sort descending so highest ranking items come first
        let sortedScores = uniqueSuccessfulScores |> Array.sortByDescending ranker

        // 5. Build our evenly sampled array
        if %count = 1 then
            // If they only asked for 1 item, return the highest ranking one
            [| sortedScores.[0] |]
        else
            // Array to hold our evenly sampled results
            let result = Array.zeroCreate %count
            
            // The highest-ranking item must be included first
            result.[0] <- sortedScores.[0]

            // We need to pick (count - 1) more items from the remaining (availableCount - 1) indices.
            // Using floating point step math ensures an even spatial distribution without edge overflow.
            let step = float (availableCount - 1) / float (%count - 1)

            for i in 1 .. (%count - 1) do
                // Calculate position and round safely to an integer index
                let targetIndex = int (round (float i * step))
                result.[i] <- sortedScores.[targetIndex]

            result



    let evenSampleByRankedValue 
                (ranker: sorterScoreOld -> float)
                (count: int<sorterCount>) 
                (scores: sorterScoreOld seq) 
                : sorterScoreOld [] =
        
        // 1. Guard check for invalid input count
        if %count <= 0 then 
            failwithf "Requested sample count must be greater than zero, but was %d" count

        // 2. Filter for successful scores (unsortedCount = 0) and strip duplicates
        let filterSuccess (score: sorterScoreOld) =
            (isSorted score) |> Option.bind (fun isS -> if isS then Some score else None)

        let uniqueSuccessfulScores = filterAndRemoveDuplicates filterSuccess scores

        // 3. Fail if we don't have enough elements to satisfy 'count'
        let availableCount = uniqueSuccessfulScores.Length
        if availableCount < %count then
            failwithf "Cannot sample %d items; only %d unique successful scores are available."
                            %count availableCount

        // 4. Sort descending so highest ranking items come first
        let sortedScores = uniqueSuccessfulScores |> Array.sortByDescending ranker

        // 5. Build our evenly sampled array
        if %count = 1 then
            // If they only asked for 1 item, return the highest ranking one
            [| sortedScores.[0] |]
        else
            // Array to hold our evenly sampled results
            let result = Array.zeroCreate %count
            
            // The highest-ranking item must be included first
            result.[0] <- sortedScores.[0]

            // We need to pick (count - 1) more items from the remaining (availableCount - 1) indices.
            // Using floating point step math ensures an even spatial distribution without edge overflow.
            let step = float (availableCount - 1) / float (%count - 1)

            for i in 1 .. (%count - 1) do
                // Calculate position and round safely to an integer index
                let targetIndex = int (round (float i * step))
                result.[i] <- sortedScores.[targetIndex]

            result