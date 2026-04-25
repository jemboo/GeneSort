
namespace GeneSort.Eval.V1.Bins

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.SortingOps

[<Measure>] type sorterEvalBinsId

type sorterEvalBinsV1 =
    private {
        sorterEvalBinsId: Guid<sorterEvalBinsId>
        bins: Map<sorterEvalKey, sorterEvalBinV1>
        sortableTestId: Guid<sortableTestId>
    }
    with
    static member createEmpty (id: Guid<sorterEvalBinsId>) (testId: Guid<sortableTestId>) =
        { 
            sorterEvalBinsId = id
            bins = Map.empty 
            sortableTestId = testId
        }

    static member create 
            (id: Guid<sorterEvalBinsId>) 
            (testId: Guid<sortableTestId>)
            (bins: Map<sorterEvalKey, sorterEvalBinV1>) =
        { 
            sorterEvalBinsId = id
            bins = bins
            sortableTestId = testId
        }

    static member createWithNewId (testId: Guid<sortableTestId>) =
        let id = Guid.NewGuid() |> UMX.tag<sorterEvalBinsId>
        sorterEvalBinsV1.createEmpty id testId

    member this.Id = this.sorterEvalBinsId
    member this.Bins = this.bins
    member this.SortableTestId = this.sortableTestId

    member this.EvalCount =
        this.bins |> Map.toSeq |> Seq.sumBy (fun (_, bin) -> bin.EvalCount)

    /// Adds a single sorter evaluation to the appropriate bin
    member this.AddSorterEval (eval: sorterEval) =
        let key = SorterEvalKey.fromSorterEval eval
        let score = SorterScore.fromSorterEval eval |> sorterScore.V1
        
        match Map.tryFind key this.bins with
        | Some existingBin -> 
            existingBin.AddScore(score)
            this // Map doesn't change because ResizeArray is mutated internally
        | None -> 
            let newBin = sorterEvalBinV1.create score key
            { this with bins = Map.add key newBin this.bins }

    /// Merges another bin into this one
    member this.MergeBin (key: sorterEvalKey) (sourceBin: sorterEvalBinV1) : sorterEvalBinsV1 =
        match Map.tryFind key this.bins with
        | Some existingBin -> 
            for score in sourceBin.Scores do existingBin.AddScore(score)
            this
        | None -> 
            { this with bins = Map.add key sourceBin this.bins }



module SorterEvalBinsV1 =
    
    let merge (target: sorterEvalBinsV1) (source: sorterEvalBinsV1) : sorterEvalBinsV1 =
        (target, source.Bins |> Map.toSeq) 
        ||> Seq.fold (fun acc (key, bin) -> acc.MergeBin key bin)

    /// Extracts all scores across all bins
    let getAllScores (bins: sorterEvalBinsV1) : sorterScore seq =
        bins.Bins 
        |> Map.toSeq 
        |> Seq.collect (fun (_, bin) -> bin.Scores)

    /// Creates a new sorterEvalBins containing only the scores that satisfy the filter.
    /// Bins that become empty after filtering are omitted from the result.
    let extractBins (scoreFilter: sorterScore -> bool) 
                    (source: sorterEvalBinsV1) : sorterEvalBinsV1 =

        let filteredId = Guid.NewGuid() |> UMX.tag<sorterEvalBinsId>
        
        let filteredLayers = 
            source.Bins
            |> Map.toSeq
            |> Seq.choose (fun (key, bin) ->
                // Filter the scores within the bin
                let matchedScores = 
                    bin.Scores 
                    |> Seq.filter scoreFilter 
                    |> Seq.toArray
                
                // If the bin has matching scores, create a new bin instance
                if matchedScores.Length > 0 then
                    let newBin = sorterEvalBinV1.createWithScores matchedScores key
                    Some (key, newBin)
                else
                    None)
            |> Map.ofSeq

        {source with bins = filteredLayers; sorterEvalBinsId = filteredId}


