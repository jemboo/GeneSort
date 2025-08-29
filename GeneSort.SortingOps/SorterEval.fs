namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open System.Linq
open System.Collections.Generic







module SorterEval =

    let refineSorter 
            (sorter: sorter)
            (sorterTests: sorterTests) =

        let ceBlockEval = CeOps.evalWithSorterTests sorterTests (ceBlock.create(sorter.Ces))
        ()


