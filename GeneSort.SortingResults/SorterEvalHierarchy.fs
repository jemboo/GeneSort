namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System
open GeneSort.Sorting.Sorter

[<Struct; StructuralEquality; NoComparison>]
type sorterEvalKey =
    private {
        ceCount:     int<ceLength>
        stageLength: int<stageLength>
        isSorted:    bool
    }
    static member create (ceCount: int<ceLength>) (stageLength: int<stageLength>) (isSorted: bool) =
        { ceCount = ceCount; stageLength = stageLength; isSorted = isSorted }
    member this.CeCount     with get() : int<ceLength>    = this.ceCount
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.IsSorted    with get() : bool             = this.isSorted


module SorterEvalKey =

    /// Raw ce count as a float
    let byCeCount (key: sorterEvalKey) : float =
        key.CeCount |> UMX.untag |> float

    /// Raw stage length as a float
    let byStageLength (key: sorterEvalKey) : float =
        key.StageLength |> UMX.untag |> float

    /// Ratio of ces to stages — lower means more ces are packed per stage
    let byCePerStage (key: sorterEvalKey) : float =
        let stages = key.StageLength |> UMX.untag |> float
        if stages = 0.0 then infinity
        else (key.CeCount |> UMX.untag |> float) / stages

    /// Sorted layers first, unsorted last
    let byIsSorted (key: sorterEvalKey) : float =
        if key.IsSorted then 0.0 else 1.0

    /// Primary: isSorted ascending, secondary: ceCount ascending
    let bySortedThenCeCount (key: sorterEvalKey) : float =
        let sortedScore = if key.IsSorted then 0.0 else 1.0e6
        sortedScore + (key.CeCount |> UMX.untag |> float)

    /// Primary: isSorted ascending, secondary: stageLength ascending
    let bySortedThenStageLength (key: sorterEvalKey) : float =
        let sortedScore = if key.IsSorted then 0.0 else 1.0e6
        sortedScore + (key.StageLength |> UMX.untag |> float)

    /// Weighted combination of ceCount and stageLength — lower values sort first
    let byWeighted (ceCountWeight: float) (stageLengthWeight: float) (key: sorterEvalKey) : float =
        ceCountWeight     * (key.CeCount     |> UMX.untag |> float) +
        stageLengthWeight * (key.StageLength |> UMX.untag |> float)

    /// Primary: isSorted ascending; secondary: weighted ceCount and stageLength
    let bySortedThenWeighted (ceCountWeight: float) (stageLengthWeight: float) (key: sorterEvalKey) : float =
        let sortedScore = if key.IsSorted then 0.0 else 1.0e6
        sortedScore + byWeighted ceCountWeight stageLengthWeight key


// ---------------------------------------------------------------------------
// Leaf: stores only the sorterIds that share a distinct ce-sequence
// ---------------------------------------------------------------------------
type sorterEvalLeaf =
    private {
        sorterIds: ResizeArray<Guid<sorterId>>
    }

    static member create (eval: sorterEval) =
        let ids = ResizeArray<Guid<sorterId>>()
        ids.Add(eval.SorterId)
        { sorterIds = ids }

    static member createWithIds (ids: Guid<sorterId> []) =
        { sorterIds = ResizeArray(ids) }

    member this.EvalCount with get() = this.sorterIds.Count
    member this.SorterIds with get() = this.sorterIds :> IReadOnlyList<Guid<sorterId>>

    // Intentionally mutable: sorterEvalLeaf uses a ResizeArray for performance
    member this.AddId (sorterId: Guid<sorterId>) =
        this.sorterIds.Add(sorterId)


// ---------------------------------------------------------------------------
// Layer: groups leaves that share the same sorterEvalKey (ceCount, stageLength, isSorted)
// ---------------------------------------------------------------------------
type sorterEvalLayer =
    private {
        key:    sorterEvalKey
        leaves: Dictionary<ceSequenceKey, sorterEvalLeaf>
    }

    static member create (key: sorterEvalKey) =
        {
            key    = key
            leaves = Dictionary<ceSequenceKey, sorterEvalLeaf>()
        }

    member this.Key         with get() = this.key
    member this.CeCount     with get() = this.key.CeCount
    member this.StageLength with get() = this.key.StageLength
    member this.IsSorted    with get() = this.key.IsSorted

    member this.EvalCount with get() =
        this.leaves.Values |> Seq.sumBy (fun l -> l.EvalCount)

    member this.Leaves with get() =
        this.leaves :> IReadOnlyDictionary<ceSequenceKey, sorterEvalLeaf>

    // Returns sorterIds from all leaves interleaved in round-robin order,
    // so that early items in the sequence sample broadly across distinct
    // ce-sequences rather than exhausting one leaf before moving to the next.
    // Leaves are visited in ascending order of EvalCount so that the
    // least-populated leaves (rarest ce-sequences) appear first.
    member this.GetAllSorterIds : Guid<sorterId> seq =
        seq {
            let columns =
                this.leaves.Values
                |> Seq.sortBy (fun l -> l.EvalCount)
                |> Seq.map (fun l -> l.SorterIds)
                |> Seq.toArray
            let mutable row = 0
            let mutable remaining = columns.Length
            while remaining > 0 do
                remaining <- 0
                for col in columns do
                    if row < col.Count then
                        yield col.[row]
                    if row + 1 < col.Count then
                        remaining <- remaining + 1
                row <- row + 1
        }

    member internal this.Add (eval: sorterEval) =
        let ceSeqKey = ceSequenceKey.create eval.CeBlockEval.UsedCes
        match this.leaves.TryGetValue(ceSeqKey) with
        | true, existing -> existing.AddId(eval.SorterId)
        | false, _       -> this.leaves.[ceSeqKey] <- sorterEvalLeaf.create(eval)

    member this.MergeLeaf (ceSeqKey: ceSequenceKey) (leaf: sorterEvalLeaf) =
        match this.leaves.TryGetValue(ceSeqKey) with
        | true, existing -> for id in leaf.SorterIds do existing.AddId(id)
        | false, _       -> this.leaves.[ceSeqKey] <- leaf


