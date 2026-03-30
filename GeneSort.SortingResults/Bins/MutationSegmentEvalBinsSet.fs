namespace GeneSort.SortingResults.Bins

open System.Collections.Generic
open FSharp.UMX

open GeneSort.Model.Sorting
open GeneSort.SortingOps


type mutationSegmentEvalBinsSet =
    private {
        binDict: Dictionary<Guid<sortingId>, mutationSegmentEvalBins>
    }

    member this.AddMutantSorterEval (sorterEval: sorterEval) (modelSetTag:modelSetTag) =
        let (sortingId, modelTag) = modelSetTag
        match this.binDict.TryGetValue(sortingId) with
        | true, bins -> bins.AddMutantSorterEval sorterEval modelTag
        | false, _ -> failwithf "SortingId %A not found in mutationSegmentEvalBinsSet." sortingId

    member this.AddParentSorterEval (sorterEval: sorterEval) (modelSetTag:modelSetTag) =
        let (sortingId, modelTag) = modelSetTag
        match this.binDict.TryGetValue(sortingId) with
        | true, bins -> bins.AddParentSorterEval sorterEval modelTag
        | false, _ -> failwithf "SortingId %A not found in mutationSegmentEvalBinsSet." sortingId

    member this.GetBins (sortingId: Guid<sortingId>) =
        match this.binDict.TryGetValue(sortingId) with
        | true, bins -> bins
        | false, _ -> failwithf "SortingId %A not found in mutationSegmentEvalBinsSet." sortingId


module MutationSegmentEvalBinsSet =

    let makeFromSortings (tings: sorting seq) : mutationSegmentEvalBinsSet =
        let binDict = Dictionary<Guid<sortingId>, mutationSegmentEvalBins>()
        for ting in tings do
            let bins = MutationSegmentEvalBins.makeFromSorting ting
            binDict.Add(Sorting.getId ting, bins)
        { binDict = binDict }