
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
        let sorterKey = prefixLibId.create sortingWidth 4<stageLength> sorterVariant
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


    let makeMergeLib() =
        let sortingWidth = 64<sortingWidth>
        let mergeDimension = 4<mergeDimension>
        let variant = sorterLibVariant.VariantB
        let mergeKey = mergeLibId.create sortingWidth mergeDimension variant
        let ceArrayOpt = SorterDataParse.getCeArrayFromMergeLib mergeKey
        ceArrayOpt


    makeMergeLib() |> ignore

    printfn "Hello from F# SortingLib"
    Console.ReadLine() |> ignore
