namespace GeneSort.Model.Sorting

open FSharp.UMX

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


    let getParentSorterIdsWithTags (model: sortingMutator) : (Guid<sorterId> * modelTag) [] =
        match model with
        | Single smm -> smm |> SorterModelMutator.getParentSorterIdsWithTags
        | Pair spmm -> spmm |> SorterPairModelMutator.getParentSorterIdsWithTags

    

    let getParentSortingId (model: sortingMutator) : Guid<sortingId> =
        match model with
        | Single smm -> %(smm |> SorterModelMutator.getParentSorterModelId) |> UMX.tag<sortingId>
        | Pair spmm -> %(spmm |> SorterPairModelMutator.getParentSorterPairId) |> UMX.tag<sortingId>


    let getParentSorting (model: sortingMutator) : sorting =
        match model with
        | Single smm -> smm |> SorterModelMutator.getParentSorterModel |> sorting.Single
        | Pair psm -> psm |> SorterPairModelMutator.getParentSorterPair |> sorting.Pairs


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


    let makeMutantSorting
                (index: int)  
                (model: sortingMutator) : sorting =
        match model with
        | Single smm -> smm |> SorterModelMutator.makeMutantSorterModel index |> sorting.Single
        | Pair spmm -> spmm |> SorterPairModelMutator.makeMutantSorterPairModel index |> sorting.Pairs


    let makeMutantSorterIdsWithTags 
                        (index: int) 
                        (model: sortingMutator) : (Guid<sorterId> * modelTag) [] = 
        match model with
        | Single smm -> smm |> SorterModelMutator.makeMutantSorterIdWithTag index |> Array.singleton
        | Pair spmm -> spmm |> SorterPairModelMutator.makeMutantSorterIdsWithTags index




