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

    let getId (model:sortingModelMaker) : Guid<sortingModelMakerID> =
        match model with
        | Single smm -> %(smm |> SorterModelMaker.getId) |> UMX.tag<sortingModelMakerID>
        | Pair spmm -> %(spmm |> SorterPairModelMaker.getId) |> UMX.tag<sortingModelMakerID>


    let getSortingWidth (model: sortingModelMaker) : int<sortingWidth> =
        match model with
        | Single smm -> smm |> SorterModelMaker.getSortingWidth
        | Pair spmm -> spmm |> SorterPairModelMaker.getSortingWidth


    let getCeLength (model: sortingModelMaker) : int<ceLength> =
        match model with
        | Single smm -> smm |> SorterModelMaker.getCeLength
        | Pair spmm -> spmm |> SorterPairModelMaker.getCeLength

    let makeSortingModel 
                (rngFactory: rngFactory)
                (index: int)  
                (model: sortingModelMaker) : sortingModel =
        match model with
        | Single smm -> smm |> SorterModelMaker.makeSorterModel rngFactory index |> sortingModel.Single
        | Pair spmm -> spmm |> SorterPairModelMaker.makeSorterPairModel rngFactory index |> sortingModel.Pair