/// Returns a new sorterEvalBins containing only the top N scores globally.
    let getTopN 
            (scoreValuer: sorterScore -> float) 
            (newId : Guid<sorterEvalBinsId>) 
            (source: sorterEvalBinsV1)
            (n : int) : sorterEvalBinsV1 =
        
        let topScoresWithKeys =
            source.Bins
            |> Map.toSeq
            |> Seq.collect (fun (key, bin) -> bin.Scores |> Seq.map (fun s -> key, s))
            |> Seq.toArray
            |> Array.sortBy (fun (_, score) -> scoreValuer score)
            |> Array.truncate n

        let regroupedBins =
            topScoresWithKeys
            |> Seq.groupBy fst
            |> Seq.map (fun (key, group) ->
                let scores = group |> Seq.map snd |> Seq.toArray
                key, sorterEvalBinV1.createWithScores scores key)
            |> Map.ofSeq

        { source with 
            sorterEvalBinsId = newId
            bins = regroupedBins }

    /// Filters the bins down to the Pareto Frontier (Convex Hull) in (Gates * Depth) space.
    let convexHull
            (newId : Guid<sorterEvalBinsId>) 
            (source: sorterEvalBinsV1) : sorterEvalBinsV1 =
        
        let sortedPoints = 
            source.Bins 
            |> Map.toSeq
            |> Seq.map (fun (key, bin) -> 
                {| X = float %key.CeCount
                   Y = float %key.StageLength
                   Key = key
                   Bin = bin |})
            |> Seq.sortBy (fun p -> p.X, p.Y)
            |> Seq.toArray

        if sortedPoints.Length <= 2 then
            { source with sorterEvalBinsId = newId }
        else
            let crossProduct 
                (o: {| X: float; Y: float; Key: sorterEvalKey; Bin: sorterEvalBinV1 |}) 
                (a: {| X: float; Y: float; Key: sorterEvalKey; Bin: sorterEvalBinV1 |}) 
                (b: {| X: float; Y: float; Key: sorterEvalKey; Bin: sorterEvalBinV1 |}) =
                (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X)

            // Monotone Chain: Lower Hull
            let lower = ResizeArray()
            for p in sortedPoints do
                while lower.Count >= 2 && crossProduct lower.[lower.Count-2] lower.[lower.Count-1] p <= 0.0 do
                    lower.RemoveAt(lower.Count - 1)
                lower.Add(p)

            // Monotone Chain: Upper Hull
            let upper = ResizeArray()
            for i in (sortedPoints.Length - 1) .. -1 .. 0 do
                let p = sortedPoints.[i]
                while upper.Count >= 2 && crossProduct upper.[upper.Count-2] upper.[upper.Count-1] p <= 0.0 do
                    upper.RemoveAt(upper.Count - 1)
                upper.Add(p)

            let hullBins =
                seq {
                    yield! lower
                    yield! upper
                }
                |> Seq.distinctBy (fun p -> p.Key)
                |> Seq.map (fun p -> p.Key, p.Bin)
                |> Map.ofSeq

            { source with 
                sorterEvalBinsId = newId
                bins = hullBins }


    /// Generates table-ready records for all scores across all bins.
    let makeDataTableRecords (source: sorterEvalBinsV1) : GeneSort.Core.dataTableRecord seq =
        source.Bins
        |> Map.toSeq
        |> Seq.collect (fun (key, bin) ->
            let keyRecord = SorterEvalKey.toDataTableRecord key
            bin.Scores |> Seq.map (fun score ->
                let scoreRecord = SorterScore.toDataTableRecord score
                (scoreRecord, keyRecord.Data) 
                ||> Map.fold (fun acc k v -> GeneSort.Core.dataTableRecord.addData k v acc)
            )
        )



type sorterEvalBins =
    | V1 of sorterEvalBinsV1
    | Unknown
