namespace GeneSort.Eval.V1.Bins.Old

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.SortingOps
open SorterScoreOld

[<Measure>] type sorterEvalBinsId

type sorterEvalBinsV1Old =
    private {
        sorterEvalBinsId: Guid<sorterEvalBinsId>
        bins: Map<sorterEvalKeyOld, sorterEvalBinV1Old>
        sortableTestId: Guid<sortableTestId>
    }
    with
    static member createEmpty (id: Guid<sorterEvalBinsId>) (testId: Guid<sortableTestId>) =
        { 
            sorterEvalBinsId = id
            bins = Map.empty 
            sortableTestId = testId
        }

    static member createFromEvals 
            (id: Guid<sorterEvalBinsId>) 
            (testId: Guid<sortableTestId>)
            (evals: sorterEvalOld seq) =
    
        // Use a local mutable dictionary for O(1) lookups without thread contention
        let dict = System.Collections.Generic.Dictionary<sorterEvalKeyOld, ResizeArray<sorterScoreOld>>()
    
        for eval in evals do
            let key = SorterEvalKeyOld.fromSorterEval eval
            let score = SorterScoreOld.fromSorterEval eval |> sorterScoreOld.V1
        
            match dict.TryGetValue(key) with
            | true, scores -> scores.Add(score)
            | false, _ -> 
                let scores = ResizeArray<sorterScoreOld>()
                scores.Add(score)
                dict.[key] <- scores

        // Convert to the immutable Map once at the end
        let binMap = 
            dict 
            |> Seq.map (fun kvp -> 
                kvp.Key, sorterEvalBinV1Old.createWithScores kvp.Value kvp.Key)
            |> Map.ofSeq

        { 
            sorterEvalBinsId = id
            bins = binMap
            sortableTestId = testId
        }


    static member createFromMap 
            (id: Guid<sorterEvalBinsId>) 
            (testId: Guid<sortableTestId>)
            (bins: Map<sorterEvalKeyOld, sorterEvalBinV1Old>) =
        { 
            sorterEvalBinsId = id
            bins = bins
            sortableTestId = testId
        }

    static member createWithNewId (testId: Guid<sortableTestId>) =
        let id = Guid.NewGuid() |> UMX.tag<sorterEvalBinsId>
        sorterEvalBinsV1Old.createEmpty id testId

    member this.Id = this.sorterEvalBinsId
    member this.Bins = this.bins
    member this.SortableTestId = this.sortableTestId

    member this.EvalCount =
        this.bins |> Map.toSeq |> Seq.sumBy (fun (_, bin) -> bin.EvalCount)

    /// Adds a single sorter evaluation to the appropriate bin
    member this.AddSorterEval (eval: sorterEvalOld) =
        let key = SorterEvalKeyOld.fromSorterEval eval
        let score = SorterScoreOld.fromSorterEval eval |> sorterScoreOld.V1
        
        match Map.tryFind key this.bins with
        | Some existingBin -> 
            existingBin.AddScore(score)
            this // Map doesn't change because ResizeArray is mutated internally
        | None -> 
            let newBin = sorterEvalBinV1Old.create score key
            { this with bins = Map.add key newBin this.bins }

    /// Merges another bin into this one
    member this.MergeBin (key: sorterEvalKeyOld) (sourceBin: sorterEvalBinV1Old) : sorterEvalBinsV1Old =
        match Map.tryFind key this.bins with
        | Some existingBin -> 
            for score in sourceBin.Scores do existingBin.AddScore(score)
            this
        | None -> 
            { this with bins = Map.add key sourceBin this.bins }

    member this.toSorterScoresWithKeys() : sorterScoreOld seq =
        this.bins 
        |> Map.toSeq 
        |> Seq.collect (fun (_, bin) -> bin.toSorterScoresWithKeys())


module SorterEvalBinsV1Old =
    
    let merge (target: sorterEvalBinsV1Old) (source: sorterEvalBinsV1Old) : sorterEvalBinsV1Old =
        (target, source.Bins |> Map.toSeq) 
        ||> Seq.fold (fun acc (key, bin) -> acc.MergeBin key bin)

    /// Extracts all scores across all bins
    let getAllScores (bins: sorterEvalBinsV1Old) : sorterScoreOld seq =
        bins.Bins 
        |> Map.toSeq 
        |> Seq.collect (fun (_, bin) -> bin.Scores)

    /// Creates a new sorterEvalBins containing only the scores that satisfy the filter.
    /// Bins that become empty after filtering are omitted from the result.
    let extractBins (scoreFilter: sorterScoreOld -> bool) 
                    (source: sorterEvalBinsV1Old) : sorterEvalBinsV1Old =

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
                    let newBin = sorterEvalBinV1Old.createWithScores matchedScores key
                    Some (key, newBin)
                else
                    None)
            |> Map.ofSeq

        {source with bins = filteredLayers; sorterEvalBinsId = filteredId}


