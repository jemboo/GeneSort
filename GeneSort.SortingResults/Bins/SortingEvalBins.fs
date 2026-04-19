namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.SortingOps
open System
open GeneSort.Model.Sorting
open GeneSort.SortingResults
open GeneSort.Core


type sortingEvalBins =
     | Single of singleSortingEvalBins
     | Pairs of pairSortingEvalBins

module SortingEvalBins = 

    let addSorterEval 
                (sortingEvalBins:sortingEvalBins) 
                (sorterEval: sorterEval) 
                (modelTag:modelTag) =
        match sortingEvalBins with
        | Single s -> s.AddSorterEval sorterEval modelTag
        | Pairs p -> PairSortingEvalBins.addSorterEval p sorterEval modelTag

    let getId (sortingEvalBins:sortingEvalBins) : Guid<sortingEvalBinsId> = 
        match sortingEvalBins with
        | Single s -> s.SortingEvalBinsId
        | Pairs p -> PairSortingEvalBins.getId p    

    let makeFromSorting (ting: sorting) : sortingEvalBins =
        match ting with
        | sorting.Single _ -> singleSortingEvalBins.create (Guid.NewGuid() |> UMX.tag<sortingEvalBinsId>)
                              |> Single
        | sorting.Pairs spm -> PairSortingEvalBins.create spm |> Pairs

    let getAllTaggedSorterEvalBins (sortingEvalBins:sortingEvalBins) : (sorterEvalBins * modelTag) seq =
        match sortingEvalBins with
        | Single s -> seq { yield (s.SorterEvalBins, modelTag.Single) }
        | Pairs p -> PairSortingEvalBins.getAllTaggedSorterEvalBins p

    let makeDataTableRecords (bins: sortingEvalBins) : dataTableRecord seq =
        match bins with
        | Single s -> SingleSortingEvalBins.makeDataTableRecords s
        | Pairs p -> PairSortingEvalBins.makeDataTableRecords p




