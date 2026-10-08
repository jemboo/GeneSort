namespace GeneSort.Eval.V1.Bins.Standard

open System.Collections.Generic
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Eval.V1.Bins

type sorterEvalBin =
    private {
        /// Mutable for O(1) additions during the evaluation phase
        sorterEvals: ResizeArray<sorterEval>
        sorterEvalKey: sorterEvalKey
    }

    static member create (score: sorterEval) (key : sorterEvalKey) =
        let sorterEvals = ResizeArray<sorterEval>()
        sorterEvals.Add(score |> SorterEval.downgradeTo sorterEvalType.V1)
        { 
            sorterEvals = sorterEvals
            sorterEvalKey = key
        }

    static member createWithSorterEvals (scores: sorterEval seq) (key : sorterEvalKey) =
        { 
            sorterEvals = ResizeArray(scores  |> Seq.map (SorterEval.downgradeTo sorterEvalType.V1))
            sorterEvalKey = key
        }

    member this.EvalCount with get() = this.sorterEvals.Count
    member this.SorterEvalKey with get() = this.sorterEvalKey
    member this.SorterEvals with get() = this.sorterEvals :> IReadOnlyList<sorterEval>
    member this.SortedCount with get() = 
        this.sorterEvals |> Seq.filter (fun eval -> SorterEval.getIsSorted eval) |> Seq.length
    member this.UnsortedCount with get() = 
        this.sorterEvals |> Seq.filter (fun eval -> SorterEval.getIsUnSorted eval) |> Seq.length
    /// Appends a score to the existing bin (Mutable Addition)
    member this.AddSorterEval (sorterEval: sorterEval) =
        this.sorterEvals.Add(sorterEval)


module SorterEvalBin =
    let toDataTableRecord (bin: sorterEvalBin) : GeneSort.Core.dataTableRecord =
        let keyRecord = SorterEvalKey.toDataTableRecord bin.SorterEvalKey
        let evalCountRecord = dataTableRecord.addData "SortedCount" (bin.SortedCount.ToString()) keyRecord
        let unsortedCountRecord = dataTableRecord.addData "UnsortedCount" (bin.UnsortedCount.ToString()) evalCountRecord
        unsortedCountRecord
