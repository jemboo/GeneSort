namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System

[<Struct; StructuralEquality; NoComparison>]
type sorterEvalKey =
    private {
        ceCount: int<ceLength>
        stageLength: int<stageLength>
        isSorted: bool
    }
    static member create (ceCount:int<ceLength>) (stageLength: int<stageLength>) (isSorted: bool) = 
          { ceCount = ceCount; stageLength = stageLength; isSorted = isSorted }
    member this.CeCount with get() : int<ceLength> = this.ceCount
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.IsSorted with get() : bool = this.isSorted

// ---------------------------------------------------------------------------
// Leaf: Bottom layer. Now contains at most ONE optional sorterEval.
// ---------------------------------------------------------------------------
type sorterEvalLeaf =
    private {
        sorterEval: sorterEval option
        sorterIds: ResizeArray<Guid<sorterId>>
    }
    static member create(eval: sorterEval, includeEval: bool) =
        let ids = ResizeArray<Guid<sorterId>>()
        ids.Add(eval.SorterId)
        { 
            sorterEval = if includeEval then Some eval else None
            sorterIds = ids 
        }

    member this.EvalCount with get() = this.sorterIds.Count
    member this.SorterEval with get() = this.sorterEval
    member this.SorterIds with get() = this.sorterIds :> IReadOnlyList<Guid<sorterId>>

    member this.AddId(sorterId: Guid<sorterId>) = 
        this.sorterIds.Add(sorterId)

// ---------------------------------------------------------------------------
// Layer: Tier 1. maxReps constrains how many leaves can hold a sorterEval.
// ---------------------------------------------------------------------------
type sorterEvalLayer =
    private {
        leaves: Dictionary<int, sorterEvalLeaf>
    }
    static member create() =
        { leaves = Dictionary<int, sorterEvalLeaf>() }
    
    member this.EvalCount with get() =
        this.leaves.Values |> Seq.sumBy (fun b -> b.EvalCount)
    
    member this.Leaves with get() = this.leaves :> IReadOnlyDictionary<int, sorterEvalLeaf>
    
    /// Counts how many leaves in this layer currently store a full sorterEval
    member private this.CurrentRepCount = 
        this.leaves.Values |> Seq.filter (fun l -> l.SorterEval.IsSome) |> Seq.length

    member internal this.Add(eval: sorterEval, maxReps: int, onLeafCreated: sorterEval -> unit) =
        let key = eval.CeBlockEval.CeUseCounts.GetHashCode()
        match this.leaves.TryGetValue(key) with
        | true, existing -> 
            existing.AddId(eval.SorterId)
        | false, _ ->
            // Only include the full eval if we haven't hit the layer-wide limit
            let includeEval = this.CurrentRepCount < maxReps
            this.leaves.[key] <- sorterEvalLeaf.create(eval, includeEval)
            // TRIGGER FOLLOW-UP ACTION
            onLeafCreated eval


    member this.MergeLeaf (hashKey: int) (leaf: sorterEvalLeaf, maxReps: int) =
        match this.leaves.TryGetValue(hashKey) with
        | true, existing -> 
            for id in leaf.SorterIds do existing.AddId(id)
        | false, _ ->
            // If the incoming leaf has an eval, but we are already at capacity, 
            // we must strip the eval to respect the maxReps constraint of this hierarchy.
            if leaf.SorterEval.IsSome && this.CurrentRepCount >= maxReps then
                let strippedLeaf = { leaf with sorterEval = None }
                this.leaves.[hashKey] <- strippedLeaf
            else
                this.leaves.[hashKey] <- leaf

// ---------------------------------------------------------------------------
// Hierarchy: Top level container
// ---------------------------------------------------------------------------
type sorterEvalHierarchy = 
    private {
        sorterEvalHierarchyId: Guid<sorterEvalHierarchyId>
        layers: Dictionary<sorterEvalKey, sorterEvalLayer>
        maxRepresentativesPerLayer: int
    }
    static member create (sorterSetEvalId: Guid<sorterEvalHierarchyId>, maxReps: int) =
        {
            sorterEvalHierarchyId = sorterSetEvalId
            layers = Dictionary<sorterEvalKey, sorterEvalLayer>()
            maxRepresentativesPerLayer = maxReps
        }

    member this.SorterEvalHierarchyId with get() = this.sorterEvalHierarchyId
    member this.Layers with get() = this.layers :> IReadOnlyDictionary<sorterEvalKey, sorterEvalLayer>
    member this.MaxRepresentativesPerLayer with get() = this.maxRepresentativesPerLayer
    
    member this.GetRepresentativeSorterEvals : sorterEval [] =
        this.layers.Values
        |> Seq.collect (fun layer -> 
            layer.Leaves.Values
            |> Seq.choose (fun leaf -> leaf.SorterEval))
        |> Seq.toArray

    member internal this.AddSorterEval (sorterEval: sorterEval, ?onLeafCreated: sorterEval -> unit) =
        let onLeafCreated = defaultArg onLeafCreated ignore
        let key = sorterEvalKey.create
                        sorterEval.CeBlockEval.CeUseCounts.UsedCeCount
                        sorterEval.CeBlockEval.getStageSequence.StageLength
                        (sorterEval.CeBlockEval.UnsortedCount = 0<sortableCount>)
        let layer =
            match this.layers.TryGetValue(key) with
            | true, existing -> existing
            | false, _ ->
                let newLayer = sorterEvalLayer.create()
                this.layers.[key] <- newLayer
                newLayer
        
        // Pass the action down to the layer
        layer.Add(sorterEval, this.maxRepresentativesPerLayer, onLeafCreated)


    member this.AddSorterEvals (sorterEvals: sorterEval seq, ?onLeafCreated: sorterEval -> unit) =
        let onAction = defaultArg onLeafCreated ignore
        sorterEvals |> Seq.iter (fun eval -> this.AddSorterEval(eval, onAction))


    member this.MergeLayer (key: sorterEvalKey) (layer: sorterEvalLayer) =
        let existing =
            match this.layers.TryGetValue(key) with
            | true, existing -> existing
            | false, _ ->
                let newLayer = sorterEvalLayer.create()
                this.layers.[key] <- newLayer
                newLayer
        for kvp in layer.Leaves do
            existing.MergeLeaf kvp.Key (kvp.Value, this.maxRepresentativesPerLayer)


module SorterEvalHierarchy =

    let create (maxReps: int) : sorterEvalHierarchy =
        sorterEvalHierarchy.create (Guid.NewGuid() |> UMX.tag<sorterEvalHierarchyId>, maxReps)

    let createFromSorterSetEval (maxReps: int) (sorterSetEval: sorterSetEval) : sorterEvalHierarchy =
        let hierarchy = create maxReps
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
                    (%kvp.Key.CeCount).ToString()
                    (%kvp.Key.StageLength).ToString()
                    kvp.Key.IsSorted.ToString()
                    leafKvp.Value.EvalCount.ToString()
                |]))
        |> Seq.toArray

    let merge (target: sorterEvalHierarchy) (source: sorterEvalHierarchy) =
        for kvp in source.Layers do target.MergeLayer kvp.Key kvp.Value
        target