
namespace GeneSort.SortingLib.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter

module Sandbox =
    let test32Pfx() =
        let channelCount = 28<channelCount>
        let sorterVariant = prefixLibVariant.PrefixA
        let sorterKey = prefixLibId.create channelCount 4<stageLength> sorterVariant
        let ceArray = (SorterDataParse.getCeArrayFromPrefixLib sorterKey).Value
        let res = SortableBoolArray.getAllPossibleResultsFromCeArray
                    ceArray
                    channelCount
                  |> Seq.toArray
        None




    let stackCeArrays () =
        let channelCount = 4<channelCount>
        let sorterVariant = sorterLibVariant.VariantA
        let sorterKey = sorterLibId.create channelCount sorterVariant
        let ceArray = (SorterDataParse.get2dCeArrayFromSorterLib sorterKey).Value

        let stackedCes = Ce.stack2d ceArray ceArray channelCount
        stackedCes


    let makeMergeLib() =
        let channelCount = 64<channelCount>
        let mergeDimension = 4<mergeDimension>
        let variant = sorterLibVariant.VariantB
        let mergeKey = mergeLibId.create channelCount mergeDimension variant
        let ceArrayOpt = SorterDataParse.getCeArrayFromMergeLib mergeKey
        ceArrayOpt


    makeMergeLib() |> ignore

    printfn "Hello from F# SortingLib"
    Console.ReadLine() |> ignore
