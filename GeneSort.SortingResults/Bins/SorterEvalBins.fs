namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System

type sorterEvalBins =
    private {
        sorterEvalBinsId: Guid<sorterEvalBinsId>
        layers: Map<sorterEvalKey, sorterEvalLeaf>
    }
    with
    static member create (id: Guid<sorterEvalBinsId>) (sorterEvals: sorterEval seq) =
        let initial = { sorterEvalBinsId = id; layers = Map.empty }
        (initial, sorterEvals) ||> Seq.fold (fun acc eval -> acc.AddSorterEval eval)

    static member createWithNewId (sorterEvals: sorterEval seq) =
        let id = Guid.NewGuid() |> UMX.tag<sorterEvalBinsId>
        sorterEvalBins.create id sorterEvals    

    static member createEmpty (id: Guid<sorterEvalBinsId>) =
         { sorterEvalBinsId = id; layers = Map.empty }

    member this.SorterEvalBinsId = this.sorterEvalBinsId
    member this.Layers = this.layers 

    member this.EvalCount =
        this.layers |> Map.toSeq |> Seq.sumBy (fun (_, leaf) -> leaf.EvalCount)

    member this.AddSorterEval (sorterEval: sorterEval) =
        let key =
            sorterEvalKey.create
                sorterEval.CeBlockEval.CeUseCounts.UsedCeCount
                sorterEval.CeBlockEval.getStageSequence.StageLength
                (sorterEval.CeBlockEval.UnsortedCount = 0<sortableCount>)
        
        let newLeaf = 
            match Map.tryFind key this.layers with
            | Some existing -> existing.AddId(sorterEval.SorterId)
            | None -> sorterEvalLeaf.create sorterEval key
            
        { this with layers = Map.add key newLeaf this.layers }

    /// Merges a leaf into the bins, preserving chronological order
    member this.MergeLeaf (key: sorterEvalKey) (leaf: sorterEvalLeaf) : sorterEvalBins =
        let mergedLeaf = 
            match Map.tryFind key this.layers with
            | Some existing -> sorterEvalLeaf.merge existing leaf
            | None -> leaf
            
        { this with layers = Map.add key mergedLeaf this.layers }

    member this.AddLeafs (sourceLayers: seq<sorterEvalKey * sorterEvalLeaf>) : sorterEvalBins =
        (this, sourceLayers) ||> Seq.fold (fun acc (key, leaf) -> acc.MergeLeaf key leaf)

    member this.ConvexHull () : sorterEvalLeaf [] =
        let sortedPoints = 
            this.layers 
            |> Map.toSeq
            |> Seq.filter (fun (key, _) -> key.IsSorted)
            |> Seq.map (fun (key, leaf) -> 
                {| X = float (%key.CeCount)
                   Y = float (%key.StageLength)
                   Leaf = leaf |})
            |> Seq.sortBy (fun p -> p.X, p.Y)
            |> Seq.toArray

        if sortedPoints.Length = 0 then [||]
        elif sortedPoints.Length <= 2 then sortedPoints |> Array.map (fun p -> p.Leaf)
        else
            let crossProduct (o: {| X: float; Y: float; Leaf: sorterEvalLeaf |}) 
                             (a: {| X: float; Y: float; Leaf: sorterEvalLeaf |}) 
                             (b: {| X: float; Y: float; Leaf: sorterEvalLeaf |}) : float =
                (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X)

            let lower = ResizeArray()
            for p in sortedPoints do
                while lower.Count >= 2 && crossProduct lower.[lower.Count-2] lower.[lower.Count-1] p < 0.0 do
                    lower.RemoveAt(lower.Count - 1)
                lower.Add(p)

            let upper = ResizeArray()
            for i in (sortedPoints.Length - 1) .. -1 .. 0 do
                let p = sortedPoints.[i]
                while upper.Count >= 2 && crossProduct upper.[upper.Count-2] upper.[upper.Count-1] p < 0.0 do
                    upper.RemoveAt(upper.Count - 1)
                upper.Add(p)

            seq {
                yield! lower |> Seq.take (lower.Count - 1)
                yield! upper |> Seq.take (upper.Count - 1)
            }
            |> Seq.map (fun p -> p.Leaf)
            |> Seq.toArray

