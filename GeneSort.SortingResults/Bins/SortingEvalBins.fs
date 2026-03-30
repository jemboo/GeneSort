namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.SortingOps
open System
open GeneSort.Model.Sorting
open GeneSort.SortingResults


type sortingEvalBins =
     | Single of singleSortingEvalBins
     | Pairs of pairSortingEvalBins

module SortingEvalBins = 

    let addSorterEval (sortingEvalBins:sortingEvalBins) (sorterEval: sorterEval) (modelTag:modelTag) =
        match sortingEvalBins with
        | Single s -> s.AddSorterEval sorterEval modelTag
        | Pairs p -> PairSortingEvalBins.addSorterEval p sorterEval modelTag

    let getId (sortingEvalBins:sortingEvalBins) : Guid<sortingEvalBinsId> = 
        match sortingEvalBins with
        | Single s -> s.SortingEvalBinsId
        | Pairs p -> PairSortingEvalBins.getId p    

    let makeFromSorting (ting: sorting) : sortingEvalBins =
        match ting with
        | sorting.Single _ -> Single (singleSortingEvalBins.create (Guid.NewGuid() |> UMX.tag<sortingEvalBinsId>))
        | sorting.Pairs spm -> Pairs (PairSortingEvalBins.create spm)