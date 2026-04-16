namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


type sortingResult =
     | Single of singleSortingResult
     | Pairs of pairsSortingResult


module SortingResult =

    let getSortingId (sortingResult: sortingResult) : Guid<sortingId> =
        match sortingResult with
        | Single singleResult -> singleResult.SortingId
        | Pairs pairsSortingResult -> pairsSortingResult |> PairsSortingResult.getSortingId


    let addSorterEval (modelTag: modelTag) 
                      (newEval: sorterEval) 
                      (sortingResult: sortingResult) : unit =
        match sortingResult with
        | Single ssr -> ssr.AddSorterEval modelTag newEval
        | Pairs psr ->
            match psr with
            | SplitPairs spsr -> spsr.AddSorterEval modelTag newEval
            | SplitPairs2 spsr -> spsr.AddSorterEval modelTag newEval


    let makeFromSorting (ting: sorting) : sortingResult =
        match ting with
        | sorting.Single ssm -> singleSortingResult.create 
                                        (ting |> Sorting.getId) |> sortingResult.Single
        | sorting.Pairs spm -> splitPairsSortingResult.create 
                                        (ting |> Sorting.getId) 
                                        |> pairsSortingResult.SplitPairs |> sortingResult.Pairs


    let getSorterEval (psr: sortingResult) (modelTag:modelTag) : sorterEval =
        match psr with
        | Single ssr -> ssr.SorterEval.Value
        | Pairs psr -> psr |> PairsSortingResult.getSorterEval modelTag

            
    let getAllTaggedSorterEvals (psr: sortingResult) : (sorterEval * modelSetTag) seq =
        match psr with
        | Single ssr -> ssr.GetAllTaggedSorterEvals ()
        | Pairs psr -> psr |> PairsSortingResult.getAllTaggedSorterEvals