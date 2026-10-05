module GeneSort.Dispatch.V1.SorterSgd.Mssi24p3b.OrthoPara

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Mssi24p3b.Common
open GeneSort.Dispatch.V1.SorterSgd



let globalSorterCount = 1024 |> UMX.tag<sorterCount>
let dbOrthoPara64Name = "OrthoPara64" |> UMX.tag<databaseName>
let dbOrthoPara128Name = "OrthoPara128" |> UMX.tag<databaseName>

let private withLocalParams (rp:runParameters) =
    let rpn = standardParams rp
    rpn.WithOrthoRate(Some 1.001<orthoRate>)


let private paramMapFilter = RunParamFilterBuilders.identity

let private finishRunParams =
    RunParamEnhancerBuilders.Sgd.fromGlobalSorterCount
        withLocalParams globalSorterCount true None



do QueryParamsBuilders.registerAll ()
do QueryParamsBuilders.registerAll ()

let makeDatabase (dbName: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName dbName, "sorter-sgd.mssi-ortho-para")


let createRunHost (spec: runHostSpec) : runHost =
    let db = makeDatabase spec.databaseName
    let run = run.SgdRun (SgdRun.create spec.databaseName projName spec.runName spec.runDescription spec.spans "expInterval100_L50ss" "summaryInterval_C.1p5C" spec.queryCatalogName)
    runHost.Create db spec run


module Specs64 =

    let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.mssi-ortho-para"
        databaseName = dbOrthoPara64Name
        runName = sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
        spans = [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [1] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
            (runParameters.paraRateKey, [0.5; 1.01; 1.5; 2.01] |> List.map string)
            (runParameters.modificationRateKey, [ 0.09; 0.11; ] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [64;] |> List.map string)
        ]
        filterCatalogName = RunParamFilterBuilders.register (paramMapFilter)
        enhancerCatalogName = RunParamEnhancerBuilders.register (finishRunParams)
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


    let SymForceDiff1 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.mssi-ortho-para"
        databaseName = dbOrthoPara64Name
        runName = sprintf @"SymForce_Diff1%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
        spans = [
            (runParameters.codeModKey, ["SymForce_Diff1"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
            (runParameters.paraRateKey, [0.5; 1.01;] |> List.map string)
            (runParameters.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [64;] |> List.map string)
        ]
        filterCatalogName = RunParamFilterBuilders.register (paramMapFilter)
        enhancerCatalogName = RunParamEnhancerBuilders.register (finishRunParams)
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


    let SymForce (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.mssi-ortho-para"
        databaseName = dbOrthoPara64Name
        runName = sprintf @"SymForce%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
        spans = [
            (runParameters.codeModKey, ["SymForce"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
            (runParameters.paraRateKey, [0.5; 1.01;] |> List.map string)
            (runParameters.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [64;] |> List.map string)
        ]
        filterCatalogName = RunParamFilterBuilders.register (paramMapFilter)
        enhancerCatalogName = RunParamEnhancerBuilders.register (finishRunParams)
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


module Specs128 =

    let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.mssi-ortho-para"
        databaseName = dbOrthoPara128Name
        runName = sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
        spans = [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [6] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [128] |>  List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
            (runParameters.modificationRateKey, [0.09; 0.11;] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [128;] |> List.map string)
        ]
        filterCatalogName = RunParamFilterBuilders.register (paramMapFilter)
        enhancerCatalogName = RunParamEnhancerBuilders.register (finishRunParams)
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }
