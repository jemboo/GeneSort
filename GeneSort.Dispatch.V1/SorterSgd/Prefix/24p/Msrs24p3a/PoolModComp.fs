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

let makeQueryParams
        (dbName: string<databaseName>)
        (repl: int<replNumber>)
        (genCurrent: int<generationNumber>)
        (sorterCtPerPool: int<sorterCountPerPool>)
        (sorterPoolCt: int<sorterPoolCount>)
        (mdr: float<modificationRate>)
        (para: float<paraRate>)
        (selfSym: float<selfSymRate>)
        (ses:sorterSelectionType)
        (selSz:int<sorterCountPerPool>)
        (mmod: int<mutationMod>)
        (outDt: outputDataType) : queryParams =

    queryParams.create 
        dbName 
        projName
        (Some repl)
        (Some genCurrent)
        outDt
        [|
            (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
            (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
            (runParameters.seedSorterPoolSelectionTypeKey, ses |> SorterSelectionType.toString)
            (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
            (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
            (runParameters.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
            (runParameters.selectedSorterCountPerPoolKey, (Some selSz) |> SorterCountPerPool.toString)
            (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
        |]


let queryParamsFromRunParams 
                (dbName: string<databaseName>)
                (rp: runParameters) 
                (odt: outputDataType) : queryParams option =
    maybe {
        let! repl = rp.GetRepl()
        let! curGen = rp.GetGenerationCurrent()
        let! scPP = rp.GetSorterCountPerPool()
        let! spc = rp.GetSorterPoolCount()
        let! spsev = rp.GetSeedSorterPoolSelectionType()
        let! mmod = rp.GetMutationMod()
        let! mdr = rp.GetModificationRate()
        let! para = rp.GetParaRate()
        let! self = rp.GetSelfSymRate()
        let! selSz = rp.GetSelectedSorterCountPerPool()
        return makeQueryParams dbName repl curGen scPP spc mdr para self spsev selSz mmod odt
    }

//let private withLocalParams (rp:runParameters) =
//    let rpn = standardPoolSzParams rp
//    rpn.WithOrthoRate(Some 4.001<orthoRate>)
//       .WithParaRate(Some 0.4<paraRate>)
//       .WithSelfSymRate(Some 2.001<selfSymRate>)

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



do QueryParamsCatalog.register (QueryParamsCatalog.nameForDatabase %projName %dbNamePoolSz128) (queryParamsFromRunParams dbNamePoolSz128)
do QueryParamsCatalog.register (QueryParamsCatalog.nameForDatabase %projName %dbName_Sz_2048_Of_4096) (queryParamsFromRunParams dbName_Sz_2048_Of_4096)
do QueryParamsCatalog.register (QueryParamsCatalog.nameForDatabase %projName %dbNamePools4096_2_vs_256) (queryParamsFromRunParams dbNamePools4096_2_vs_256)
do QueryParamsCatalog.register (QueryParamsCatalog.nameForDatabase %projName %dbNamePools4096_4096) (queryParamsFromRunParams dbNamePools4096_4096)

let makeDatabase (dbName: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName dbName)


let createRunHost (spec: runHostSpec) : IRunHost =
    let db = makeDatabase spec.databaseName
    let run = run.create spec.databaseName projName spec.runName spec.runDescription spec.spans
    runHost.Create db spec run :> IRunHost


module Specs =

    let TestSpec (executorType: sorterSgdExecutorType)  : runHostSpec = {
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
