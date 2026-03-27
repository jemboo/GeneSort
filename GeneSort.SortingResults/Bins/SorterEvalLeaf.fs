namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System
open GeneSort.Sorting.Sorter


// ---------------------------------------------------------------------------
// Leaf: stores only the sorterIds that share a distinct ce-sequence
// ---------------------------------------------------------------------------
type sorterEvalLeaf =
    private {
        sorterIds: ResizeArray<Guid<sorterId>>
        sorterEvalKey: sorterEvalKey
    }

    static member create (eval: sorterEval) (key : sorterEvalKey) =
        let ids = ResizeArray<Guid<sorterId>>()
        ids.Add(eval.SorterId)
        { 
            sorterIds = ids;
            sorterEvalKey = key
        }

    static member createWithIds (ids: Guid<sorterId> []) (key : sorterEvalKey) =
        { 
            sorterIds = ResizeArray(ids) 
            sorterEvalKey = key
        }

    member this.EvalCount with get() = this.sorterIds.Count
    member this.SorterEvalKey with get() = this.sorterEvalKey
    member this.SorterIds with get() = this.sorterIds :> IReadOnlyList<Guid<sorterId>>

    // Intentionally mutable: sorterEvalLeaf uses a ResizeArray for performance
    member this.AddId (sorterId: Guid<sorterId>) =
        this.sorterIds.Add(sorterId)


