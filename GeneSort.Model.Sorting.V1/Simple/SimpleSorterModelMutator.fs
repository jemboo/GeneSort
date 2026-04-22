namespace GeneSort.Model.Sorting.Simple.V1

open FSharp.UMX
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Model.Sorting.V1.Simple.Uf6
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple


type simpleSorterModelMutator =
     | SmmMsceRandMutate of msceRandMutate
     | SmmMssiRandMutate of mssiRandMutate
     | SmmMsrsRandMutate of msrsRandMutate
     | SmmMsuf4RandMutate of msuf4RandMutate
     | SmmMsuf6RandMutate of msuf6RandMutate


module SorterModelMutator =

    let getId (model:simpleSorterModelMutator) : Guid<sorterModelMutatorId> =
        match model with
        | SmmMsceRandMutate msce -> msce.Id
        | SmmMssiRandMutate mssi -> mssi.Id
        | SmmMsrsRandMutate msrs -> msrs.Id
        | SmmMsuf4RandMutate msuf4 -> msuf4.Id
        | SmmMsuf6RandMutate msuf6 -> msuf6.Id


    let getParentSorterModel (model:simpleSorterModelMutator) : simpleSorterModel =
        match model with
        | SmmMsceRandMutate msce -> msce.Msce |> simpleSorterModel.Msce
        | SmmMssiRandMutate mssi -> mssi.Mssi |> simpleSorterModel.Mssi
        | SmmMsrsRandMutate msrs -> msrs.Msrs |> simpleSorterModel.Msrs
        | SmmMsuf4RandMutate msuf4 -> msuf4.Msuf4 |> simpleSorterModel.Msuf4
        | SmmMsuf6RandMutate msuf6 -> msuf6.Msuf6 |> simpleSorterModel.Msuf6


    let getParentSorterModelId (model: simpleSorterModelMutator) : Guid<sorterModelId> =
        match model with
        | SmmMsceRandMutate msce -> msce.Msce.Id
        | SmmMssiRandMutate mssi -> mssi.Mssi.Id
        | SmmMsrsRandMutate msrs -> msrs.Msrs.Id
        | SmmMsuf4RandMutate msuf4 -> msuf4.Msuf4.Id
        | SmmMsuf6RandMutate msuf6 -> msuf6.Msuf6.Id


    //let getMutantSortingId (index: int) (model: simpleSorterModelMutator) : Guid<sortingId> =
    //    match model with
    //    | SmmMsceRandMutate msce -> msce.getMutantSortingId index
    //    | SmmMssiRandMutate mssi -> mssi.getMutantSortingId index
    //    | SmmMsrsRandMutate msrs -> msrs.getMutantSortingId index
    //    | SmmMsuf4RandMutate msuf4 -> msuf4.getMutantSortingId index
    //    | SmmMsuf6RandMutate msuf6 -> msuf6.getMutantSortingId index


    let getSortingWidth (model: simpleSorterModelMutator) : int<sortingWidth> =
        match model with
        | SmmMsceRandMutate msce -> msce.SortingWidth
        | SmmMssiRandMutate mssi -> mssi.SortingWidth
        | SmmMsrsRandMutate msrs -> msrs.SortingWidth
        | SmmMsuf4RandMutate msuf4 -> msuf4.SortingWidth
        | SmmMsuf6RandMutate msuf6 -> msuf6.SortingWidth


    let getCeLength (model: simpleSorterModelMutator) : int<ceLength> =
        match model with
        | SmmMsceRandMutate msce -> msce.CeLength
        | SmmMssiRandMutate mssi -> mssi.CeLength
        | SmmMsrsRandMutate msrs -> msrs.CeLength
        | SmmMsuf4RandMutate msuf4 -> msuf4.CeLength
        | SmmMsuf6RandMutate msuf6 -> msuf6.CeLength


    let makeMutantSorterModelFromIndex 
                (index: int)  
                (model: simpleSorterModelMutator) : simpleSorterModel =
        match model with
        | SmmMsceRandMutate msce -> msce.MakeSorterModelFromIndex index |> simpleSorterModel.Msce
        | SmmMssiRandMutate mssi -> mssi.MakeSorterModelFromIndex index |> simpleSorterModel.Mssi
        | SmmMsrsRandMutate msrs -> msrs.MakeSorterModelFromIndex index |> simpleSorterModel.Msrs
        | SmmMsuf4RandMutate msuf4 -> msuf4.MakeSorterModelFromIndex index |> simpleSorterModel.Msuf4
        | SmmMsuf6RandMutate msuf6 -> msuf6.MakeSorterModelFromIndex index |> simpleSorterModel.Msuf6


    let makeMutantSorterModelFromId 
                (simpleSorterModelId: Guid<sorterModelId>)  
                (model: simpleSorterModelMutator) : simpleSorterModel =
        match model with
        | SmmMsceRandMutate msce -> msce.MakeSorterModelFromId simpleSorterModelId |> simpleSorterModel.Msce
        | SmmMssiRandMutate mssi -> mssi.MakeSorterModelFromId simpleSorterModelId |> simpleSorterModel.Mssi
        | SmmMsrsRandMutate msrs -> msrs.MakeSorterModelFromId simpleSorterModelId |> simpleSorterModel.Msrs
        | SmmMsuf4RandMutate msuf4 -> msuf4.MakeSorterModelFromId simpleSorterModelId |> simpleSorterModel.Msuf4
        | SmmMsuf6RandMutate msuf6 -> msuf6.MakeSorterModelFromId simpleSorterModelId |> simpleSorterModel.Msuf6







