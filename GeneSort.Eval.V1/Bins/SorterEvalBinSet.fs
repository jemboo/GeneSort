namespace GeneSort.Eval.V1.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps

[<Measure>] type sorterEvalBinSetId

type sorterEvalBinSet =
    private {
        sorterEvalBinSetId: Guid<sorterEvalBinSetId>
        sorterEvalBins: Map<sorterEvalKey, sorterEvalBin>
        sortableTestId: Guid<sortableTestId>
    }
    with
    static member create (id: Guid<sorterEvalBinSetId>)  
                         (sortableTestId: Guid<sortableTestId>) =
        {
            sorterEvalBinSetId = id
            sorterEvalBins = Map.empty
            sortableTestId = sortableTestId
        }

    static member createFromSorterEvals (id: Guid<sorterEvalBinSetId>)  
                         (sortableTestId: Guid<sortableTestId>) 
                         (sorterEvals: sorterEval seq) =
        let bins = 
            sorterEvals
            |> Seq.groupBy SorterEvalKey.fromSorterEval
            |> Seq.map (fun (key, evals) -> 
                let bin = sorterEvalBin.createWithSorterEvals evals key
                (key, bin))
            |> Map.ofSeq
        {
            sorterEvalBinSetId = id
            sortableTestId = sortableTestId
            sorterEvalBins = bins
        }

    member this.SorterEvalBinSetId with get() = this.sorterEvalBinSetId
    member this.Bins with get() = this.sorterEvalBins
    member this.SortableTestId with get() = this.sortableTestId


module SorterEvalBinSet = 

    // returns one dataTableRecord for each member of sorterEvalBins
    let makeDataTableRecords (source: sorterEvalBinSet) : GeneSort.Core.dataTableRecord seq =
        source.Bins
        |> Seq.map (fun kvp -> SorterEvalBin.toDataTableRecord kvp.Value)
