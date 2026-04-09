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
type sorterEvalLeafOld =
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




type sorterEvalLeaf =
    private {
        /// Internal storage is [Newest; ...; Oldest] for O(1) prepending
        sorterIds: Guid<sorterId> list
        sorterEvalKey: sorterEvalKey
    }

    static member create (eval: sorterEval) (key : sorterEvalKey) =
        { 
            sorterIds = [eval.SorterId]
            sorterEvalKey = key
        }

    static member createWithIds (ids: Guid<sorterId> seq) (key : sorterEvalKey) =
        { 
            // Expects ids in chronological order; reverses them for internal storage
            sorterIds = ids |> Seq.toList |> List.rev
            sorterEvalKey = key
        }

    member this.EvalCount = this.sorterIds.Length
    member this.SorterEvalKey = this.sorterEvalKey
    
    /// Returns IDs in the order they were added (Oldest to Newest)
    member this.SorterIds = 
        this.sorterIds |> List.rev |> List.toSeq

    /// Returns a NEW leaf with the added ID
    member this.AddId (sorterId: Guid<sorterId>) : sorterEvalLeaf =
        { this with sorterIds = sorterId :: this.sorterIds }

    /// Merges two leaves: [Target IDs] then [Source IDs]
    static member merge (target: sorterEvalLeaf) (source: sorterEvalLeaf) =
        if target.sorterEvalKey <> source.sorterEvalKey then 
            failwith "Key mismatch"
        { target with 
            // source.sorterIds are the "newer" items, so they sit at the head
            sorterIds = source.sorterIds @ target.sorterIds }