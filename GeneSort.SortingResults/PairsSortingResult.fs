namespace GeneSort.SortingResults

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


type pairsSortingResult=
     | SplitPairs of splitPairsSortingResult
     | SplitPairs2 of splitPairsSortingResult


module PairsSortingResult = 

    let getSortingId (psr: pairsSortingResult): Guid<sortingId> =
        match psr with
        | SplitPairs spsr -> spsr.SortingId
        | SplitPairs2 spsr -> spsr.SortingId


