namespace GeneSort.Model.Sorting

open System
open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair


type sortingModelMaker =
     | Single of sorterModelMaker
     | Pair of sorterPairModelMaker


module SortingModelMaker =

    let getId (model:sortingModelMaker) : Guid<sortingModelMakerId> =
        match model with
        | Single smm -> %(smm |> SorterModelMaker.getId) |> UMX.tag<sortingModelMakerId>
        | Pair spmm -> %(spmm |> SorterPairModelMaker.getId) |> UMX.tag<sortingModelMakerId>


    let getSortingWidth (model: sortingModelMaker) : int<sortingWidth> =
        match model with
        | Single smm -> smm |> SorterModelMaker.getSortingWidth
        | Pair spmm -> spmm |> SorterPairModelMaker.getSortingWidth


    let getCeLength (model: sortingModelMaker) : int<ceLength> =
        match model with
        | Single smm -> smm |> SorterModelMaker.getCeLength
        | Pair spmm -> spmm |> SorterPairModelMaker.getCeLength

    let makeSortingModel
                (index: int)  
                (model: sortingModelMaker) : sortingModel =
        match model with
        | Single smm -> smm |> SorterModelMaker.makeSorterModel index |> sortingModel.Single
        | Pair spmm -> spmm |> SorterPairModelMaker.makeSorterPairModel index |> sortingModel.Pair


    let makeSorterIdsWithTags (index: int) (model: sortingModelMaker)  
                                        : (Guid<sorterId> * modelTag) [] = 
        match model with
        | Single smm -> smm |> SorterModelMaker.makeSorterIdWithTag index |> Array.singleton
        | Pair spmm -> spmm |> SorterPairModelMaker.makeSorterIdsWithTags index
