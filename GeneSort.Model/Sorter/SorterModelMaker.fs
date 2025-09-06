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


type SorterModelMaker =
     | SmmMsceRandGen of MsceRandGen
     | SmmMsceRandMutate of MsceRandMutate
     | SmmMssiRandGen of MssiRandGen
     | SmmMssiRandMutate of MssiRandMutate
     | SmmMsrsRandGen of MsrsRandGen
     | SmmMsrsRandMutate of MsrsRandMutate
     | SmmMsuf4RandGen of Msuf4RandGen
     | SmmMsuf4RandMutate of Msuf4RandMutate
     | SmmMsuf6RandGen of Msuf6RandGen
     | SmmMsuf6RandMutate of Msuf6RandMutate


module SorterModelMaker =

    let makeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int)  (model: SorterModelMaker) : SorterModel =
        match model with
        | SmmMsceRandGen msce -> msce.MakeSorterModel rngFactory index |> SorterModel.Msce
        | SmmMsceRandMutate msce -> msce.MakeSorterModel rngFactory index |> SorterModel.Msce
        | SmmMssiRandGen mssi -> mssi.MakeSorterModel rngFactory index |> SorterModel.Mssi
        | SmmMssiRandMutate mssi -> mssi.MakeSorterModel rngFactory index |> SorterModel.Mssi
        | SmmMsrsRandGen msrs -> msrs.MakeSorterModel rngFactory index |> SorterModel.Msrs
        | SmmMsrsRandMutate msrs -> msrs.MakeSorterModel rngFactory index |> SorterModel.Msrs
        | SmmMsuf4RandGen msuf4 -> msuf4.MakeSorterModel rngFactory index |> SorterModel.Msuf4
        | SmmMsuf4RandMutate msuf4 -> msuf4.MakeSorterModel rngFactory index |> SorterModel.Msuf4
        | SmmMsuf6RandGen msuf6 -> msuf6.MakeSorterModel rngFactory index |> SorterModel.Msuf6
        | SmmMsuf6RandMutate msuf6 -> msuf6.MakeSorterModel rngFactory index |> SorterModel.Msuf6

    let getCeLength (model: SorterModelMaker) : int<ceLength> =
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


