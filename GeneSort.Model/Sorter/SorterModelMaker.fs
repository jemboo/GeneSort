namespace GeneSort.Model.Sorter

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Core
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Uf6


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

