namespace GeneSort.Eval.V1.Bins

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps

type sorterEvalBin =
    private {
        /// Mutable for O(1) additions during the evaluation phase
        sorterEvals: ResizeArray<sorterEval>
        sorterEvalKey: sorterEvalKey
    }

    static member create (score: sorterEval) (key : sorterEvalKey) =
        let sorterEvals = ResizeArray<sorterEval>()
        sorterEvals.Add(score)
        { 
            sorterEvals = sorterEvals
            sorterEvalKey = key
        }

    static member createWithSorterEvals (scores: sorterEval seq) (key : sorterEvalKey) =
        { 
            sorterEvals = ResizeArray(scores) 
            sorterEvalKey = key
        }

    member this.EvalCount with get() = this.sorterEvals.Count
    member this.SorterEvalKey with get() = this.sorterEvalKey
    member this.SorterEvals with get() = this.sorterEvals :> IReadOnlyList<sorterEval>
    member this.SortedCount with get() = 
        this.sorterEvals |> Seq.filter (SorterEval.getIsSorted) |> Seq.length
    member this.UnsortedCount with get() = 
        this.sorterEvals |> Seq.filter (SorterEval.getIsUnSorted) |> Seq.length
    /// Appends a score to the existing bin (Mutable Addition)
    member this.AddSorterEval (sorterEval: sorterEval) =
        this.sorterEvals.Add(sorterEval)


module SorterEvalBin =
    let toDataTableRecord (bin: sorterEvalBin) : GeneSort.Core.dataTableRecord =
        let keyRecord = SorterEvalKey.toDataTableRecord bin.SorterEvalKey
        let evalCountRecord = dataTableRecord.addData "SortedCount" (bin.SortedCount.ToString()) keyRecord
        let unsortedCountRecord = dataTableRecord.addData "UnsortedCount" (bin.UnsortedCount.ToString()) evalCountRecord
        unsortedCountRecord