// ---------------------------------------------------------------------------
// Hierarchy: top-level container, keyed by sorterEvalKey
// ---------------------------------------------------------------------------
type sorterEvalHierarchy =
    private {
        sorterEvalHierarchyId: Guid<sorterEvalHierarchyId>
        layers:                Dictionary<sorterEvalKey, sorterEvalLayer>
    }

    static member create (id: Guid<sorterEvalHierarchyId>) =
        {
            sorterEvalHierarchyId = id
            layers                = Dictionary<sorterEvalKey, sorterEvalLayer>()
        }

    member this.SorterEvalHierarchyId with get() = this.sorterEvalHierarchyId
    member this.Layers                with get() = this.layers :> IReadOnlyDictionary<sorterEvalKey, sorterEvalLayer>

    member internal this.AddSorterEval (sorterEval: sorterEval) =
        let key = sorterEvalKey.create
                        sorterEval.CeBlockEval.CeUseCounts.UsedCeCount
                        sorterEval.CeBlockEval.getStageSequence.StageLength
                        (sorterEval.CeBlockEval.UnsortedCount = 0<sortableCount>)
        let layer =
            match this.layers.TryGetValue(key) with
            | true, existing -> existing
            | false, _ ->
                let newLayer = sorterEvalLayer.create(key)
                this.layers.[key] <- newLayer
                newLayer
        layer.Add(sorterEval)

    member this.AddSorterEvals (sorterEvals: sorterEval seq) =
        sorterEvals |> Seq.iter this.AddSorterEval

    member this.MergeLayer (layer: sorterEvalLayer) =
        let existing =
            match this.layers.TryGetValue(layer.Key) with
            | true, existing -> existing
            | false, _ ->
                let newLayer = sorterEvalLayer.create(layer.Key)
                this.layers.[layer.Key] <- newLayer
                newLayer
        for kvp in layer.Leaves do
            existing.MergeLeaf kvp.Key kvp.Value


module SorterEvalHierarchy =

    let create () : sorterEvalHierarchy =
        sorterEvalHierarchy.create (Guid.NewGuid() |> UMX.tag<sorterEvalHierarchyId>)

    let createFromSorterSetEval (sorterSetEval: sorterSetEval) : sorterEvalHierarchy =
        let hierarchy = create ()
        sorterSetEval.SorterEvals |> Array.iter hierarchy.AddSorterEval
        hierarchy

    let getHierarchyReport (sortingWidth: int<sortingWidth> option) (sorterModelKey: string) (hierarchy: sorterEvalHierarchy) =
        let widthStr = sortingWidth |> Option.map (fun w -> (%w).ToString()) |> Option.defaultValue "N/A"
        hierarchy.Layers
        |> Seq.collect (fun kvp ->
            kvp.Value.Leaves
            |> Seq.map (fun leafKvp ->
                [|
                    widthStr
                    sorterModelKey
                    (%kvp.Value.CeCount).ToString()
                    (%kvp.Value.StageLength).ToString()
                    kvp.Value.IsSorted.ToString()
                    leafKvp.Value.EvalCount.ToString()
                |]))
        |> Seq.toArray

    let merge (target: sorterEvalHierarchy) (source: sorterEvalHierarchy) =
        for kvp in source.Layers do target.MergeLayer kvp.Value
        target

    // Returns up to maxSorterIds sorterIds from each layer, where:
    // - layers are visited in ascending order of orderFunc applied to their sorterEvalKey
    // - within each layer, ids are drawn via round-robin across leaves (see GetAllSorterIds),
    //   so the first ids returned from each layer sample the broadest variety of ce-sequences
    // - the result is a flat sequence of (sorterEvalKey * Guid<sorterId>) pairs so callers
    //   know which layer each id came from
    let getUpToNSorterIdsPerLayer
            (orderFunc:   sorterEvalKey -> float)
            (maxSorterIds: int)
            (source:      sorterEvalHierarchy)
            : (sorterEvalKey * Guid<sorterId>) seq =
        source.Layers
        |> Seq.sortBy (fun kvp -> orderFunc kvp.Key)
        |> Seq.collect (fun kvp ->
            kvp.Value.GetAllSorterIds
            |> Seq.truncate maxSorterIds
            |> Seq.map (fun id -> kvp.Key, id))