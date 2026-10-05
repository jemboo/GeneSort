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



let globalSorterCount = 8192 |> UMX.tag<sorterCount>
let dbOrthoPara32Name = "OrthoPara32" |> UMX.tag<databaseName>


let private withLocalParams (rp:runParameters) =
    let rpn = standardParams rp
    rpn.WithOrthoRate(Some 4.001<orthoRate>)


do QueryParamsBuilders.registerAll ()

let makeDatabase (name: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName name, "sorter-sgd.msrs-ortho-para")


let createRunHost (spec: runHostSpec) : runHost =
    let db = makeDatabase spec.databaseName
    let run = run.SgdRun (SgdRun.create spec.databaseName projName spec.runName spec.runDescription spec.spans "expInterval100_L50ss" "summaryInterval_C.1p5C" spec.queryCatalogName)
    runHost.Create db spec run


module Specs =

    let PickMode2_2 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-ortho-para"
        databaseName = dbOrthoPara32Name
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
        filterCatalogName = RunParamFilterBuilders.register RunParamFilterBuilders.identity
        enhancerCatalogName =
            RunParamEnhancerBuilders.register (
                RunParamEnhancerBuilders.Sgd.fromGlobalSorterCount
                    withLocalParams globalSorterCount true (Some 0.99<modificationRate>))
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }

    let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-ortho-para"
        databaseName = dbOrthoPara32Name
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
        filterCatalogName = RunParamFilterBuilders.register RunParamFilterBuilders.identity
        enhancerCatalogName =
            RunParamEnhancerBuilders.register (
                RunParamEnhancerBuilders.Sgd.fromGlobalSorterCount
                    withLocalParams globalSorterCount true (Some 0.99<modificationRate>))
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }
