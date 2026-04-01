namespace GeneSort.SortingResults.Bins

open System.Collections.Generic
open FSharp.UMX

open GeneSort.Model.Sorting
open GeneSort.SortingOps


type mutationSegmentEvalBinsSet =
    private {
        binDict: Dictionary<Guid<sortingId>, mutationSegmentEvalBins>
    }

    member this.GetBinDict() = this.binDict :> seq<KeyValuePair<Guid<sortingId>, mutationSegmentEvalBins>>

    member this.AddMutantSorterEval (sorterEval: sorterEval) (modelSuperSetTag:modelSuperSetTag) =
        match this.binDict.TryGetValue(modelSuperSetTag.SortingId) with
        | true, bins -> bins.AddMutantSorterEval sorterEval modelSuperSetTag.ModelTag
        | false, _ -> failwithf "SortingId %A not found in mutationSegmentEvalBinsSet." modelSuperSetTag.SortingId

    member this.AddAllMutantSorterEvals (tupes: (sorterEval * modelSuperSetTag) seq) =
        for (sorterEval, modelSuperSetTag) in tupes do
            this.AddMutantSorterEval sorterEval modelSuperSetTag

    member this.AddParentSorterEval (sorterEval: sorterEval) (modelSetTag:modelSetTag) =
        match this.binDict.TryGetValue(modelSetTag.SortingId) with
        | true, bins -> bins.AddParentSorterEval sorterEval modelSetTag.ModelTag
        | false, _ -> failwithf "SortingId %A not found in mutationSegmentEvalBinsSet." modelSetTag.SortingId

    member this.AddAllParentSorterEvals (tupes: (sorterEval * modelSetTag) seq) =
        for (sorterEval, modelSetTag) in tupes do
            this.AddParentSorterEval sorterEval modelSetTag

    member this.GetBins (sortingId: Guid<sortingId>) =
        match this.binDict.TryGetValue(sortingId) with
        | true, bins -> bins
        | false, _ -> failwithf "SortingId %A not found in mutationSegmentEvalBinsSet." sortingId


module MutationSegmentEvalBinsSet =

    let makeFromStorage 
                (parentSortingResult: (Guid<sortingId> * mutationSegmentEvalBins) seq) 
                : mutationSegmentEvalBinsSet =
        let binDict = Dictionary<Guid<sortingId>, mutationSegmentEvalBins>()
        for (sortingId, bins) in parentSortingResult do
            binDict.Add(sortingId, bins)
        { binDict = binDict }


    let makeFromSortings (tings: sorting seq) : mutationSegmentEvalBinsSet =
        let binDict = Dictionary<Guid<sortingId>, mutationSegmentEvalBins>()
        for ting in tings do
            let bins = MutationSegmentEvalBins.makeFromSorting ting
            binDict.Add(Sorting.getId ting, bins)
        { binDict = binDict }