namespace GeneSort.Model.Sorter

open System
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Uf6
open GeneSort.Core
open FSharp.UMX


type SorterModelMaker =
     | MsceRandGen of MsceRandGen
     | MsceRandMutate of MsceRandMutate
     | MssiRandGen of MssiRandGen
     | MssiRandMutate of MssiRandMutate
     | MsrsRandGen of MsrsRandGen
     | MsrsRandMutate of MsrsRandMutate
     | Msuf4RandGen of Msuf4RandGen
     | Msuf4RandMutate of Msuf4RandMutate
     | Msuf6RandGen of Msuf6RandGen
     | Msuf6RandMutate of Msuf6RandMutate


module SorterModelMaker =
    let makeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int)  (model: SorterModelMaker) : SorterModel =
        match model with
        | MsceRandGen msce -> msce.MakeSorterModel rngFactory index |> SorterModel.Msce
        | MsceRandMutate msce -> msce.MakeSorterModel rngFactory index |> SorterModel.Msce
        | MssiRandGen mssi -> mssi.MakeSorterModel rngFactory index |> SorterModel.Mssi
        | MssiRandMutate mssi -> mssi.MakeSorterModel rngFactory index |> SorterModel.Mssi
        | MsrsRandGen msrs -> msrs.MakeSorterModel rngFactory index |> SorterModel.Msrs
        | MsrsRandMutate msrs -> msrs.MakeSorterModel rngFactory index |> SorterModel.Msrs
        | Msuf4RandGen msuf4 -> msuf4.MakeSorterModel rngFactory index |> SorterModel.Msuf4
        | Msuf4RandMutate msuf4 -> msuf4.MakeSorterModel rngFactory index |> SorterModel.Msuf4
        | Msuf6RandGen msuf6 -> msuf6.MakeSorterModel rngFactory index |> SorterModel.Msuf6
        | Msuf6RandMutate msuf6 -> msuf6.MakeSorterModel rngFactory index |> SorterModel.Msuf6


