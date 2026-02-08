namespace GeneSort.Model.Sorter

open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorter
open FSharp.UMX


type sortingModel =
     | Single of sortingModelSingle
     | Pair of sortingModelPair


module SortingModel =
    
    let getId (model: sortingModel) : Guid<sortingModelID> =
        match model with
        | Single sms -> sms.Id
        | Pair smp -> smp.Id
                             
    let makeSorters (model: sortingModel) : sorter []  =
        match model with
        | Single sms -> sms.SorterModel |> SorterModel.makeSorter |> Array.singleton
        | Pair smp -> smp.SorterPairModel |> SorterPairModel.makeSorters |> Array.singleton

    let containsSorterModel (sorterModelId: Guid<sorterModelID>) (model: sortingModel) : bool =
        match model with
        | Single sms -> sms.SorterModel |> SorterModel.getId |> (=) (%sorterModelId)
        | Pair smp -> smp |> SortingModelPair.hasChild sorterModelId