module SorterEvalBins =

    let merge (target: sorterEvalBins) (source: sorterEvalBins) : sorterEvalBins =
        source.Layers 
        |> Map.toSeq
        |> target.AddLeafs

    let getUpToNSorterIdsPerBin
            (orderFunc: sorterEvalKey -> float)
            (successfullySorted: bool)
            (maxPerBin: int)
            (source: sorterEvalBins)
            : (sorterEvalKey * Guid<sorterId>) seq =
        source.Layers
        |> Map.toSeq
        |> Seq.filter (fun (key, _) -> key.IsSorted = successfullySorted)
        |> Seq.sortBy (fun (key, _) -> orderFunc key)
        |> Seq.collect (fun (key, leaf) ->
                            leaf.SorterIds
                            |> Seq.truncate maxPerBin
                            |> Seq.map (fun id -> key, id))

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
                            |> Seq.map (fun id -> leaf.SorterEvalKey, id))


    // Returns sorter IDs from the bins; filtering by whether they are 
    // successfully sorted or not.
    // The returned IDs are ordered by proximity to the center of the distribution of ceCount
    // and stageLength, as determined by the orderFunc.
    let getAverageSorterIds
            (orderFunc: sorterEvalKey -> float)
            (successfullySorted: bool)
            (source: sorterEvalBins)
            : (sorterEvalKey * Guid<sorterId>) seq =
        
        let relevantLayers = 
            source.Layers
            |> Map.toSeq
            |> Seq.filter (fun (k, _) -> k.IsSorted = successfullySorted)
            |> Seq.toArray

        if relevantLayers.Length = 0 then
            Seq.empty
        else
            // 1. Calculate the weighted average score based on bin density (EvalCount)
            let totalWeight = relevantLayers |> Array.sumBy (fun (_, leaf) -> float leaf.EvalCount)
            let weightedSum = 
                relevantLayers 
                |> Array.sumBy (fun (k, leaf) -> orderFunc k * float leaf.EvalCount)
            
            let averageScore = weightedSum / totalWeight

            // 2. Sort bins by proximity to the average score
            // 3. Collect IDs from those bins until maxReturned is reached
            relevantLayers
            |> Seq.sortBy (fun (k, _) -> Math.Abs(orderFunc k - averageScore))
            |> Seq.collect (fun (k, leaf) -> 
                leaf.SorterIds 
                |> Seq.map (fun id -> k, id))



    // Returns sorter IDs from the bins; filtering by whether they are 
    // successfully sorted or not.
    // The returned have the lowest values of ceCount and stageLength,
    // as determined by the orderFunc.
    let getWinningSorterIds
            (orderFunc: sorterEvalKey -> float)
            (successfullySorted: bool)
            (source: sorterEvalBins)
            : (sorterEvalKey * Guid<sorterId>) seq =

        source.Layers
        |> Map.toSeq
        // 1. Filter for success/failure status
        |> Seq.filter (fun (key, _) -> key.IsSorted = successfullySorted)
        // 2. Sort the bins by the orderFunc (lowest score wins)
        |> Seq.sortBy (fun (key, _) -> orderFunc key)
        // 3. Expand the IDs within those bins
        |> Seq.collect (fun (key, leaf) ->
            leaf.SorterIds 
            |> Seq.map (fun id -> key, id))

        


    let getBinsReport
            (prefixes:string [])
            (bins: sorterEvalBins)
            : string[][] =
        bins.Layers
        |> Map.toSeq
        |> Seq.map (fun (key, leaf) ->
            prefixes |> Array.append
                [|
                    (%key.CeCount).ToString()
                    (%key.StageLength).ToString()
                    key.IsSorted.ToString()
                    leaf.EvalCount.ToString()
                |])
        |> Seq.toArray


    let getPropertyMaps<'t> 
                (bins: sorterEvalBins) 
                (baseKey:'t) 
                (baseProperties: Map<string, string>) 
                    : (('t * sorterEvalKey) * Map<string, string>) seq =
        bins.Layers
        |> Map.toSeq
        |> Seq.map (fun (key, leaf) ->
            let combinedMap = leaf.combineMap baseProperties
            ((baseKey, key), combinedMap))