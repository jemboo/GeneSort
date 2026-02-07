namespace GeneSort.Model.Sorter

open GeneSort.Sorting.Sorter
open GeneSort.Sorting
open FSharp.UMX


type sortingModel =
     | Sorter of sorterModel


module SortingModel =
    
    let getId (model: sortingModel) : Guid<sortingModelID> =
        match model with
        | Sorter sorter -> sorter |> SorterModel.getId


    let makeSorter (model: sortingModel) : sorter =
        match model with
        | Sorter sorter ->sorter |> SorterModel.makeSorter


    let getSortingWidth (model: sortingModel) : int<sortingWidth> =
        match model with
        | Sorter sorter -> sorter |> SorterModel.getSortingWidth


    let getCeLength (model: sortingModel) : int<ceLength> =
        match model with
        | Sorter sorter -> sorter |> SorterModel.getCeLength

