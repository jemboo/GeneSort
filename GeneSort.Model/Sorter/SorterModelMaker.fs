namespace GeneSort.Model.Sorter

open System
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Uf6
open GeneSort.Core
open FSharp.UMX
open GeneSort.Sorter


type sorterModelMaker =
     | SmmMsceRandGen of MsceRandGen
     | SmmMsceRandMutate of MsceRandMutate
     | SmmMssiRandGen of MssiRandGen
     | SmmMssiRandMutate of MssiRandMutate
     | SmmMsrsRandGen of msrsRandGen
     | SmmMsrsRandMutate of msrsRandMutate
     | SmmMsuf4RandGen of msuf4RandGen
     | SmmMsuf4RandMutate of msuf4RandMutate
     | SmmMsuf6RandGen of msuf6RandGen
     | SmmMsuf6RandMutate of msuf6RandMutate


module SorterModelMaker =

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


