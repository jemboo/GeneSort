namespace GeneSort.Model.Sorting

open System
open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair


type sortingModelMutator =
     | Single of sorterModelMutator
     | Pair of sorterPairModelMutator


module SortingModelMutator =

    let getId (model:sortingModelMutator) : Guid<sortingModelMutatorId> =
        match model with
        | Single smm -> %(smm |> SorterModelMutator.getId) |> UMX.tag<sortingModelMutatorId>
        | Pair spmm -> %(spmm |> SorterPairModelMutator.getId) |> UMX.tag<sortingModelMutatorId>


    let getSortingModelSeedId (model: sortingModelMutator) : Guid<sortingModelId> =
        match model with
        | Single smm -> smm |> SorterModelMutator.getSortingModelSeedId
        | Pair spmm -> failwith "SorterPairModelMutator does not have a single sorting model seed ID"


    let getSortingWidth (model: sortingModelMutator) : int<sortingWidth> =
        match model with
        | Single smm -> smm |> SorterModelMutator.getSortingWidth
        | Pair spmm -> spmm |> SorterPairModelMutator.getSortingWidth


    let getCeLength (model: sortingModelMutator) : int<ceLength> =
        match model with
        | Single smm -> smm |> SorterModelMutator.getCeLength
        | Pair spmm -> spmm |> SorterPairModelMutator.getCeLength


    let makeSortingModel
                (index: int)  
                (model: sortingModelMutator) : sortingModel =
        match model with
        | Single smm -> smm |> SorterModelMutator.makeSorterModel index |> sortingModel.Single
        | Pair spmm -> spmm |> SorterPairModelMutator.makeSorterPairModel index |> sortingModel.Pair


    let makeSorterIdsWithTags (index: int) (model: sortingModelMutator)  
                                        : (Guid<sorterId> * modelTag) [] = 
        match model with
        | Single smm -> smm |> SorterModelMutator.makeSorterIdWithTag index |> Array.singleton
        | Pair spmm -> spmm |> SorterPairModelMutator.makeSorterIdsWithTags index




