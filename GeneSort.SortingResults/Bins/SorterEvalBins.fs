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