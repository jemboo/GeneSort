
namespace GeneSort.SortingLib.Sorter

open FSharp.UMX
open System
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

module Sandbox =
    let test32Pfx() =
        let sortingWidth = 28<sortingWidth>
        let sorterVariant = sorterVariant.Prefix4
        let sorterKey = SorterLibId.create sortingWidth sorterVariant
        let ceArray = (SorterDataParse.getCeArrayFromLib sorterKey).Value
        let res = SortableBoolArray.getAllPossibleResultsFromCeArray
                    ceArray
                    sortingWidth
                  |> Seq.toArray
        None











    test32Pfx() |> ignore

    printfn "Hello from F# SortingLib"
    Console.ReadLine() |> ignore
