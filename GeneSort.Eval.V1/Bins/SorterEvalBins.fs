
namespace GeneSort.Eval.V1.Bins

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.SortingOps

[<Measure>] type sorterEvalBinsId

type sorterEvalBins =
    private {
        sorterEvalBinsId: Guid<sorterEvalBinsId>
        layers: Map<sorterEvalKey, sorterEvalBin>
        sortableTestId: Guid<sortableTestId>
    }
    with
    static member create (id: Guid<sorterEvalBinsId>) (testId: Guid<sortableTestId>) =
        { 
            sorterEvalBinsId = id
            layers = Map.empty 
            sortableTestId = testId
        }

    static member createWithNewId (testId: Guid<sortableTestId>) =
        let id = Guid.NewGuid() |> UMX.tag<sorterEvalBinsId>
        sorterEvalBins.create id testId

    member this.Id = this.sorterEvalBinsId
    member this.Layers = this.layers
    member this.SortableTestId = this.sortableTestId

    member this.EvalCount =
        this.layers |> Map.toSeq |> Seq.sumBy (fun (_, bin) -> bin.EvalCount)

    /// Adds a single sorter evaluation to the appropriate bin
    member this.AddSorterEval (eval: sorterEval) =
        let key = SorterEvalKey.fromSorterEval eval
        let score = SorterScore.fromSorterEval eval |> sorterScore.V1
        
        match Map.tryFind key this.layers with
        | Some existingBin -> 
            existingBin.AddScore(score)
            this // Map doesn't change because ResizeArray is mutated internally
        | None -> 
            let newBin = sorterEvalBin.create score key
            { this with layers = Map.add key newBin this.layers }

    /// Merges another bin into this one
    member this.MergeBin (key: sorterEvalKey) (sourceBin: sorterEvalBin) : sorterEvalBins =
        match Map.tryFind key this.layers with
        | Some existingBin -> 
            for score in sourceBin.Scores do existingBin.AddScore(score)
            this
        | None -> 
            { this with layers = Map.add key sourceBin this.layers }



module SorterEvalBins =
    
    let merge (target: sorterEvalBins) (source: sorterEvalBins) : sorterEvalBins =
        (target, source.Layers |> Map.toSeq) 
        ||> Seq.fold (fun acc (key, bin) -> acc.MergeBin key bin)

    /// Extracts all scores across all bins
    let getAllScores (bins: sorterEvalBins) : sorterScore seq =
        bins.Layers 
        |> Map.toSeq 
        |> Seq.collect (fun (_, bin) -> bin.Scores)
