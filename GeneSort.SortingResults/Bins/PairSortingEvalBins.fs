namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.SortingOps
open System
open GeneSort.Model.Sorting
open GeneSort.SortingResults
open GeneSort.Model.Sorting.SorterPair


type pairSortingEvalBins =
     | SplitPairs of splitPairSortingEvalBins
     | SplitPairs2 of splitPairSortingEvalBins


module PairSortingEvalBins = 

    let addSorterEval 
                (pairSortingEvalBins:pairSortingEvalBins) 
                (sorterEval: sorterEval) 
                (modelTag:modelTag) : unit =
        match pairSortingEvalBins with
        | SplitPairs sp -> sp.AddSorterEval sorterEval modelTag
        | SplitPairs2 sp2 -> sp2.AddSorterEval sorterEval modelTag


    let getId (pairSortingEvalBins:pairSortingEvalBins) : Guid<sortingEvalBinsId> = 
        match pairSortingEvalBins with
        | SplitPairs sp -> sp.SortingEvalBinsId
        | SplitPairs2 sp2 -> sp2.SortingEvalBinsId


    let create (model: sorterPairModel) : pairSortingEvalBins =
        match model with
        | sorterPairModel.SplitPairs _ -> 
            SplitPairs (
                    splitPairSortingEvalBins.create (Guid.NewGuid () |> UMX.tag<sortingEvalBinsId>))
        | sorterPairModel.SplitPairs2 _ -> 
            SplitPairs2 (
                    splitPairSortingEvalBins.create (Guid.NewGuid () |> UMX.tag<sortingEvalBinsId>))
