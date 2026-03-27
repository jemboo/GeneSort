namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System


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

    static member create 
                        (id: Guid<sorterEvalBinsId>) 
                        (sorterEvals: sorterEval seq) =
        let bins =
            {
                sorterEvalBinsId = id
                layers           = Dictionary<sorterEvalKey, sorterEvalLeaf>()
            }
        sorterEvals |> Seq.iter bins.AddSorterEval
        bins


    static member createWithNewId
                        (sorterEvals: sorterEval seq) =
        let bins =
            {
                sorterEvalBinsId = (Guid.NewGuid() |> UMX.tag<sorterEvalBinsId>)
                layers           = Dictionary<sorterEvalKey, sorterEvalLeaf>()
            }
        sorterEvals |> Seq.iter bins.AddSorterEval
        bins


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
        | false, _       -> this.layers.[key] <- 
                                sorterEvalLeaf.create 
                                            (sorterEval) 
                                            key

    member this.AddSorterEvals (sorterEvals: sorterEval seq) =
        sorterEvals |> Seq.iter this.AddSorterEval

    member this.MergeLeaf (key: sorterEvalKey) (leaf: sorterEvalLeaf) =
        match this.layers.TryGetValue(key) with
        | true, existing -> for id in leaf.SorterIds do existing.AddId(id)
        | false, _       -> 
                    this.layers.[key] <- sorterEvalLeaf.createWithIds 
                                            (leaf.SorterIds |> Seq.toArray) 
                                            key


    member this.AddLeafs (sourceLayers: IEnumerable<KeyValuePair<sorterEvalKey, sorterEvalLeaf>>) =
            for kvp in sourceLayers do
                this.MergeLeaf kvp.Key kvp.Value


    /// Returns an array of leaves, where each leaf represents a vertex 
    /// on the convex hull of the (CeCount, StageLength) lattice for isSorted = true.
    member this.ConvexHull () : sorterEvalLeaf [] =
        // 1. Filter for sorted bins and extract points (x=Ce, y=Stage)
        let sortedPoints = 
            this.layers 
            |> Seq.filter (fun kvp -> kvp.Key.IsSorted)
            |> Seq.map (fun kvp -> 
                {| X = float (%kvp.Key.CeCount)
                   Y = float (%kvp.Key.StageLength)
                   Leaf = kvp.Value |})
            |> Seq.sortBy (fun p -> p.X, p.Y)
            |> Seq.toArray

        if sortedPoints.Length = 0 then 
            [||]
        elif sortedPoints.Length <= 2 then
            sortedPoints |> Array.map (fun p -> p.Leaf)
        else
            // 2D cross product of OA and OB vectors
            // Returns a positive value for a left turn, 
            // negative for a right turn, and 0 if collinear.
            let crossProduct (o: {| X: float; Y: float; Leaf: sorterEvalLeaf |}) 
                             (a: {| X: float; Y: float; Leaf: sorterEvalLeaf |}) 
                             (b: {| X: float; Y: float; Leaf: sorterEvalLeaf |}) : float =
                (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X)

            // Build Lower Hull
            let lower = ResizeArray()
            for p in sortedPoints do
                while lower.Count >= 2 && crossProduct lower.[lower.Count-2] lower.[lower.Count-1] p < 0.0 do
                    lower.RemoveAt(lower.Count - 1)
                lower.Add(p)

            // Build Upper Hull
            let upper = ResizeArray()
            for i in (sortedPoints.Length - 1) .. -1 .. 0 do
                let p = sortedPoints.[i]
                while upper.Count >= 2 && crossProduct upper.[upper.Count-2] upper.[upper.Count-1] p < 0.0 do
                    upper.RemoveAt(upper.Count - 1)
                upper.Add(p)

            // Combine hulls, removing the last point of each because it's repeated
            let hullVertices = 
                seq {
                    yield! lower |> Seq.take (lower.Count - 1)
                    yield! upper |> Seq.take (upper.Count - 1)
                }

            hullVertices 
            |> Seq.map (fun p -> p.Leaf)
            |> Seq.toArray




module SorterEvalBins =

    let merge (target: sorterEvalBins) (source: sorterEvalBins) : sorterEvalBins =
        target.AddLeafs source.Layers
        target

    // Returns up to maxSorterIds sorterIds from each bin, where:
    // bins are filtered by their isSorted property.
    // - bins are visited in ascending order of orderFunc applied to their sorterEvalKey
    // - result is a flat sequence of (sorterEvalKey * Guid<sorterId>) pairs
    let getUpToNSorterIdsPerBin
            (orderFunc: sorterEvalKey -> float)
            (successfullySorted: bool)
            (maxPerBin: int)
            (source: sorterEvalBins)
            : (sorterEvalKey * Guid<sorterId>) seq =
        source.Layers
        |> Seq.filter (fun kvp -> kvp.Key.IsSorted = successfullySorted)
        |> Seq.sortBy (fun kvp -> orderFunc kvp.Key)
        |> Seq.collect (fun kvp ->
                            kvp.Value.SorterIds
                            |> Seq.truncate maxPerBin
                            |> Seq.map (fun id -> kvp.Key, id)
                       )


    let getUpToNSorterIdsPerConvexHullBin
            (orderFunc: sorterEvalKey -> float)
            (successfullySorted: bool)
            (maxPerBin: int)
            (source: sorterEvalBins)
            : (sorterEvalKey * Guid<sorterId>) seq =
        source.ConvexHull ()
        |> Seq.filter (fun leaf -> leaf.SorterEvalKey.IsSorted = successfullySorted)
        |> Seq.sortBy (fun leaf -> orderFunc leaf.SorterEvalKey)
        |> Seq.collect (fun leaf ->
                            leaf.SorterIds
                            |> Seq.truncate maxPerBin
                            |> Seq.map (fun id -> leaf.SorterEvalKey, id)
                       )


    // reporting
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