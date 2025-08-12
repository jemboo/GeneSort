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


type SorterModel =
     | Msce of Msce
     | Mssi of Mssi
     | Msrs of Msrs
     | Msuf4 of Msuf4
     | Msuf6 of Msuf6

