namespace GeneSort.Eval.V1.Bins

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

type sorterEvalBinV1 =
    private {
        /// Mutable for O(1) additions during the evaluation phase
        sorterScores: ResizeArray<sorterScore>
        sorterEvalKey: sorterEvalKey
    }

    static member create (score: sorterScore) (key : sorterEvalKey) =
        let scores = ResizeArray<sorterScore>()
        scores.Add(score)
        { 
            sorterScores = scores
            sorterEvalKey = key
        }

    static member createWithScores (scores: sorterScore seq) (key : sorterEvalKey) =
        { 
            sorterScores = ResizeArray(scores) 
            sorterEvalKey = key
        }

    member this.EvalCount with get() = this.sorterScores.Count
    member this.SorterEvalKey with get() = this.sorterEvalKey
    member this.Scores with get() = this.sorterScores :> IReadOnlyList<sorterScore>
    member this.UnsortedCount with get() = 
        this.sorterScores 
        |> Seq.map (function sorterScore.V1 s -> s.UnsortedCount | _ -> 0<sortableCount>) 
        |> Seq.countBy id

    /// Appends a score to the existing bin (Mutable Addition)
    member this.AddScore (score: sorterScore) =
        this.sorterScores.Add(score)

    member this.ToEvalCountRecord() : dataTableRecord =
        SorterEvalKey.toDataTableRecord this.sorterEvalKey
        |> dataTableRecord.addData "EvalCount" (this.EvalCount.ToString())

    // Returns two records, providing the EvalCount for both the sorted and unsorted cases.
    member this.ToSortedAndUnsortedRecords() : dataTableRecord [] =
        [| 
            let unsortedCount = this.sorterScores 
                                |> Seq.filter (SorterScore.isUnsorted >> Option.defaultValue false) 
                                |> Seq.length
            let dtrS = SorterEvalKey.toDataTableRecord this.sorterEvalKey
                       |> dataTableRecord.addKeyAndData "Sorted" "True"
                       |> dataTableRecord.addData "EvalCount" ((this.EvalCount - unsortedCount).ToString())
            dtrS;

            let dtrU = SorterEvalKey.toDataTableRecord this.sorterEvalKey
                       |> dataTableRecord.addKeyAndData "Sorted" "False"
                       |> dataTableRecord.addData "EvalCount" (unsortedCount.ToString())
            dtrU;
        |]


module SorterEvalBinV1 =
    
    /// Combines the bin key and each score into a sequence of records
    let makeDataTableRecords (bin: sorterEvalBinV1) : GeneSort.Core.dataTableRecord seq =
        let keyRecord = SorterEvalKey.toDataTableRecord bin.SorterEvalKey
        
        bin.Scores 
        |> Seq.map (fun score ->
            let scoreRecord = SorterScore.toDataTableRecord score
            // Merge the key data into the score data
            (scoreRecord, keyRecord.Data) 
            ||> Map.fold (fun acc k v -> GeneSort.Core.dataTableRecord.addData k v acc)
        )




type sorterEvalBin =
    | V1 of sorterEvalBinV1
    | Unknown
