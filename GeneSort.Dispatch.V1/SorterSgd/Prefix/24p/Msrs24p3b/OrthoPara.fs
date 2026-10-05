module GeneSort.Dispatch.V1.SorterSgd.Msrs24p3b.OrthoPara

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Msrs24p3b.Common
open GeneSort.Dispatch.V1.SorterSgd



let globalSorterCount = 8192 |> UMX.tag<sorterCount>
let dbOrthoPara32Name = "OrthoPara32" |> UMX.tag<databaseName>


let private withLocalParams (rp:runParameters) =
    let rpn = standardParams rp
    rpn.WithOrthoRate(Some 4.001<orthoRate>)


let private paramMapFilter = RunParamFilterBuilders.identity

let private finishRunParams =
    RunParamEnhancerBuilders.Sgd.fromGlobalSorterCount
        withLocalParams globalSorterCount true (Some 0.99<modificationRate>)



do QueryParamsBuilders.registerAll ()

let makeDatabase (dbName: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName dbName, "sorter-sgd.msrs-ortho-para")


let createRunHost (spec: runHostSpec) : runHost =
    let db = makeDatabase spec.databaseName
    let run = run.SgdRun (SgdRun.create spec.databaseName projName spec.runName spec.runDescription spec.spans "expInterval100_L50ss" "summaryInterval_C.1p5C" spec.queryCatalogName)
    runHost.Create db spec run


module Specs =

    let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-ortho-para"
        databaseName = dbOrthoPara32Name
        runName = sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Msrs, NoMods"
        spans = [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
            (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
        ]
        filterCatalogName = RunParamFilterBuilders.register (paramMapFilter)
        enhancerCatalogName = RunParamEnhancerBuilders.register (finishRunParams)
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }
