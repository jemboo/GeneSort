module GeneSort.Dispatch.V1.SorterSgd.Msrs24p3a.PoolModComp

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



let globalSorterCount = 512 |> UMX.tag<sorterCount>
let dbName_Sz_2048_Of_4096 = "Sz_2048_Of_4096" |> UMX.tag<databaseName>
let dbNamePools4096_2_vs_256 = "PoolsSelSzTest_2_vs_256" |> UMX.tag<databaseName>
let dbNamePools4096_4096 = "Pools4096_4096" |> UMX.tag<databaseName>
let dbNamePoolSz128 = "PoolSelSz128" |> UMX.tag<databaseName>

let private withLocalParams (rp:runParameters) =
    let rpn = standardParams rp
    rpn.WithOrthoRate(Some 4.001<orthoRate>)


let private paramMapFilter (rp: runParameters) =
    Some rp

let private finishRunParams (host: IRunHost) (rp:runParameters) =
    let rp2 = withLocalParams rp
    let scpp = rp.GetSorterCountPerPool().Value
    let spc = (%globalSorterCount / %scpp) |> UMX.tag<sorterPoolCount> |> Option.Some
    let rp3 = rp2.WithSorterPoolCount(spc)
    let qp = host.QueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

    rp3.WithRunFinished(Some false)
            .WithId(Some qp.Value.Id)
            .WithRunName(Some host.Run.RunName)

do QueryParamsBuilders.registerAll ()

let makeDatabase (dbName: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName dbName, "sorter-sgd.msrs-pool-mod-comp")


let createRunHost (spec: runHostSpec) : IRunHost =
    let db = makeDatabase spec.databaseName
    let run = run.createWithCatalogName spec.databaseName projName spec.runName spec.runDescription spec.spans spec.queryCatalogName
    runHost.Create db spec run :> IRunHost


module Specs =

    let TestSpec (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-pool-mod-comp"
        databaseName = dbNamePoolSz128
        runName = sprintf @"PoolSz32_Mod_Testc%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Para rate comp for 24pfx3a Msrs"
        spans = [
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [8] |> List.map string)
            (runParameters.sorterCountPerPoolKey, ["32"] |> List.map string)
            (runParameters.modificationRateKey, [0.99;] |> List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125;] |> List.map string)
            (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
            (runParameters.mutationModKey, [0 .. 1] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, ["32";] |> List.map string)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


    let Sz_2048_Of_4096 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-pool-mod-comp"
        databaseName = dbName_Sz_2048_Of_4096
        runName = sprintf @"Sz_2048_Of_4096_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Selection size comp for 24pfx3a Msrs"
        spans = [
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [12] |> List.map string)
            (runParameters.sorterCountPerPoolKey, ["4096";])
            (runParameters.mutationModKey, [0 .. 7;] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [2048;] |> List.map string)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


    let PoolSz_2n256 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-pool-mod-comp"
        databaseName = dbNamePools4096_2_vs_256
        runName = sprintf @"PoolSz_2_vs_256_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Pool size comp (2 vs 256) for 24pfx3a Msrs"
        spans = [
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, ["2"; "256";])
            (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, ["64"; "128"; "256"] |> List.map string)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 16
    }
