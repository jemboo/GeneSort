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
     | SmmMsceRandMutate of msceRandMutate
     | SmmMssiRandGen of mssiRandGen
     | SmmMssiRandMutate of mssiRandMutate
     | SmmMsrsRandGen of msrsRandGen
     | SmmMsrsRandMutate of msrsRandMutate
     | SmmMsuf4RandGen of msuf4RandGen
     | SmmMsuf4RandMutate of msuf4RandMutate
     | SmmMsuf6RandGen of msuf6RandGen
     | SmmMsuf6RandMutate of msuf6RandMutate


module SorterModelMaker =

    let getId (model:sorterModelMaker) : Guid<sorterModelMakerID> =
        match model with
        | SmmMsceRandGen msce -> msce.Id
        | SmmMsceRandMutate msce -> msce.Id
        | SmmMssiRandGen mssi -> mssi.Id
        | SmmMssiRandMutate mssi -> mssi.Id
        | SmmMsrsRandGen msrs -> msrs.Id
        | SmmMsrsRandMutate msrs -> msrs.Id
        | SmmMsuf4RandGen msuf4 -> msuf4.Id
        | SmmMsuf4RandMutate msuf4 -> msuf4.Id
        | SmmMsuf6RandGen msuf6 -> msuf6.Id
        | SmmMsuf6RandMutate msuf6 -> msuf6.Id


    let getSortingWidth (model: sorterModelMaker) : int<sortingWidth> =
        match model with
        | SmmMsceRandGen msce -> msce.SortingWidth
        | SmmMsceRandMutate msce -> msce.SortingWidth
        | SmmMssiRandGen mssi -> mssi.SortingWidth
        | SmmMssiRandMutate mssi -> mssi.SortingWidth
        | SmmMsrsRandGen msrs -> msrs.SortingWidth
        | SmmMsrsRandMutate msrs -> msrs.SortingWidth
        | SmmMsuf4RandGen msuf4 -> msuf4.SortingWidth
        | SmmMsuf4RandMutate msuf4 -> msuf4.SortingWidth
        | SmmMsuf6RandGen msuf6 -> msuf6.SortingWidth
        | SmmMsuf6RandMutate msuf6 -> msuf6.SortingWidth

    let getCeLength (model: sorterModelMaker) : int<ceLength> =
        match model with
        | SmmMsceRandGen msce -> msce.CeLength
        | SmmMsceRandMutate msce -> msce.CeLength
        | SmmMssiRandGen mssi -> mssi.CeLength
        | SmmMssiRandMutate mssi -> mssi.CeLength
        | SmmMsrsRandGen msrs -> msrs.CeLength
        | SmmMsrsRandMutate msrs -> msrs.CeLength
        | SmmMsuf4RandGen msuf4 -> msuf4.CeLength
        | SmmMsuf4RandMutate msuf4 -> msuf4.CeLength
        | SmmMsuf6RandGen msuf6 -> msuf6.CeLength
        | SmmMsuf6RandMutate msuf6 -> msuf6.CeLength


    let getSorterModelMakerId (model: sorterModelMaker) : Guid<sorterModelMakerID> =
        match model with
        | SmmMsceRandGen msce -> msce.Id
        | SmmMsceRandMutate msce -> msce.Id
        | SmmMssiRandGen mssi -> mssi.Id
        | SmmMssiRandMutate mssi -> mssi.Id
        | SmmMsrsRandGen msrs -> msrs.Id
        | SmmMsrsRandMutate msrs -> msrs.Id
        | SmmMsuf4RandGen msuf4 -> msuf4.Id
        | SmmMsuf4RandMutate msuf4 -> msuf4.Id
        | SmmMsuf6RandGen msuf6 -> msuf6.Id
        | SmmMsuf6RandMutate msuf6 -> msuf6.Id

    let makeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int)  (model: sorterModelMaker) : sorterModel =
        match model with
        | SmmMsceRandGen msce -> msce.MakeSorterModel rngFactory index |> sorterModel.Msce
        | SmmMsceRandMutate msce -> msce.MakeSorterModel rngFactory index |> sorterModel.Msce
        | SmmMssiRandGen mssi -> mssi.MakeSorterModel rngFactory index |> sorterModel.Mssi
        | SmmMssiRandMutate mssi -> mssi.MakeSorterModel rngFactory index |> sorterModel.Mssi
        | SmmMsrsRandGen msrs -> msrs.MakeSorterModel rngFactory index |> sorterModel.Msrs
        | SmmMsrsRandMutate msrs -> msrs.MakeSorterModel rngFactory index |> sorterModel.Msrs
        | SmmMsuf4RandGen msuf4 -> msuf4.MakeSorterModel rngFactory index |> sorterModel.Msuf4
        | SmmMsuf4RandMutate msuf4 -> msuf4.MakeSorterModel rngFactory index |> sorterModel.Msuf4
        | SmmMsuf6RandGen msuf6 -> msuf6.MakeSorterModel rngFactory index |> sorterModel.Msuf6
        | SmmMsuf6RandMutate msuf6 -> msuf6.MakeSorterModel rngFactory index |> sorterModel.Msuf6



