module GeneSort.Dispatch.V1.SorterSgd.Msrs24p3a.OrthoPara

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Msrs24p3a.Common
open GeneSort.Dispatch.V1.SorterSgd
let dbOrthoPara32Name = "OrthoPara32" |> UMX.tag<databaseName>



do QueryParamsBuilders.registerAll ()




module Specs =

    let PickMode2_2 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-ortho-para"
        databaseName = dbOrthoPara32Name
        projectName = projName
        runName = sprintf @"PickMode2_2_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3a Msrs, PickMode2_2"
        spans = [
            (runParameters.codeModKey, ["PickMode2_2"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
            (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
        ]
        filterCatalogName = RunParamBuilderNames.Filter.identity
        enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs24p3aOrthoPara
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }

    let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-ortho-para"
        databaseName = dbOrthoPara32Name
        projectName = projName
        runName = sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3a Msrs, NoMods"
        spans = [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [7] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
            (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
        ]
        filterCatalogName = RunParamBuilderNames.Filter.identity
        enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs24p3aOrthoPara
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }
