namespace GeneSort.Eval.V1.Bins

open FSharp.UMX
open GeneSort.SortingOps

[<Measure>] type sorterEvalBinSetId

type sorterEvalBinSet =
    private {
        sorterEvalBinSetId: Guid<sorterEvalBinSetId>
        sorterSetEvalId: Guid<sorterSetEvalId>
        sorterEvalBins: Map<sorterEvalKey, sorterEvalBin>
    }
    with
    /// Creates a bin set directly from a parent sorterSetEval
    static member create (id: Guid<sorterEvalBinSetId>) (setEval: sorterSetEval) =
        let bins = 
            setEval.SorterEvals
            |> Seq.groupBy SorterEvalKey.fromSorterEval
            |> Seq.map (fun (key, evals) -> 
                let bin = sorterEvalBin.createWithSorterEvals evals key
                (key, bin))
            |> Map.ofSeq

        {
            sorterEvalBinSetId = id
            sorterSetEvalId = setEval.SorterSetEvalId
            sorterEvalBins = bins
        }

    /// Explicit reconstructor for persistence/DTO deserialization
    static member recreate (id: Guid<sorterEvalBinSetId>) 
                            (sorterSetEvalId: Guid<sorterSetEvalId>) 
                            (bins: Map<sorterEvalKey, sorterEvalBin>) =
        {
            sorterEvalBinSetId = id
            sorterSetEvalId = sorterSetEvalId
            sorterEvalBins = bins
        }

    member this.SorterEvalBinSetId with get() = this.sorterEvalBinSetId
    member this.SorterSetEvalId with get() = this.sorterSetEvalId
    member this.Bins with get() = this.sorterEvalBins


module SorterEvalBinSet = 

    let makeDataTableRecords (source: sorterEvalBinSet) : GeneSort.Core.dataTableRecord seq =
        source.Bins
        |> Seq.map (fun kvp -> SorterEvalBin.toDataTableRecord kvp.Value)