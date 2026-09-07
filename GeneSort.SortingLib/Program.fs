
namespace GeneSort.SortingLib.Sorter

open FSharp.UMX
open System
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

module Sandbox =
    let test32Pfx() =
        let sortingWidth = 28<sortingWidth>
        let sorterVariant = prefixLibVariant.PrefixA
        let sorterKey = PrefixLibId.create sortingWidth 4<stageLength> sorterVariant
        let ceArray = (SorterDataParse.getCeArrayFromPrefixLib sorterKey).Value
        let res = SortableBoolArray.getAllPossibleResultsFromCeArray
                    ceArray
                    sortingWidth
                  |> Seq.toArray
        None











    test32Pfx() |> ignore

    printfn "Hello from F# SortingLib"
    Console.ReadLine() |> ignore
