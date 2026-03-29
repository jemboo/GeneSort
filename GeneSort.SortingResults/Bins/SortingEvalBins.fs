namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System
open GeneSort.Model.Sorting
open GeneSort.SortingResults


type sortingEvalBins =
     | Single of singleSortingEvalBins
     | Pairs of pairSortingEvalBins

    //static member create (sortingId: Guid<sortingId>) (tags: modelTag seq) =
    //    {
    //        sortingId = sortingId
    //        sorterEvalBinsMap =
    //            let d = Dictionary<modelTag, sorterEvalBins>()
    //            for tag in tags do d.[tag] <- sorterEvalBins.createWithNewId Seq.empty
    //            d
    //    }

    //member this.SortingId         with get() = this.sortingId
    //member this.SorterEvalBinsMap with get() = this.sorterEvalBinsMap :> IReadOnlyDictionary<modelTag, sorterEvalBins>

    //member this.TryGetBins (tag: modelTag) : sorterEvalBins option =
    //    match this.sorterEvalBinsMap.TryGetValue(tag) with
    //    | true, bins -> Some bins
    //    | false, _   -> None

    //member this.AddSorterEvals (tag: modelTag) (sorterEvals: sorterEval seq) =
    //    match this.sorterEvalBinsMap.TryGetValue(tag) with
    //    | true, bins -> bins.AddSorterEvals sorterEvals
    //    | false, _   -> this.sorterEvalBinsMap.[tag] <- sorterEvalBins.createWithNewId sorterEvals

    //member this.AddTaggedSorterEvals (taggedEvals: (modelTag * sorterEval) seq) =
    //    for (tag, eval) in taggedEvals do
    //        match this.sorterEvalBinsMap.TryGetValue(tag) with
    //        | true, bins -> bins.AddSorterEval eval
    //        | false, _   -> this.sorterEvalBinsMap.[tag] <- sorterEvalBins.createWithNewId (Seq.singleton eval)


    //// on sortingEvalBins:
    //member this.AddTaggedSorterEval (tag: modelTag) (eval: sorterEval) =
    //    match this.sorterEvalBinsMap.TryGetValue(tag) with
    //    | true, bins -> bins.AddSorterEval eval
    //    | false, _   -> this.sorterEvalBinsMap.[tag] <- sorterEvalBins.createWithNewId (Seq.singleton eval)


    //member this.MergeBins (tag: modelTag) (source: sorterEvalBins) =
    //    match this.sorterEvalBinsMap.TryGetValue(tag) with
    //    | true, existing -> SorterEvalBins.merge existing source |> ignore
    //    | false, _       -> this.sorterEvalBinsMap.[tag] <- source

    //// Total across all tags — mirrors sorterEvalBins.EvalCount
    //member this.EvalCount with get() =
    //    this.sorterEvalBinsMap.Values |> Seq.sumBy (fun b -> b.EvalCount)

    //// Useful for diagnostics — are all expected tags populated?
    //member this.HasTag (tag: modelTag) =
    //    this.sorterEvalBinsMap.ContainsKey(tag)

    //member this.Tags with get() =
    //    this.sorterEvalBinsMap.Keys |> Seq.toArray



module SortingEvalBins = 

    let addSorterEval (sortingEvalBins:sortingEvalBins) (sorterEval: sorterEval) (modelTag:modelTag) =
        match sortingEvalBins with
        | Single s -> s.AddSorterEval sorterEval modelTag
        | Pairs p -> PairSortingEvalBins.addSorterEval p sorterEval modelTag

    let getId (sortingEvalBins:sortingEvalBins) : Guid<sortingEvalBinsId> = 
        match sortingEvalBins with
        | Single s -> s.SortingEvalBinsId
        | Pairs p -> PairSortingEvalBins.getId p    