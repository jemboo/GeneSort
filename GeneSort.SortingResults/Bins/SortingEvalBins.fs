namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System
open GeneSort.Model.Sorting


type sortingEvalBins =
    private {
        sortingEvalKeys:   sortingEvalKeys
        sorterEvalBinsMap: Dictionary<modelTag, sorterEvalBins>
    }

    static member create (sortingId: Guid<sortingId>) (tags: modelTag seq) =
        {
            sortingEvalKeys   = sortingEvalKeys.create sortingId
            sorterEvalBinsMap =
                let d = Dictionary<modelTag, sorterEvalBins>()
                for tag in tags do d.[tag] <- sorterEvalBins.createWithNewId Seq.empty
                d
        }

    member this.SortingId         with get() = this.sortingEvalKeys.SortingId
    member this.SortingEvalKeys   with get() = this.sortingEvalKeys
    member this.SorterEvalBinsMap with get() = this.sorterEvalBinsMap :> IReadOnlyDictionary<modelTag, sorterEvalBins>

    member this.TryGetBins (tag: modelTag) : sorterEvalBins option =
        match this.sorterEvalBinsMap.TryGetValue(tag) with
        | true, bins -> Some bins
        | false, _   -> None

    member this.AddSorterEvals (tag: modelTag) (sorterEvals: sorterEval seq) =
        match this.sorterEvalBinsMap.TryGetValue(tag) with
        | true, bins -> bins.AddSorterEvals sorterEvals
        | false, _   -> this.sorterEvalBinsMap.[tag] <- sorterEvalBins.createWithNewId sorterEvals

    member this.AddTaggedSorterEvals (taggedEvals: (modelTag * sorterEval) seq) =
        for (tag, eval) in taggedEvals do
            match this.sorterEvalBinsMap.TryGetValue(tag) with
            | true, bins -> bins.AddSorterEval eval
            | false, _   -> this.sorterEvalBinsMap.[tag] <- sorterEvalBins.createWithNewId (Seq.singleton eval)

    member this.MergeBins (tag: modelTag) (source: sorterEvalBins) =
        match this.sorterEvalBinsMap.TryGetValue(tag) with
        | true, existing -> SorterEvalBins.merge existing source |> ignore
        | false, _       -> this.sorterEvalBinsMap.[tag] <- source

    // Total across all tags — mirrors sorterEvalBins.EvalCount
    member this.EvalCount with get() =
        this.sorterEvalBinsMap.Values |> Seq.sumBy (fun b -> b.EvalCount)

    // Useful for diagnostics — are all expected tags populated?
    member this.HasTag (tag: modelTag) =
        this.sorterEvalBinsMap.ContainsKey(tag)

    member this.Tags with get() =
        this.sorterEvalBinsMap.Keys |> Seq.toArray



module SortingEvalBins =

    let createFromSorting (sorting: sorting) : sortingEvalBins =
        let id = sorting |> Sorting.getId
        let modelTags = sorting |> Sorting.getModelTags
        sortingEvalBins.create id modelTags

    let merge (target: sortingEvalBins) (source: sortingEvalBins) : sortingEvalBins =
        for kvp in source.SorterEvalBinsMap do
            target.MergeBins kvp.Key kvp.Value
        target

    let getUpToNSorterIdsPerBin
            (tag:                modelTag)
            (orderFunc:          sorterEvalKey -> float)
            (successfullySorted: bool)
            (maxPerBin:          int)
            (source:             sortingEvalBins)
            : (sorterEvalKey * Guid<sorterId>) seq =
        match source.TryGetBins tag with
        | None      -> Seq.empty
        | Some bins -> SorterEvalBins.getUpToNSorterIdsPerBin orderFunc successfullySorted maxPerBin bins

    let getUpToNSorterIdsPerConvexHullBin
            (tag:                modelTag)
            (orderFunc:          sorterEvalKey -> float)
            (successfullySorted: bool)
            (maxPerBin:          int)
            (source:             sortingEvalBins)
            : (sorterEvalKey * Guid<sorterId>) seq =
        match source.TryGetBins tag with
        | None      -> Seq.empty
        | Some bins -> SorterEvalBins.getUpToNSorterIdsPerConvexHullBin orderFunc successfullySorted maxPerBin bins