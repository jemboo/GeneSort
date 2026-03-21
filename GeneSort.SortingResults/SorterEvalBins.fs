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
// Bins: flat container — one leaf per sorterEvalKey (ceCount, stageLength, isSorted)
// Collapses the ceSequenceKey dimension; sorterIds from all ce-sequences
// that share a key are pooled into a single leaf.
// ---------------------------------------------------------------------------
type sorterEvalBins =
    private {
        sorterEvalBinsId: Guid<sorterEvalBinsId>
        layers:           Dictionary<sorterEvalKey, sorterEvalLeaf>
    }

    static member create (id: Guid<sorterEvalBinsId>) =
        {
            sorterEvalBinsId = id
            layers           = Dictionary<sorterEvalKey, sorterEvalLeaf>()
        }

    member this.SorterEvalBinsId with get() = this.sorterEvalBinsId
    member this.Layers           with get() = this.layers :> IReadOnlyDictionary<sorterEvalKey, sorterEvalLeaf>

    member this.EvalCount with get() =
        this.layers.Values |> Seq.sumBy (fun l -> l.EvalCount)

    member internal this.AddSorterEval (sorterEval: sorterEval) =
        let key =
            sorterEvalKey.create
                sorterEval.CeBlockEval.CeUseCounts.UsedCeCount
                sorterEval.CeBlockEval.getStageSequence.StageLength
                (sorterEval.CeBlockEval.UnsortedCount = 0<sortableCount>)
        match this.layers.TryGetValue(key) with
        | true, existing -> existing.AddId(sorterEval.SorterId)
        | false, _       -> this.layers.[key] <- sorterEvalLeaf.create(sorterEval)

    member this.AddSorterEvals (sorterEvals: sorterEval seq) =
        sorterEvals |> Seq.iter this.AddSorterEval

    member this.MergeLeaf (key: sorterEvalKey) (leaf: sorterEvalLeaf) =
        match this.layers.TryGetValue(key) with
        | true, existing -> for id in leaf.SorterIds do existing.AddId(id)
        | false, _       -> this.layers.[key] <- sorterEvalLeaf.createWithIds(leaf.SorterIds |> Seq.toArray)

    member this.MergeBins (source: sorterEvalBins) =
        for kvp in source.Layers do
            this.MergeLeaf kvp.Key kvp.Value


module SorterEvalBins =

    let create () : sorterEvalBins =
        sorterEvalBins.create (Guid.NewGuid() |> UMX.tag<sorterEvalBinsId>)

    let createFromSorterSetEval (sorterSetEval: sorterSetEval) : sorterEvalBins =
        let bins = create ()
        sorterSetEval.SorterEvals |> Array.iter bins.AddSorterEval
        bins

    let merge (target: sorterEvalBins) (source: sorterEvalBins) : sorterEvalBins =
        target.MergeBins source
        target

    // Returns up to maxSorterIds sorterIds from each bin, where:
    // - bins are visited in ascending order of orderFunc applied to their sorterEvalKey
    // - result is a flat sequence of (sorterEvalKey * Guid<sorterId>) pairs
    let getUpToNSorterIdsPerBin
            (orderFunc:    sorterEvalKey -> float)
            (maxSorterIds: int)
            (source:       sorterEvalBins)
            : (sorterEvalKey * Guid<sorterId>) seq =
        source.Layers
        |> Seq.sortBy (fun kvp -> orderFunc kvp.Key)
        |> Seq.collect (fun kvp ->
            kvp.Value.SorterIds
            |> Seq.truncate maxSorterIds
            |> Seq.map (fun id -> kvp.Key, id))

    let getBinsReport
            (sortingWidth:   int<sortingWidth> option)
            (sorterModelKey: string)
            (bins:           sorterEvalBins)
            : string[][] =
        let widthStr =
            sortingWidth
            |> Option.map (fun w -> (%w).ToString())
            |> Option.defaultValue "N/A"
        bins.Layers
        |> Seq.map (fun kvp ->
            [|
                widthStr
                sorterModelKey
                (%kvp.Key.CeCount).ToString()
                (%kvp.Key.StageLength).ToString()
                kvp.Key.IsSorted.ToString()
                kvp.Value.EvalCount.ToString()
            |])
        |> Seq.toArray