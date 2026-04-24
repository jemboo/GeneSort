

namespace GeneSort.Eval.V1.Bins

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Core

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

    /// Appends a score to the existing bin (Mutable Addition)
    member this.AddScore (score: sorterScore) =
        this.sorterScores.Add(score)

    member this.ToDataTableRecord() : dataTableRecord =
        SorterEvalKey.toDataTableRecord this.sorterEvalKey
        |> dataTableRecord.addData "EvalCount" (this.EvalCount.ToString())



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
