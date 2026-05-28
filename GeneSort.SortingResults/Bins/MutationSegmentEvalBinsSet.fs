namespace GeneSort.SortingResults.Bins

open System.Collections.Generic
open FSharp.UMX

open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.SortingResults


type mutationSegmentEvalBinsSet =
    private {
        id: Guid<mutationSegmentEvalBinsSetId>
        binDict: Dictionary<Guid<sortingId>, mutationSegmentEvalBins>
    }

    member this.Id = this.id

    member this.GetBinDict() = this.binDict :> seq<KeyValuePair<Guid<sortingId>, mutationSegmentEvalBins>>

    member this.AddMutantSorterEval (sorterEval: sorterEvalOld) (modelSuperSetTag:modelSuperSetTag) : unit =
        match this.binDict.TryGetValue(modelSuperSetTag.SortingId) with
        | true, bins -> bins.AddMutantSorterEval sorterEval modelSuperSetTag.ModelTag |> ignore
        | false, _ -> failwithf "SortingId %A not found in mutationSegmentEvalBinsSet." modelSuperSetTag.SortingId

    member this.AddAllMutantSorterEvals (tupes: (sorterEvalOld * modelSuperSetTag) seq) =
        for (sorterEval, modelSuperSetTag) in tupes do
            this.AddMutantSorterEval sorterEval modelSuperSetTag

    member this.AddParentSorterEval (sorterEval: sorterEvalOld) (modelSetTag:modelSetTag) =
        match this.binDict.TryGetValue(modelSetTag.SortingId) with
        | true, bins -> bins.AddParentSorterEval sorterEval modelSetTag.ModelTag
        | false, _ -> failwithf "SortingId %A not found in mutationSegmentEvalBinsSet." modelSetTag.SortingId

    member this.AddAllParentSorterEvals (tupes: (sorterEvalOld * modelSetTag) seq) =
        for (sorterEval, modelSetTag) in tupes do
            this.AddParentSorterEval sorterEval modelSetTag

    member this.GetBins (sortingId: Guid<sortingId>) =
        match this.binDict.TryGetValue(sortingId) with
        | true, bins -> bins
        | false, _ -> failwithf "SortingId %A not found in mutationSegmentEvalBinsSet." sortingId


module MutationSegmentEvalBinsSet =

    let makeFromStorage 
                (id: Guid<mutationSegmentEvalBinsSetId>)
                (parentSortingResult: (Guid<sortingId> * mutationSegmentEvalBins) seq) 
                : mutationSegmentEvalBinsSet =
        let binDict = Dictionary<Guid<sortingId>, mutationSegmentEvalBins>()
        for (sortingId, bins) in parentSortingResult do
            binDict.Add(sortingId, bins)
        {   id = id
            binDict = binDict }


    let makeFromSortings 
                (id: Guid<mutationSegmentEvalBinsSetId>)
                (tings: sorting seq) : mutationSegmentEvalBinsSet =
        let binDict = Dictionary<Guid<sortingId>, mutationSegmentEvalBins>()
        for ting in tings do
            let bins = MutationSegmentEvalBins.makeFromSorting ting
            binDict.Add(Sorting.getId ting, bins)
        {   id = id
            binDict = binDict }