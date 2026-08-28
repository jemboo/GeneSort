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


module StageCrossings =

    let globalSorterCount = 256 |> UMX.tag<sorterCount>
    let dbNamePoolsTest = "PoolsTest" |> UMX.tag<databaseName>
    let dbNamePools4096_1_vs_512 = "Pools4098_1_vs_512" |> UMX.tag<databaseName>
    let dbNamePools4096_2_vs_256 = "Pools4098_2_vs_256" |> UMX.tag<databaseName>
    let dbNamePools4096_4096 = "Pools4096_4096" |> UMX.tag<databaseName>
    let dbNamePoolSz128 = "PoolSz128" |> UMX.tag<databaseName>
    let dbFolderTest = @$"c:\Projects\{%projName}\{%dbNamePoolsTest}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolderPools4098a = @$"c:\Projects\{%projName}\{%dbNamePools4096_1_vs_512}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolderPools4098b = @$"c:\Projects\{%projName}\{%dbNamePools4096_2_vs_256}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolderPools4098c = @$"c:\Projects\{%projName}\{%dbNamePools4096_4096}\Data" |> UMX.tag<pathToRootFolder>

    let makeQueryParams
            (repl: int<replNumber>)
            (genCurrent: int<generationNumber>)
            (sorterCtPerPool: int<sorterCountPerPool>)
            (sorterPoolCt: int<sorterPoolCount>)
            (ses:sorterEvalSelectionType)
            (mmod: int<mutationMod>)
            (outDt: outputDataType) : queryParams =

        match outDt with
        | outputDataType.RunParameters _ ->
            queryParams.create 
                dbNamePoolsTest 
                projName
                (Some repl)
                outDt
                [|
                    (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                    (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                    (runParameters.seedPoolSorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                    (runParameters.mutationModKey, (Some %mmod) |> string)
                |]
        | _ ->
            queryParams.create 
                dbNamePoolsTest 
                projName
                (Some repl)
                outDt
                [| 
                    (runParameters.generationCurrentKey, (Some genCurrent) |> GenerationNumber.toString)
                    (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                    (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                    (runParameters.seedPoolSorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                    (runParameters.mutationModKey, (Some %mmod) |> string)
                |]


    let queryParamsFromRunParams 
                    (rp: runParameters) 
                    (odt: outputDataType) : queryParams option =
        maybe {
            let! repl = rp.GetRepl()
            let! curGen = rp.GetGenerationCurrent()
            let! scPP = rp.GetSorterCountPerPool()
            let! spc = rp.GetSorterPoolCount()
            let! spsev = rp.GetSeedPoolSorterEvalSelectionType()
            let! mmod = rp.GetMutationMod()
            return makeQueryParams repl curGen scPP spc spsev mmod odt
        }

    let private withLocalParams (rp:runParameters) =
        let rpn = standardPoolSzParams rp
        rpn.WithSeedModificationRate(Some 0.02<seedModificationRate>)
            .WithModificationRate(Some 0.06<modificationRate>)
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


    let saveIntervals = SampleRegistry.samplingConfigsDict["expInterval100_L50"]
    let saveSubIntervals = SampleRegistry.samplingConfigsDict["summaryInterval_C.K"]

    let dbTest = new GeneSortGenDbMp(dbFolderTest, queryParamsFromRunParams, saveIntervals, saveSubIntervals)
    let dbPools4098a = new GeneSortGenDbMp(dbFolderPools4098a, queryParamsFromRunParams, saveIntervals, saveSubIntervals)
    let dbPools4098b = new GeneSortGenDbMp(dbFolderPools4098b, queryParamsFromRunParams, saveIntervals, saveSubIntervals)
    let dbPools4098c = new GeneSortGenDbMp(dbFolderPools4098c, queryParamsFromRunParams, saveIntervals, saveSubIntervals)


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (dbNamePoolsTest, dbTest :> IGeneSortDb);
            (dbNamePools4096_1_vs_512, dbPools4098a :> IGeneSortDb);
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
            runName = sprintf @"Test_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for 24pfx Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [1] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["256"])
                (runParameters.mutationModKey, [3;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let PoolSz_1n512 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePools4096_1_vs_512
            runName = sprintf @"PoolSz_1_vs_512_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Pool size comp (1 vs 512) for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [5] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["1"; "512";])
                (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
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
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }


        let PoolSz_4096 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePools4096_4096
            runName = sprintf @"PoolSz_4096_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Pool size 4096 for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [5] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["4096";])
                (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }
