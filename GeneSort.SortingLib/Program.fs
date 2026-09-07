
namespace GeneSort.SortingLib.Sorter

open FSharp.UMX
open System
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter

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




    let stackCeArrays () =
        let sortingWidth = 4<sortingWidth>
        let sorterVariant = sorterLibVariant.VariantA
        let sorterKey = sorterLibId.create sortingWidth sorterVariant
        let ceArray = (SorterDataParse.get2dCeArrayFromSorterLib sorterKey).Value

        let stackedCes = Ce.stack2d ceArray ceArray sortingWidth
        stackedCes






    stackCeArrays() |> ignore

    printfn "Hello from F# SortingLib"
    Console.ReadLine() |> ignore
