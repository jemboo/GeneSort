namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting


type sortingEval =
     | Single of singleSortingEval
     | Pairs of pairsSortingEval


module SortingEval =

    let getSortingId (sortingResult: sortingEval) : Guid<sortingId> =
        match sortingResult with
        | Single singleResult -> singleResult.SortingId
        | Pairs pairsSortingEval -> pairsSortingEval |> PairsSortingEval.getSortingId


    let addSorterEval (modelTag: modelTag) 
                      (newEval: sorterEval) 
                      (sortingEval: sortingEval) : unit =
        match sortingEval with
        | Single ssr -> ssr.AddSorterEval modelTag newEval
        | Pairs psr ->
            match psr with
            | SplitPairs spsr -> spsr.AddSorterEval modelTag newEval
            | SplitPairs2 spsr -> spsr.AddSorterEval modelTag newEval


    let makeFromSorting (ting: sorting) : sortingEval =
        match ting with
        | sorting.Single ssm -> singleSortingEval.create 
                                        (ting |> Sorting.getId) |> sortingEval.Single
        | sorting.Pairs spm -> splitPairsSortingEval.create 
                                        (ting |> Sorting.getId) 
                                        |> pairsSortingEval.SplitPairs |> sortingEval.Pairs


    let getSorterEval (psr: sortingEval) (modelTag:modelTag) : sorterEval =
        match psr with
        | Single ssr -> ssr.SorterEval.Value
        | Pairs psr -> psr |> PairsSortingEval.getSorterEval modelTag

            
    let getAllTaggedSorterEvals (psr: sortingEval) : (sorterEval * modelSetTag) seq =
        match psr with
        | Single ssr -> ssr.GetAllTaggedSorterEvals ()
        | Pairs psr -> psr |> PairsSortingEval.getAllTaggedSorterEvals




    /// For the sortingSet and its corresponding sorterSetEval, this creates a sortingSet 
    /// that consists of all the sortings that produced sorters with an UnsortedCount = 0
    let makePassingSortingSet
            (id: Guid<sortingSetId>)
            (sms: sortingSet)
            (sorterSetEval: sorterSetEval) : sortingSet =
        
        // 1. Identify the IDs of the sorters that passed (UnsortedCount = 0)
        let passingIds = 
            sorterSetEval.SorterEvals 
            |> Array.filter (fun se -> se.CeBlockEval.UnsortedCount = 0<sortableCount>)
            |> Array.map (fun se -> se.SorterId)

        // 2. Filter the original sorter collection based on the passing IDs
        let passingSorterModels = 
            sms.Sortings 
            |> Array.filter (fun stm -> Sorting.containsAnySorter passingIds stm)

        sortingSet.create 
            id
            passingSorterModels
