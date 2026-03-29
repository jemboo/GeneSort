namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System
open GeneSort.Model.Sorting
open GeneSort.SortingResults


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