namespace GeneSort.Dispatch.V1.SorterSgd.Msrs24p3a

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


module PoolSelSzComp =

    let globalSorterCount = 4096 |> UMX.tag<sorterCount>
    let dbNamePoolsTest = "PoolsSelSzTest" |> UMX.tag<databaseName>
    let dbName_Sz_2048_Of_4096 = "Sz_2048_Of_4096" |> UMX.tag<databaseName>
    let dbNamePools4096_2_vs_256 = "PoolsSelSzTest_2_vs_256" |> UMX.tag<databaseName>
    let dbNamePools4096_4096 = "Pools4096_4096" |> UMX.tag<databaseName>
    let dbNamePoolSz128 = "PoolSelSz128" |> UMX.tag<databaseName>
    let dbFolderTest = @$"c:\Projects\{%projName}\{%dbNamePoolsTest}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolderPools4096 = @$"c:\Projects\{%projName}\{%dbName_Sz_2048_Of_4096}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolderPools4098b = @$"c:\Projects\{%projName}\{%dbNamePools4096_2_vs_256}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolderPools4098c = @$"c:\Projects\{%projName}\{%dbNamePools4096_4096}\Data" |> UMX.tag<pathToRootFolder>

    let makeQueryParams
            (repl: int<replNumber>)
            (genCurrent: int<generationNumber>)
            (sorterCtPerPool: int<sorterCountPerPool>)
            (sorterPoolCt: int<sorterPoolCount>)
            (ses:sorterSelectionType)
            (selSz:int<sorterCountPerPool>)
            (mmod: int<mutationMod>)
            (outDt: outputDataType) : queryParams =

        queryParams.create 
            dbNamePoolsTest 
            projName
            (Some repl)
            (Some genCurrent)
            outDt
            [|
                (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                (runParameters.seedSorterPoolSelectionTypeKey, ses |> SorterEvalSelectionType.toString)
                (runParameters.selectedSorterCountPerPoolKey, (Some selSz) |> SorterCountPerPool.toString)
                (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
            |]


    let queryParamsFromRunParams 
                    (rp: runParameters) 
                    (odt: outputDataType) : queryParams option =
        maybe {
            let! repl = rp.GetRepl()
            let! curGen = rp.GetGenerationCurrent()
            let! scPP = rp.GetSorterCountPerPool()
            let! spc = rp.GetSorterPoolCount()
            let! spsev = rp.GetSeedSorterPoolSelectionType()
            let! mmod = rp.GetMutationMod()
            let! selSz = rp.GetSelectedSorterCountPerPool()
            return makeQueryParams repl curGen scPP spc spsev selSz mmod odt
        }

    let private withLocalParams (rp:runParameters) =
        let rpn = standardPoolSzParams rp
        rpn.WithModificationRate(Some 0.06<modificationRate>)
            .WithOrthoRate(Some 4.001<orthoRate>)
            .WithParaRate(Some 0.4<paraRate>)
            .WithSelfSymRate(Some 2.001<selfSymRate>)

    let private paramMapFilter (rp: runParameters) =
        Some rp

    let private finishRunParams (host: IRunHost) (rp:runParameters) =
        let rp2 = withLocalParams rp
        let scpp = rp.GetSorterCountPerPool().Value
        let spc = (%globalSorterCount / %scpp) |> UMX.tag<sorterPoolCount> |> Option.Some
        let rp3 = rp2.WithSorterPoolCount(spc)
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

        rp3.WithRunFinished(Some false)
                .WithId(Some qp.Value.Id)
                .WithRunName(Some host.Run.RunName)


    let saveIntervals = SampleRegistry.samplingConfigsDict["expInterval100_L50ss"]
    let saveSubIntervals = SampleRegistry.samplingConfigsDict["summaryInterval_C.1p5C"]

    let dbTest = new GeneSortGenDbMp(dbFolderTest, queryParamsFromRunParams, saveIntervals, saveSubIntervals)
    let dbPools4096 = new GeneSortGenDbMp(dbFolderPools4096, queryParamsFromRunParams, saveIntervals, saveSubIntervals)
    let dbPools4098b = new GeneSortGenDbMp(dbFolderPools4098b, queryParamsFromRunParams, saveIntervals, saveSubIntervals)
    let dbPools4098c = new GeneSortGenDbMp(dbFolderPools4098c, queryParamsFromRunParams, saveIntervals, saveSubIntervals)


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (dbNamePoolsTest, dbTest :> IGeneSortDb);
            (dbName_Sz_2048_Of_4096, dbPools4096 :> IGeneSortDb);
            (dbNamePools4096_2_vs_256, dbPools4098b :> IGeneSortDb);
            (dbNamePools4096_4096, dbPools4098c :> IGeneSortDb);
        ]
        |> Map.ofList

    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)


    let createRunHost (spec: runHostSpec) : IRunHost =
        let db = getDatabaseByName spec.databaseName
        let run = run.create spec.databaseName projName spec.runName spec.runDescription
        runHost.Create db spec run :> IRunHost


    module Specs =

        let TestSpec (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePoolsTest
            runName = sprintf @"PoolSz_Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Selection size comp for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [1] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["256"])
                (runParameters.mutationModKey, [3;] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, ["64"; "128"; "256"] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
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
                (runParameters.selectedSorterCountPerPoolKey, ["2048";] |> List.map string)
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
