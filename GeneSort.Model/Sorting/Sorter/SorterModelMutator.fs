namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Sorting.Sorter


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


    let getParentSorterModel (model:sorterModelMutator) : sorterModel =
        match model with
        | SmmMsceRandMutate msce -> msce.Msce |> sorterModel.Msce
        | SmmMssiRandMutate mssi -> mssi.Mssi |> sorterModel.Mssi
        | SmmMsrsRandMutate msrs -> msrs.Msrs |> sorterModel.Msrs
        | SmmMsuf4RandMutate msuf4 -> msuf4.Msuf4 |> sorterModel.Msuf4
        | SmmMsuf6RandMutate msuf6 -> msuf6.Msuf6 |> sorterModel.Msuf6


    let getParentSorterModelId (model: sorterModelMutator) : Guid<sorterModelId> =
        match model with
        | SmmMsceRandMutate msce -> msce.Msce.Id
        | SmmMssiRandMutate mssi -> mssi.Mssi.Id
        | SmmMsrsRandMutate msrs -> msrs.Msrs.Id
        | SmmMsuf4RandMutate msuf4 -> msuf4.Msuf4.Id
        | SmmMsuf6RandMutate msuf6 -> msuf6.Msuf6.Id


    let getParentSorterIdsWithTags (model: sorterModelMutator) : (Guid<sorterId>  * modelTag) [] =
        match model with
        | SmmMsceRandMutate msce -> [|(%msce.Msce.Id |> UMX.tag<sorterId>,  modelTag.Single )|]
        | SmmMssiRandMutate mssi -> [|(%mssi.Mssi.Id |> UMX.tag<sorterId>,  modelTag.Single )|]
        | SmmMsrsRandMutate msrs -> [|(%msrs.Msrs.Id |> UMX.tag<sorterId>,  modelTag.Single )|]
        | SmmMsuf4RandMutate msuf4 -> [|(%msuf4.Msuf4.Id |> UMX.tag<sorterId>,  modelTag.Single )|]
        | SmmMsuf6RandMutate msuf6 -> [|(%msuf6.Msuf6.Id |> UMX.tag<sorterId>,  modelTag.Single )|]


    let getMutantSortingId (index: int) (model: sorterModelMutator) : Guid<sortingId> =
        match model with
        | SmmMsceRandMutate msce -> msce.getMutantSortingId index
        | SmmMssiRandMutate mssi -> mssi.getMutantSortingId index
        | SmmMsrsRandMutate msrs -> msrs.getMutantSortingId index
        | SmmMsuf4RandMutate msuf4 -> msuf4.getMutantSortingId index
        | SmmMsuf6RandMutate msuf6 -> msuf6.getMutantSortingId index


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


    let makeMutantSorterModelFromIndex 
                (index: int)  
                (model: sorterModelMutator) : sorterModel =
        match model with
        | SmmMsceRandMutate msce -> msce.MakeSorterModelFromIndex index |> sorterModel.Msce
        | SmmMssiRandMutate mssi -> mssi.MakeSorterModelFromIndex index |> sorterModel.Mssi
        | SmmMsrsRandMutate msrs -> msrs.MakeSorterModelFromIndex index |> sorterModel.Msrs
        | SmmMsuf4RandMutate msuf4 -> msuf4.MakeSorterModelFromIndex index |> sorterModel.Msuf4
        | SmmMsuf6RandMutate msuf6 -> msuf6.MakeSorterModelFromIndex index |> sorterModel.Msuf6



    let makeMutantSorterModelFromId 
                (sortingId: Guid<sortingId>)  
                (model: sorterModelMutator) : sorterModel =

        let sorterModelId = %sortingId |> UMX.tag<sorterModelId>
        match model with
        | SmmMsceRandMutate msce -> msce.MakeSorterModelFromId sorterModelId |> sorterModel.Msce
        | SmmMssiRandMutate mssi -> mssi.MakeSorterModelFromId sorterModelId |> sorterModel.Mssi
        | SmmMsrsRandMutate msrs -> msrs.MakeSorterModelFromId sorterModelId |> sorterModel.Msrs
        | SmmMsuf4RandMutate msuf4 -> msuf4.MakeSorterModelFromId sorterModelId |> sorterModel.Msuf4
        | SmmMsuf6RandMutate msuf6 -> msuf6.MakeSorterModelFromId sorterModelId |> sorterModel.Msuf6


    let makeMutantSorterIdWithTag (index:int) (model:sorterModelMutator) : Guid<sorterId> * modelTag =
        match model with
        | SmmMsceRandMutate msce -> (%(msce.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMssiRandMutate mssi -> (%(mssi.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsrsRandMutate msrs -> (%(msrs.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsuf4RandMutate msuf4 -> (%(msuf4.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)
        | SmmMsuf6RandMutate msuf6 -> (%(msuf6.MakeSorterModelId index) |> UMX.tag<sorterId>, modelTag.Single)