/// Returns a new sorterEvalBins containing only the top N scores globally.
    let getTopN 
            (scoreValuer: sorterScoreOld -> float) 
            (newId : Guid<sorterEvalBinsId>) 
            (source: sorterEvalBinsV1Old)
            (n : int) : sorterEvalBinsV1Old =
        
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
                key, sorterEvalBinV1Old.createWithScores scores key)
            |> Map.ofSeq

        { source with 
            sorterEvalBinsId = newId
            bins = regroupedBins }

    /// Filters the bins down to the Pareto Frontier (Convex Hull) in (Gates * Depth) space.
    let convexHull
            (newId : Guid<sorterEvalBinsId>) 
            (source: sorterEvalBinsV1Old) : sorterEvalBinsV1Old =
        
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
                (o: {| X: float; Y: float; Key: sorterEvalKeyOld; Bin: sorterEvalBinV1Old |}) 
                (a: {| X: float; Y: float; Key: sorterEvalKeyOld; Bin: sorterEvalBinV1Old |}) 
                (b: {| X: float; Y: float; Key: sorterEvalKeyOld; Bin: sorterEvalBinV1Old |}) =
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
    let makeFullDataTableRecords (source: sorterEvalBinsV1Old) : GeneSort.Core.dataTableRecord seq =
        source.Bins
        |> Map.toSeq
        |> Seq.collect (fun (key, bin) ->
            let keyRecord = SorterEvalKeyOld.toDataTableRecord key
            bin.Scores |> Seq.map (fun score ->
                let scoreRecord = SorterScoreOld.toDataTableRecord score
                (scoreRecord, keyRecord.Data) 
                ||> Map.fold (fun acc k v -> GeneSort.Core.dataTableRecord.addKeyAndData k v acc)
            )
        )

    /// Generates two dataTableRecords per bin, one that summarizes the sorted members of the bin, 
    /// and one for the unsorted.
    let makeSummaryDataTableRecords (source: sorterEvalBinsV1Old) : GeneSort.Core.dataTableRecord seq =
        source.Bins
        |> Map.toSeq
        |> Seq.collect (fun (key, bin) ->
            let keyRecord = SorterEvalKeyOld.toDataTableRecord key
            
            // Partition scores based on whether they are fully sorted
            let unsortedScores, sortedScores = 
                bin.Scores 
                |> Seq.toArray
                |> Array.partition (SorterScoreOld.isUnsorted >> Option.defaultValue true)

            let createSummary (scores: sorterScoreOld array) (status: string) =
                if scores.Length = 0 then 
                    None
                else
                    // Find the number of unique stage sequences in this bin, which serves as a proxy for solution diversity
                    let uniqueHashes = scores |> Seq.map (SorterScoreOld.sequenceHash >> Option.defaultValue 0<sequenceHash>) 
                                              |> Seq.distinct |> Seq.length
                    
                    let summary = 
                        GeneSort.Core.dataTableRecord.createEmpty()
                        |> GeneSort.Core.dataTableRecord.addData "Sorted" status
                        |> GeneSort.Core.dataTableRecord.addData "BinCount" (string scores.Length)
                        |> GeneSort.Core.dataTableRecord.addData "UniqueHashes" (string %uniqueHashes)
                    
                    // Merge with the bin's key data (Gates, Depth, etc.)
                    Some ((summary, keyRecord.Data) 
                        ||> Map.fold (fun acc k v -> GeneSort.Core.dataTableRecord.addKeyAndData k v acc))

            seq {
                match createSummary sortedScores "True" with | Some r -> yield r | None -> ()
                match createSummary unsortedScores "False" with | Some r -> yield r | None -> ()
            }
        )




type sorterEvalBinsOld =
    | V1 of sorterEvalBinsV1Old
    | Unknown


module SorterEvalBinsOld =
    let createEmpty = sorterEvalBinsV1Old.createEmpty
    let createFromEvals = sorterEvalBinsV1Old.createFromEvals
    let createFromMap = sorterEvalBinsV1Old.createFromMap
    let createWithNewId = sorterEvalBinsV1Old.createWithNewId
    let addSorterEval = (fun bins eval -> match bins with | V1 b -> V1 (b.AddSorterEval eval) | _ -> bins)
    let merge = (fun target source -> match target, source with | V1 t, V1 s -> V1 (SorterEvalBinsV1Old.merge t s) | _ -> target)
    let getAllScores = (fun bins -> match bins with | V1 b -> SorterEvalBinsV1Old.getAllScores b | _ -> Seq.empty)
    let extractBins = (fun filter bins -> match bins with | V1 b -> V1 (SorterEvalBinsV1Old.extractBins filter b) | _ -> bins)
    let getTopN = (fun scoreValuer newId bins n -> match bins with | V1 b -> V1 (SorterEvalBinsV1Old.getTopN scoreValuer newId b n) | _ -> bins)
    let convexHull = (fun newId bins -> match bins with | V1 b -> V1 (SorterEvalBinsV1Old.convexHull newId b) | _ -> bins)
    let makeFullDataTableRecords = (fun bins -> match bins with | V1 b -> SorterEvalBinsV1Old.makeFullDataTableRecords b | _ -> Seq.empty)
    let makeSummaryDataTableRecords = (fun bins -> match bins with | V1 b -> SorterEvalBinsV1Old.makeSummaryDataTableRecords b | _ -> Seq.empty)