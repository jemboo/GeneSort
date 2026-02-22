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

    let getId (model:sorterModelMutator) : Guid<sorterModelMutatorID> =
        match model with
        | SmmMsceRandMutate msce -> msce.Id
        | SmmMssiRandMutate mssi -> mssi.Id
        | SmmMsrsRandMutate msrs -> msrs.Id
        | SmmMsuf4RandMutate msuf4 -> msuf4.Id
        | SmmMsuf6RandMutate msuf6 -> msuf6.Id


    let getSortingModelSeedId (model: sorterModelMutator) : Guid<sortingModelID> =
        match model with
        | SmmMsceRandMutate msce -> %msce.Msce.Id |> UMX.tag<sortingModelID>
        | SmmMssiRandMutate mssi -> %mssi.Mssi.Id |> UMX.tag<sortingModelID>
        | SmmMsrsRandMutate msrs -> %msrs.Msrs.Id |> UMX.tag<sortingModelID>
        | SmmMsuf4RandMutate msuf4 -> %msuf4.Msuf4.Id |> UMX.tag<sortingModelID>
        | SmmMsuf6RandMutate msuf6 -> %msuf6.Msuf6.Id |> UMX.tag<sortingModelID>


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
                (rngFactory: rngType -> Guid -> IRando) 
                (index: int)  
                (model: sorterModelMutator) : sorterModel =
        match model with
        | SmmMsceRandMutate msce -> msce.MakeSorterModel rngFactory index |> sorterModel.Msce
        | SmmMssiRandMutate mssi -> mssi.MakeSorterModel rngFactory index |> sorterModel.Mssi
        | SmmMsrsRandMutate msrs -> msrs.MakeSorterModel rngFactory index |> sorterModel.Msrs
        | SmmMsuf4RandMutate msuf4 -> msuf4.MakeSorterModel rngFactory index |> sorterModel.Msuf4
        | SmmMsuf6RandMutate msuf6 -> msuf6.MakeSorterModel rngFactory index |> sorterModel.Msuf6



