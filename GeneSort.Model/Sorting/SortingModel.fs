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

    let getSortingWidth (model: sortingModel) : int<sortingWidth> =
        match model with
        | Single sms -> sms |> SorterModel.getSortingWidth
        | Pair smp -> smp |> SorterPairModel.getSortingWidth


    let getStageLength (model: sortingModel) : int<stageLength> =
        match model with
        | Single sms -> sms |> SorterModel.getStageLength
        | Pair smp -> smp |> SorterPairModel.getStageLength

    
    let getId (model: sortingModel) : Guid<sortingModelId> =
        match model with
        | Single sms -> %(sms |> SorterModel.getId) |> UMX.tag<sortingModelId>
        | Pair smp -> %(smp |> SorterPairModel.getId) |> UMX.tag<sortingModelId>


    let makeSorters (model: sortingModel) : sorter []  =
        match model with
        | Single sms -> sms |> SorterModel.makeSorter |> Array.singleton
        | Pair smp ->  smp |> SorterPairModel.makeSorters


    let containsSorter (sorterId: Guid<sorterId>) (model: sortingModel) : bool =
        match model with
        | Single sms -> %(sms |> SorterModel.getId) = (%sorterId)
        | Pair smp -> sorterId |> SorterPairModel.isAChildOf smp


    let containsAnySorter (sorterIds: Guid<sorterId> []) (model: sortingModel) : bool =
        match model with
        | Single sms -> sorterIds |> Array.exists (fun id -> %(sms |> SorterModel.getId) = (%id))
        | Pair smp -> 
            sorterIds |> Array.exists (fun id -> id |> SorterPairModel.isAChildOf smp)


    let getSorterIdForModelTag  (model: sortingModel) (tag: modelTag) : Guid<sorterId> =
        match model with
        | Single sms -> SorterModel.getSorterIdForModelTag sms tag
        | Pair smp -> SorterPairModel.getSorterIdForModelTag smp tag