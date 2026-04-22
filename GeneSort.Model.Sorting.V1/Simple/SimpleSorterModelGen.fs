namespace GeneSort.Model.Sorting.Simple.V1

open FSharp.UMX
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Model.Sorting.V1.Simple.Uf6
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple


type simpleSorterModelGen =
     | SmmMsceRandGen of msceRandGen
     | SmmMssiRandGen of mssiRandGen
     | SmmMsrsRandGen of msrsRandGen
     | SmmMsuf4RandGen of msuf4RandGen
     | SmmMsuf6RandGen of msuf6RandGen


module SorterModelGen =

    let getId (model:simpleSorterModelGen) : Guid<sorterModelGenId> =
        match model with
        | SmmMsceRandGen msce -> msce.Id
        | SmmMssiRandGen mssi -> mssi.Id
        | SmmMsrsRandGen msrs -> msrs.Id
        | SmmMsuf4RandGen msuf4 -> msuf4.Id
        | SmmMsuf6RandGen msuf6 -> msuf6.Id


    let getSortingWidth (model: simpleSorterModelGen) : int<sortingWidth> =
        match model with
        | SmmMsceRandGen msce -> msce.SortingWidth
        | SmmMssiRandGen mssi -> mssi.SortingWidth
        | SmmMsrsRandGen msrs -> msrs.SortingWidth
        | SmmMsuf4RandGen msuf4 -> msuf4.SortingWidth
        | SmmMsuf6RandGen msuf6 -> msuf6.SortingWidth


    let getCeLength (model: simpleSorterModelGen) : int<ceLength> =
        match model with
        | SmmMsceRandGen msce -> msce.CeLength
        | SmmMssiRandGen mssi -> mssi.CeLength
        | SmmMsrsRandGen msrs -> msrs.CeLength
        | SmmMsuf4RandGen msuf4 -> msuf4.CeLength
        | SmmMsuf6RandGen msuf6 -> msuf6.CeLength


    let getSorterModelMakerId (model: simpleSorterModelGen) : Guid<sorterModelGenId> =
        match model with
        | SmmMsceRandGen msce -> msce.Id
        | SmmMssiRandGen mssi -> mssi.Id
        | SmmMsrsRandGen msrs -> msrs.Id
        | SmmMsuf4RandGen msuf4 -> msuf4.Id
        | SmmMsuf6RandGen msuf6 -> msuf6.Id


    let makeSorterModelFromIndex (index: int)  (model: simpleSorterModelGen) : simpleSorterModel =
        match model with
        | SmmMsceRandGen msce -> msce.MakeSorterModelFromIndex index |> simpleSorterModel.Msce
        | SmmMssiRandGen mssi -> mssi.MakeSorterModelFromIndex index |> simpleSorterModel.Mssi
        | SmmMsrsRandGen msrs -> msrs.MakeSorterModelFromIndex index |> simpleSorterModel.Msrs
        | SmmMsuf4RandGen msuf4 -> msuf4.MakeSorterModelFromIndex index |> simpleSorterModel.Msuf4
        | SmmMsuf6RandGen msuf6 -> msuf6.MakeSorterModelFromIndex index |> simpleSorterModel.Msuf6


    //let makeSorterModelFromId 
    //                (sortingId: Guid<sortingId>)  
    //                (model: simpleSorterModelGen) : simpleSorterModel =
    //    let sorterModelId = %sortingId |> UMX.tag<simpleSorterModelId>
    //    match model with
    //    | SmmMsceRandGen msce -> msce.MakeSorterModelFromId sorterModelId |> simpleSorterModel.Msce
    //    | SmmMssiRandGen mssi -> mssi.MakeSorterModelFromId sorterModelId |> sorterModel.Mssi
    //    | SmmMsrsRandGen msrs -> msrs.MakeSorterModelFromId sorterModelId |> sorterModel.Msrs
    //    | SmmMsuf4RandGen msuf4 -> msuf4.MakeSorterModelFromId sorterModelId |> sorterModel.Msuf4
    //    | SmmMsuf6RandGen msuf6 -> msuf6.MakeSorterModelFromId sorterModelId |> sorterModel.Msuf6





