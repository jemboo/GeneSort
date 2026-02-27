namespace GeneSort.Model.Sorting

open System
open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair


type sortingMaker =
     | Single of sorterModelMaker
     | Pair of sorterPairModelMaker


module SortingMaker =

    let getId (model:sortingMaker) : Guid<sortingMakerId> =
        match model with
        | Single smm -> %(smm |> SorterModelMaker.getId) |> UMX.tag<sortingMakerId>
        | Pair spmm -> %(spmm |> SorterPairModelMaker.getId) |> UMX.tag<sortingMakerId>


    let getSortingWidth (model: sortingMaker) : int<sortingWidth> =
        match model with
        | Single smm -> smm |> SorterModelMaker.getSortingWidth
        | Pair spmm -> spmm |> SorterPairModelMaker.getSortingWidth


    let getCeLength (model: sortingMaker) : int<ceLength> =
        match model with
        | Single smm -> smm |> SorterModelMaker.getCeLength
        | Pair spmm -> spmm |> SorterPairModelMaker.getCeLength

    let makeSorting
                (index: int)  
                (model: sortingMaker) : sorting =
        match model with
        | Single smm -> smm |> SorterModelMaker.makeSorterModel index |> sorting.Single
        | Pair spmm -> spmm |> SorterPairModelMaker.makeSorterPairModel index |> sorting.Pair


    let makeSorterIdsWithTags (index: int) (model: sortingMaker)  
                                        : (Guid<sorterId> * modelTag) [] = 
        match model with
        | Single smm -> smm |> SorterModelMaker.makeSorterIdWithTag index |> Array.singleton
        | Pair spmm -> spmm |> SorterPairModelMaker.makeSorterIdsWithTags index
