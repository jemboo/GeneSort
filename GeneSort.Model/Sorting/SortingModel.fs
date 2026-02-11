namespace GeneSort.Model.Sorting
open FSharp.UMX

open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair


type sortingModel =
     | Single of sorterModel
     | Pair of sorterPairModel


module SortingModel =
    
    let getId (model: sortingModel) : Guid<sortingModelID> =
        match model with
        | Single sms -> %(sms |> SorterModel.getId) |> UMX.tag<sortingModelID>
        | Pair smp -> %(smp |> SorterPairModel.getId) |> UMX.tag<sortingModelID>
                             
    let makeSorters (model: sortingModel) : sorter []  =
        match model with
        | Single sms -> sms |> SorterModel.makeSorter |> Array.singleton
        | Pair smp -> smp |> SorterPairModel.makeSorters |> Array.singleton

    let containsSorter (sorterId: Guid<sorterId>) (model: sortingModel) : bool =
        match model with
        | Single sms -> %(sms |> SorterModel.getId) = (%sorterId)
        | Pair smp -> sorterId |> SorterPairModel.isAChildOf smp


    let containsAnySorter (sorterIds: Guid<sorterId> []) (model: sortingModel) : bool =
        match model with
        | Single sms -> sorterIds |> Array.exists (fun id -> %(sms |> SorterModel.getId) = (%id))
        | Pair smp -> 
            let sorterId = %(smp |> SorterPairModel.getId) |> UMX.tag<sorterId>
            sorterIds |> Array.exists (fun id -> id |> SorterPairModel.isAChildOf smp)