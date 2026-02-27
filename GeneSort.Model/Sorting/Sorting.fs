namespace GeneSort.Model.Sorting
open FSharp.UMX

open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair


type sorting =
     | Single of sorterModel
     | Pair of sorterPairModel


module Sorting =

    let getSortingWidth (model: sorting) : int<sortingWidth> =
        match model with
        | Single sms -> sms |> SorterModel.getSortingWidth
        | Pair smp -> smp |> SorterPairModel.getSortingWidth


    let getStageLength (model: sorting) : int<stageLength> =
        match model with
        | Single sms -> sms |> SorterModel.getStageLength
        | Pair smp -> smp |> SorterPairModel.getStageLength

    
    let getId (model: sorting) : Guid<sortingId> =
        match model with
        | Single sms -> %(sms |> SorterModel.getId) |> UMX.tag<sortingId>
        | Pair smp -> %(smp |> SorterPairModel.getId) |> UMX.tag<sortingId>


    let makeSorters (model: sorting) : sorter []  =
        match model with
        | Single sms -> sms |> SorterModel.makeSorter |> Array.singleton
        | Pair smp ->  smp |> SorterPairModel.makeSorters


    let containsSorter (sorterId: Guid<sorterId>) (model: sorting) : bool =
        match model with
        | Single sms -> %(sms |> SorterModel.getId) = (%sorterId)
        | Pair smp -> sorterId |> SorterPairModel.isAChildOf smp


    let containsAnySorter (sorterIds: Guid<sorterId> []) (model: sorting) : bool =
        match model with
        | Single sms -> sorterIds |> Array.exists (fun id -> %(sms |> SorterModel.getId) = (%id))
        | Pair smp -> 
            sorterIds |> Array.exists (fun id -> id |> SorterPairModel.isAChildOf smp)


    let getSorterIdForModelTag  (model: sorting) (tag: modelTag) : Guid<sorterId> =
        match model with
        | Single sms -> SorterModel.getSorterIdForModelTag sms tag
        | Pair smp -> SorterPairModel.getSorterIdForModelTag smp tag