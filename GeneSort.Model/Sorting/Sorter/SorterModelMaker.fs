namespace GeneSort.Model.Sorting

open System
open FSharp.UMX
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting


type sorterModelMaker =
     | SmmMsceRandGen of msceRandGen
     | SmmMssiRandGen of mssiRandGen
     | SmmMsrsRandGen of msrsRandGen
     | SmmMsuf4RandGen of msuf4RandGen
     | SmmMsuf6RandGen of msuf6RandGen


module SorterModelMaker =

    let getId (model:sorterModelMaker) : Guid<sorterModelMakerId> =
        match model with
        | SmmMsceRandGen msce -> msce.Id
        | SmmMssiRandGen mssi -> mssi.Id
        | SmmMsrsRandGen msrs -> msrs.Id
        | SmmMsuf4RandGen msuf4 -> msuf4.Id
        | SmmMsuf6RandGen msuf6 -> msuf6.Id


    let getSortingWidth (model: sorterModelMaker) : int<sortingWidth> =
        match model with
        | SmmMsceRandGen msce -> msce.SortingWidth
        | SmmMssiRandGen mssi -> mssi.SortingWidth
        | SmmMsrsRandGen msrs -> msrs.SortingWidth
        | SmmMsuf4RandGen msuf4 -> msuf4.SortingWidth
        | SmmMsuf6RandGen msuf6 -> msuf6.SortingWidth


    let getCeLength (model: sorterModelMaker) : int<ceLength> =
        match model with
        | SmmMsceRandGen msce -> msce.CeLength
        | SmmMssiRandGen mssi -> mssi.CeLength
        | SmmMsrsRandGen msrs -> msrs.CeLength
        | SmmMsuf4RandGen msuf4 -> msuf4.CeLength
        | SmmMsuf6RandGen msuf6 -> msuf6.CeLength


    let getSorterModelMakerId (model: sorterModelMaker) : Guid<sorterModelMakerId> =
        match model with
        | SmmMsceRandGen msce -> msce.Id
        | SmmMssiRandGen mssi -> mssi.Id
        | SmmMsrsRandGen msrs -> msrs.Id
        | SmmMsuf4RandGen msuf4 -> msuf4.Id
        | SmmMsuf6RandGen msuf6 -> msuf6.Id


    let makeSorterModel (index: int)  (model: sorterModelMaker) : sorterModel =
        match model with
        | SmmMsceRandGen msce -> msce.MakeSorterModel index |> sorterModel.Msce
        | SmmMssiRandGen mssi -> mssi.MakeSorterModel index |> sorterModel.Mssi
        | SmmMsrsRandGen msrs -> msrs.MakeSorterModel index |> sorterModel.Msrs
        | SmmMsuf4RandGen msuf4 -> msuf4.MakeSorterModel index |> sorterModel.Msuf4
        | SmmMsuf6RandGen msuf6 -> msuf6.MakeSorterModel index |> sorterModel.Msuf6


    let makeSorterIdWithTag (index:int) (model:sorterModelMaker) : Guid<sorterId> * modelTag =
        match model with
        | SmmMsceRandGen msce -> (%(msce.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMssiRandGen mssi -> (%(mssi.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsrsRandGen msrs -> (%(msrs.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsuf4RandGen msuf4 -> (%(msuf4.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsuf6RandGen msuf6 -> (%(msuf6.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)



