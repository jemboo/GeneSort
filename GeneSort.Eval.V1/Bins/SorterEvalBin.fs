namespace GeneSort.Eval.V1.Bins

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

type sorterEvalBinV1Old =
    private {
        /// Mutable for O(1) additions during the evaluation phase
        sorterScores: ResizeArray<sorterScoreOld>
        sorterEvalKey: sorterEvalKeyOld
    }

    static member create (score: sorterScoreOld) (key : sorterEvalKeyOld) =
        let scores = ResizeArray<sorterScoreOld>()
        scores.Add(score)
        { 
            sorterScores = scores
            sorterEvalKey = key
        }

    static member createWithScores (scores: sorterScoreOld seq) (key : sorterEvalKeyOld) =
        { 
            sorterScores = ResizeArray(scores) 
            sorterEvalKey = key
        }

    member this.EvalCount with get() = this.sorterScores.Count
    member this.SorterEvalKey with get() = this.sorterEvalKey
    member this.Scores with get() = this.sorterScores :> IReadOnlyList<sorterScoreOld>
    member this.UnsortedCount with get() = 
        this.sorterScores 
        |> Seq.map (function sorterScoreOld.V1 s -> s.UnsortedCount | _ -> 0<sortableCount>) 
        |> Seq.countBy id

    /// Appends a score to the existing bin (Mutable Addition)
    member this.AddScore (score: sorterScoreOld) =
        this.sorterScores.Add(score)

    member this.toSorterScoresWithKeys() : sorterScoreOld seq =
        this.sorterScores 
        |> Seq.map (function 
            | sorterScoreOld.V1 s -> sorterScoreOld.V1Key (s, this.sorterEvalKey)
            | other -> other)

    member this.ToEvalCountRecord() : dataTableRecord =
        SorterEvalKeyOld.toDataTableRecord this.sorterEvalKey
        |> dataTableRecord.addData "EvalCount" (this.EvalCount.ToString())

    // Returns two records, providing the EvalCount for both the sorted and unsorted cases.
    member this.ToSortedAndUnsortedRecords() : dataTableRecord [] =
        [| 
            let unsortedCount = this.sorterScores 
                                |> Seq.filter (SorterScoreOld.isUnsorted >> Option.defaultValue false) 
                                |> Seq.length
            let dtrS = SorterEvalKeyOld.toDataTableRecord this.sorterEvalKey
                       |> dataTableRecord.addKeyAndData "Sorted" "True"
                       |> dataTableRecord.addData "EvalCount" ((this.EvalCount - unsortedCount).ToString())
            dtrS;

            let dtrU = SorterEvalKeyOld.toDataTableRecord this.sorterEvalKey
                       |> dataTableRecord.addKeyAndData "Sorted" "False"
                       |> dataTableRecord.addData "EvalCount" (unsortedCount.ToString())
            dtrU;
        |]


module SorterEvalBinV1 =
    
    /// Combines the bin key and each score into a sequence of records
    let makeDataTableRecords (bin: sorterEvalBinV1Old) : GeneSort.Core.dataTableRecord seq =
        let keyRecord = SorterEvalKeyOld.toDataTableRecord bin.SorterEvalKey
        
        bin.Scores 
        |> Seq.map (fun score ->
            let scoreRecord = SorterScoreOld.toDataTableRecord score
            // Merge the key data into the score data
            (scoreRecord, keyRecord.Data) 
            ||> Map.fold (fun acc k v -> GeneSort.Core.dataTableRecord.addData k v acc)
        )




type sorterEvalBin =
    | V1 of sorterEvalBinV1Old
    | Unknown
