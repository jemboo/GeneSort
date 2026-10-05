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
let dbName_Sz_2048_Of_4096 = "Sz_2048_Of_4096" |> UMX.tag<databaseName>
let dbNamePools4096_2_vs_256 = "PoolsSelSzTest_2_vs_256" |> UMX.tag<databaseName>
let dbNamePools4096_4096 = "Pools4096_4096" |> UMX.tag<databaseName>
let dbNamePoolSz128 = "PoolSelSz128" |> UMX.tag<databaseName>




module Specs =

    let TestSpec (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-pool-mod-comp"
        databaseName = dbNamePoolSz128
        projectName = projName
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
        filterCatalogName = RunParamBuilderNames.Filter.identity
        enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


    let Sz_2048_Of_4096 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-pool-mod-comp"
        databaseName = dbName_Sz_2048_Of_4096
        projectName = projName
        runName = sprintf @"Sz_2048_Of_4096_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Selection size comp for 24pfx3a Msrs"
        spans = [
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [12] |> List.map string)
            (runParameters.sorterCountPerPoolKey, ["4096";])
            (runParameters.mutationModKey, [0 .. 7;] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [2048;] |> List.map string)
        ]
        filterCatalogName = RunParamBuilderNames.Filter.identity
        enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


    let PoolSz_2n256 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-pool-mod-comp"
        databaseName = dbNamePools4096_2_vs_256
        projectName = projName
        runName = sprintf @"PoolSz_2_vs_256_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Pool size comp (2 vs 256) for 24pfx3a Msrs"
        spans = [
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, ["2"; "256";])
            (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, ["64"; "128"; "256"] |> List.map string)
        ]
        filterCatalogName = RunParamBuilderNames.Filter.identity
        enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
        allowOverwrite = false |> UMX.tag
        maxParallel = 16
    }
