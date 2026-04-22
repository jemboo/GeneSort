namespace GeneSort.Model.Sorting.V1
open FSharp.UMX

open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1.Simple


type sorterModel =
     | Simple of simpleSorterModel
     | Next of simpleSorterModel


module SorterModel =

    let getSortingWidth (model: sorterModel) : int<sortingWidth> =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.getSortingWidth
        | Next smp -> smp |> SimpleSorterModel.getSortingWidth


    let getStageLength (model: sorterModel) : int<stageLength> =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.getStageLength
        | Next sms -> sms |> SimpleSorterModel.getStageLength

    
    let getId (model: sorterModel) : Guid<sorterModelId> =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.getId
        | Next sms -> sms |> SimpleSorterModel.getId


    let makeSorter (model: sorterModel) : sorter =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.makeSorter
        | Next sms -> sms |> SimpleSorterModel.makeSorter




