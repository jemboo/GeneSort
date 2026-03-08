namespace GeneSort.Model.Sorting

open System
open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair


type sortingMutator =
     | Single of sorterModelMutator
     | Pair of sorterPairModelMutator


module SortingMutator =

    let getId (model:sortingMutator) : Guid<sortingMutatorId> =
        match model with
        | Single smm -> %(smm |> SorterModelMutator.getId) |> UMX.tag<sortingMutatorId>
        | Pair spmm -> %(spmm |> SorterPairModelMutator.getId) |> UMX.tag<sortingMutatorId>


    let getMutatorSeedSortingIdWithTags (model: sortingMutator) : Guid<sortingId> * (modelTag []) =
        match model with
        | Single smm -> smm |> SorterModelMutator.getMutatorSeedSortingIdWithTags
        | Pair spmm -> spmm |> SorterPairModelMutator.getMutatorSeedSortingIdWithTags


    let getMutantSortingId (index: int) (model: sortingMutator) : Guid<sortingId> =
        match model with
        | Single smm -> smm |> SorterModelMutator.getMutantSortingId index
        | Pair spmm -> spmm |> SorterPairModelMutator.getMutantSortingId index


    let getSortingWidth (model: sortingMutator) : int<sortingWidth> =
        match model with
        | Single smm -> smm |> SorterModelMutator.getSortingWidth
        | Pair spmm -> spmm |> SorterPairModelMutator.getSortingWidth


    let getCeLength (model: sortingMutator) : int<ceLength> =
        match model with
        | Single smm -> smm |> SorterModelMutator.getCeLength
        | Pair spmm -> spmm |> SorterPairModelMutator.getCeLength


    let makeSorting
                (index: int)  
                (model: sortingMutator) : sorting =
        match model with
        | Single smm -> smm |> SorterModelMutator.makeSorterModel index |> sorting.Single
        | Pair spmm -> spmm |> SorterPairModelMutator.makeSorterPairModel index |> sorting.Pairs


    let makeSorterIdsWithTags (index: int) 
                              (model: sortingMutator)  
                                : (Guid<sorterId> * modelTag) [] = 
        match model with
        | Single smm -> smm |> SorterModelMutator.makeSorterIdWithTag index |> Array.singleton
        | Pair spmm -> spmm |> SorterPairModelMutator.makeSorterIdsWithTags index




