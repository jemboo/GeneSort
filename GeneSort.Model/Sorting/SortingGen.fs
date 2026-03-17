namespace GeneSort.Model.Sorting

open FSharp.UMX

open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair


type sortingGen =
     | Single of sorterModelGen
     | Pair of sorterPairModelGen


module SortingGen =

    let getId (model:sortingGen) : Guid<sortingGenId> =
        match model with
        | Single smm -> %(smm |> SorterModelGen.getId) |> UMX.tag<sortingGenId>
        | Pair spmm -> %(spmm |> SorterPairModelGen.getId) |> UMX.tag<sortingGenId>


    let getSortingWidth (model: sortingGen) : int<sortingWidth> =
        match model with
        | Single smm -> smm |> SorterModelGen.getSortingWidth
        | Pair spmm -> spmm |> SorterPairModelGen.getSortingWidth


    let getCeLength (model: sortingGen) : int<ceLength> =
        match model with
        | Single smm -> smm |> SorterModelGen.getCeLength
        | Pair spmm -> spmm |> SorterPairModelGen.getCeLength


    let makeSortingFromIndex
                (index: int)  
                (model: sortingGen) : sorting =
        match model with
        | Single smm -> smm |> SorterModelGen.makeSorterModelFromIndex index |> sorting.Single
        | Pair spmm -> spmm |> SorterPairModelGen.makeSorterPairModelFromIndex index |> sorting.Pairs


    let makeSortingFromId
                (sortingId: Guid<sortingId>) 
                (model: sortingGen) : sorting =
        match model with
        | Single smm -> smm |> SorterModelGen.makeSorterModelFromId sortingId |> sorting.Single
        | Pair spmm -> spmm |> SorterPairModelGen.makeSorterPairModelFromId sortingId |> sorting.Pairs

    let makeSorterFromIdAndTag
                (sortingId: Guid<sortingId>) 
                (model: sortingGen) : sorting =
        match model with
        | Single smm -> smm |> SorterModelGen.makeSorterModelFromId sortingId |> sorting.Single
        | Pair spmm -> spmm |> SorterPairModelGen.makeSorterPairModelFromId sortingId |> sorting.Pairs

    //let makeSorterIdsWithTags 
    //             (index: int) 
    //             (model: sortingGen) : (Guid<sorterId> * modelTag) [] = 
    //    match model with
    //    | Single smm -> smm |> SorterModelGen.makeSorterIdWithTag index |> Array.singleton
    //    | Pair spmm -> spmm |> SorterPairModelGen.makeSorterIdsWithTags index
