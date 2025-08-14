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

    let toISorterModelMaker (smm: SorterModelMaker) : ISorterModelMaker =
        match smm with
        | MsceRandGen m -> m :> ISorterModelMaker
        | MsceRandMutate m -> m :> ISorterModelMaker
        | MssiRandGen m -> m :> ISorterModelMaker
        | MssiRandMutate m -> m :> ISorterModelMaker
        | MsrsRandGen m -> m :> ISorterModelMaker
        | MsrsRandMutate m -> m :> ISorterModelMaker
        | Msuf4RandGen m -> m :> ISorterModelMaker
        | Msuf4RandMutate m -> m :> ISorterModelMaker
        | Msuf6RandGen m -> m :> ISorterModelMaker
        | Msuf6RandMutate m -> m :> ISorterModelMaker

    let makeSorterModel (randoGen: rngType -> Guid -> IRando) (index: int) (smm: SorterModelMaker) : ISorterModel =
        (smm |> toISorterModelMaker).MakeSorterModel randoGen index

