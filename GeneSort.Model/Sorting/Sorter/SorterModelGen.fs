namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Sorting
open GeneSort.Model.Sorting


type sorterModelGen =
     | SmmMsceRandGen of msceRandGen
     | SmmMssiRandGen of mssiRandGen
     | SmmMsrsRandGen of msrsRandGen
     | SmmMsuf4RandGen of msuf4RandGen
     | SmmMsuf6RandGen of msuf6RandGen


module SorterModelGen =

    let getId (model:sorterModelGen) : Guid<sorterModelGenId> =
        match model with
        | SmmMsceRandGen msce -> msce.Id
        | SmmMssiRandGen mssi -> mssi.Id
        | SmmMsrsRandGen msrs -> msrs.Id
        | SmmMsuf4RandGen msuf4 -> msuf4.Id
        | SmmMsuf6RandGen msuf6 -> msuf6.Id


    let getSortingWidth (model: sorterModelGen) : int<sortingWidth> =
        match model with
        | SmmMsceRandGen msce -> msce.SortingWidth
        | SmmMssiRandGen mssi -> mssi.SortingWidth
        | SmmMsrsRandGen msrs -> msrs.SortingWidth
        | SmmMsuf4RandGen msuf4 -> msuf4.SortingWidth
        | SmmMsuf6RandGen msuf6 -> msuf6.SortingWidth


    let getCeLength (model: sorterModelGen) : int<ceLength> =
        match model with
        | SmmMsceRandGen msce -> msce.CeLength
        | SmmMssiRandGen mssi -> mssi.CeLength
        | SmmMsrsRandGen msrs -> msrs.CeLength
        | SmmMsuf4RandGen msuf4 -> msuf4.CeLength
        | SmmMsuf6RandGen msuf6 -> msuf6.CeLength


    let getSorterModelMakerId (model: sorterModelGen) : Guid<sorterModelGenId> =
        match model with
        | SmmMsceRandGen msce -> msce.Id
        | SmmMssiRandGen mssi -> mssi.Id
        | SmmMsrsRandGen msrs -> msrs.Id
        | SmmMsuf4RandGen msuf4 -> msuf4.Id
        | SmmMsuf6RandGen msuf6 -> msuf6.Id


    let makeSorterModelFromIndex (index: int)  (model: sorterModelGen) : sorterModel =
        match model with
        | SmmMsceRandGen msce -> msce.MakeSorterModelFromIndex index |> sorterModel.Msce
        | SmmMssiRandGen mssi -> mssi.MakeSorterModelFromIndex index |> sorterModel.Mssi
        | SmmMsrsRandGen msrs -> msrs.MakeSorterModelFromIndex index |> sorterModel.Msrs
        | SmmMsuf4RandGen msuf4 -> msuf4.MakeSorterModelFromIndex index |> sorterModel.Msuf4
        | SmmMsuf6RandGen msuf6 -> msuf6.MakeSorterModelFromIndex index |> sorterModel.Msuf6


    let makeSorterModelFromId 
                    (sortingId: Guid<sortingId>)  
                    (model: sorterModelGen) : sorterModel =
        let sorterModelId = %sortingId |> UMX.tag<sorterModelId>
        match model with
        | SmmMsceRandGen msce -> msce.MakeSorterModelFromId sorterModelId |> sorterModel.Msce
        | SmmMssiRandGen mssi -> mssi.MakeSorterModelFromId sorterModelId |> sorterModel.Mssi
        | SmmMsrsRandGen msrs -> msrs.MakeSorterModelFromId sorterModelId |> sorterModel.Msrs
        | SmmMsuf4RandGen msuf4 -> msuf4.MakeSorterModelFromId sorterModelId |> sorterModel.Msuf4
        | SmmMsuf6RandGen msuf6 -> msuf6.MakeSorterModelFromId sorterModelId |> sorterModel.Msuf6



    let makeSorterIdWithTag (index:int) (model:sorterModelGen) : Guid<sorterId> * modelTag =
        match model with
        | SmmMsceRandGen msce -> (%(msce.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMssiRandGen mssi -> (%(mssi.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsrsRandGen msrs -> (%(msrs.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsuf4RandGen msuf4 -> (%(msuf4.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsuf6RandGen msuf6 -> (%(msuf6.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)



