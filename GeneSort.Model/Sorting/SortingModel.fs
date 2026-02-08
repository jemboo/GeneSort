namespace GeneSort.Model.Sorter

open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting
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

    let containsSorter (sorterId: Guid<sorterId>) (model: sortingModel) : bool =
        match model with
        | Single sms -> %(sms.SorterModel |> SorterModel.getId) = (%sorterId)
        | Pair smp -> smp |> SortingModelPair.hasChild sorterId


    let containsAnySorter (sorterIds: Guid<sorterId> []) (model: sortingModel) : bool =
        match model with
        | Single sms -> sorterIds |> Array.exists (fun id -> %(sms.SorterModel |> SorterModel.getId) = (%id))
        | Pair smp -> sorterIds |> Array.exists (fun id -> smp |> SortingModelPair.hasChild id)