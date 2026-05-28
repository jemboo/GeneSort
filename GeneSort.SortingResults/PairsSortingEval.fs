namespace GeneSort.SortingResults

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


type pairsSortingEval=
     | SplitPairs of splitPairsSortingEval
     | SplitPairs2 of splitPairsSortingEval


module PairsSortingEval = 

    let getSortingId (psr: pairsSortingEval) : Guid<sortingId> =
        match psr with
        | SplitPairs spsr -> spsr.SortingId
        | SplitPairs2 spsr -> spsr.SortingId


    let getSorterEval (modelTag:modelTag) (psr: pairsSortingEval) : sorterEvalOld =
        match psr with
        | SplitPairs spsr -> spsr.GetSorterEval modelTag
        | SplitPairs2 spsr -> spsr.GetSorterEval modelTag

    let getAllTaggedSorterEvals (psr: pairsSortingEval) : (sorterEvalOld * modelSetTag) seq =
        match psr with
        | SplitPairs spsr -> spsr.GetAllSorterEvals ()
        | SplitPairs2 spsr -> spsr.GetAllSorterEvals ()



