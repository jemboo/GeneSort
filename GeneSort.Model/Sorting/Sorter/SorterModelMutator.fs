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


type sorterModelMutator =
     | SmmMsceRandMutate of msceRandMutate
     | SmmMssiRandMutate of mssiRandMutate
     | SmmMsrsRandMutate of msrsRandMutate
     | SmmMsuf4RandMutate of msuf4RandMutate
     | SmmMsuf6RandMutate of msuf6RandMutate


module SorterModelMutator =

    let getId (model:sorterModelMutator) : Guid<sorterModelMutatorId> =
        match model with
        | SmmMsceRandMutate msce -> msce.Id
        | SmmMssiRandMutate mssi -> mssi.Id
        | SmmMsrsRandMutate msrs -> msrs.Id
        | SmmMsuf4RandMutate msuf4 -> msuf4.Id
        | SmmMsuf6RandMutate msuf6 -> msuf6.Id


    let getSortingModelSeedId (model: sorterModelMutator) : Guid<sortingModelId> =
        match model with
        | SmmMsceRandMutate msce -> %msce.Msce.Id |> UMX.tag<sortingModelId>
        | SmmMssiRandMutate mssi -> %mssi.Mssi.Id |> UMX.tag<sortingModelId>
        | SmmMsrsRandMutate msrs -> %msrs.Msrs.Id |> UMX.tag<sortingModelId>
        | SmmMsuf4RandMutate msuf4 -> %msuf4.Msuf4.Id |> UMX.tag<sortingModelId>
        | SmmMsuf6RandMutate msuf6 -> %msuf6.Msuf6.Id |> UMX.tag<sortingModelId>


    let getSortingWidth (model: sorterModelMutator) : int<sortingWidth> =
        match model with
        | SmmMsceRandMutate msce -> msce.SortingWidth
        | SmmMssiRandMutate mssi -> mssi.SortingWidth
        | SmmMsrsRandMutate msrs -> msrs.SortingWidth
        | SmmMsuf4RandMutate msuf4 -> msuf4.SortingWidth
        | SmmMsuf6RandMutate msuf6 -> msuf6.SortingWidth


    let getCeLength (model: sorterModelMutator) : int<ceLength> =
        match model with
        | SmmMsceRandMutate msce -> msce.CeLength
        | SmmMssiRandMutate mssi -> mssi.CeLength
        | SmmMsrsRandMutate msrs -> msrs.CeLength
        | SmmMsuf4RandMutate msuf4 -> msuf4.CeLength
        | SmmMsuf6RandMutate msuf6 -> msuf6.CeLength


    let makeSorterModel 
                (index: int)  
                (model: sorterModelMutator) : sorterModel =
        match model with
        | SmmMsceRandMutate msce -> msce.MakeSorterModel index |> sorterModel.Msce
        | SmmMssiRandMutate mssi -> mssi.MakeSorterModel index |> sorterModel.Mssi
        | SmmMsrsRandMutate msrs -> msrs.MakeSorterModel index |> sorterModel.Msrs
        | SmmMsuf4RandMutate msuf4 -> msuf4.MakeSorterModel index |> sorterModel.Msuf4
        | SmmMsuf6RandMutate msuf6 -> msuf6.MakeSorterModel index |> sorterModel.Msuf6


    let makeSorterIdWithTag (index:int) (model:sorterModelMutator) : Guid<sorterId> * modelTag =
        match model with
        | SmmMsceRandMutate msce -> (%(msce.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMssiRandMutate mssi -> (%(mssi.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsrsRandMutate msrs -> (%(msrs.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsuf4RandMutate msuf4 -> (%(msuf4.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsuf6RandMutate msuf6 -> (%(msuf6.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)





