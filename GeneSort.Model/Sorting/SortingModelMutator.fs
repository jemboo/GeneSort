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

    let getId (model:sortingModelMutator) : Guid<sortingModelMutatorID> =
        match model with
        | Single smm -> %(smm |> SorterModelMutator.getId) |> UMX.tag<sortingModelMutatorID>
        | Pair spmm -> %(spmm |> SorterPairModelMutator.getId) |> UMX.tag<sortingModelMutatorID>


    let getSortingWidth (model: sortingModelMutator) : int<sortingWidth> =
        match model with
        | Single smm -> smm |> SorterModelMutator.getSortingWidth
        | Pair spmm -> spmm |> SorterPairModelMutator.getSortingWidth


    let getCeLength (model: sortingModelMutator) : int<ceLength> =
        match model with
        | Single smm -> smm |> SorterModelMutator.getCeLength
        | Pair spmm -> spmm |> SorterPairModelMutator.getCeLength


    let makeSortingModel 
                (rngFactory: rngType -> Guid -> IRando)
                (index: int)  
                (model: sortingModelMutator) : sortingModel =
        match model with
        | Single smm -> smm |> SorterModelMutator.makeSorterModel rngFactory index |> sortingModel.Single
        | Pair spmm -> spmm |> SorterPairModelMutator.makeSorterPairModel rngFactory index |> sortingModel.Pair



