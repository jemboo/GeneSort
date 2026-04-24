namespace GeneSort.Model.Sorting.V1
open FSharp.UMX

open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1.Simple


type sorterModel =
     | Simple of simpleSorterModel
     | Unknown


module SorterModel =

    let getSortingWidth (model: sorterModel) : int<sortingWidth> =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.getSortingWidth
        | Unknown -> failwith "Unknown sorterModel"


    let getStageLength (model: sorterModel) : int<stageLength> =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.getStageLength
        | Unknown -> failwith "Unknown sorterModel"

    
    let getId (model: sorterModel) : Guid<sorterModelId> =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.getId
        | Unknown -> failwith "Unknown sorterModel"


    let makeSorter (model: sorterModel) : sorter =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.makeSorter
        | Unknown -> failwith "Unknown sorterModel"




