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


    let getSortingSeedId (model: sortingMutator) : Guid<sortingId> =
        match model with
        | Single smm -> smm |> SorterModelMutator.getSortingSeedId
        | Pair spmm -> failwith "SorterPairModelMutator does not have a single sorting model seed ID"


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
        | Pair spmm -> spmm |> SorterPairModelMutator.makeSorterPairModel index |> sorting.Pair


    let makeSorterIdsWithTags (index: int) (model: sortingMutator)  
                                        : (Guid<sorterId> * modelTag) [] = 
        match model with
        | Single smm -> smm |> SorterModelMutator.makeSorterIdWithTag index |> Array.singleton
        | Pair spmm -> spmm |> SorterPairModelMutator.makeSorterIdsWithTags index




