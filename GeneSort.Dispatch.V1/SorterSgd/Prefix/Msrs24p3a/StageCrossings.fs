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
open GeneSort.SortingOps


module StageCrossings =

    let globalSorterCount = 512 |> UMX.tag<sorterCount>
    let dbNamePoolsTest = "StageCrossingsTest" |> UMX.tag<databaseName>
    let dbNamePoolSz256 = "StageCrossingsPoolSz256" |> UMX.tag<databaseName>

    let dbFolderTest = @$"c:\Projects\{%projName}\{%dbNamePoolsTest}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolderPoolSz256 = @$"c:\Projects\{%projName}\{%dbNamePoolSz256}\Data" |> UMX.tag<pathToRootFolder>

    let makeQueryParams
            (repl: int<replNumber>)
            (genCurrent: int<generationNumber>)
            (sorterCtPerPool: int<sorterCountPerPool>)
            (sorterPoolCt: int<sorterPoolCount>)
            (ses:sorterSelectionType)
            (mmod: int<mutationMod>)
            (sev: sorterEvalMeasure)
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
                (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
                (runParameters.sorterEvalMeasureKey, sev |> SorterEvalFunctions.toCompactString)
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
            let! sev = rp.GetSorterEvalMeasure()
            return makeQueryParams repl curGen scPP spc spsev mmod sev odt
        }

    let private withLocalParams (rp:runParameters) =
        let rpn = standardStageCrossingsParams rp
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


    let saveIntervals = SampleRegistry.samplingConfigsDict["expInterval100_L50s"]
    let saveSubIntervals = SampleRegistry.samplingConfigsDict["summaryInterval_C.K"]

    let dbTest = new GeneSortGenDbMp(dbFolderTest, queryParamsFromRunParams, saveIntervals, saveSubIntervals)
    let dbPoolSz256 = new GeneSortGenDbMp(dbFolderPoolSz256, queryParamsFromRunParams, saveIntervals, saveSubIntervals)



    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (dbNamePoolsTest, dbTest :> IGeneSortDb);
            (dbNamePoolSz256, dbPoolSz256 :> IGeneSortDb);
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
            runDescription = "StageCrossing analysis for 24pfx Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [6] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["512"])
                (runParameters.mutationModKey, [0 .. 7] |> List.map string)
                CommonParams.sorterEvalMeasure_StageCrossing_Range
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        let PoolSz_256a (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePoolSz256
            runName = sprintf @"PoolSz256a_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "StageCrossing analysis for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [5] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["256"])
                (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
                CommonParams.sorterEvalMeasure_StageCrossing_Range
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }
