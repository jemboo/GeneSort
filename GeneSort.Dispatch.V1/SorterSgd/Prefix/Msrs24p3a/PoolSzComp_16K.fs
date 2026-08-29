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


module PoolSzComp16k =

   // let globalSorterCount = 8192 |> UMX.tag<sorterCount> //(2^13)
    let globalSorterCount = 16384 |> UMX.tag<sorterCount> //(2^16)
    //let globalSorterCount = 131072 |> UMX.tag<sorterCount> //(2^17)
    //let globalSorterCount = 262144 |> UMX.tag<sorterCount> //(2^18)

    let dbNamePools_16K = "PoolSz_16K" |> UMX.tag<databaseName>
    let dbFolderPoolSz_16K = @$"c:\Projects\{%projName}\{%dbNamePools_16K}\Data" |> UMX.tag<pathToRootFolder>

    let makeQueryParams
            (dbName: string<databaseName>)
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
                dbName 
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
                dbName 
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
            (dbName: string<databaseName>)
            (rp: runParameters) 
            (odt: outputDataType) : queryParams option =
        maybe {
            let! repl = rp.GetRepl()
            let! curGen = rp.GetGenerationCurrent()
            let! scPP = rp.GetSorterCountPerPool()
            let! spc = rp.GetSorterPoolCount()
            let! spsev = rp.GetSeedPoolSorterEvalSelectionType()
            let! mmod = rp.GetMutationMod()
            return makeQueryParams dbName repl curGen scPP spc spsev mmod odt
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


    let saveIntervals = SampleRegistry.samplingConfigsDict["expInterval100_L50s"]
    let saveSubIntervals = SampleRegistry.samplingConfigsDict["summaryInterval_C.2C"]

    let dbPools_16K = new GeneSortGenDbMp(dbFolderPoolSz_16K, queryParamsFromRunParams dbNamePools_16K, saveIntervals, saveSubIntervals)


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (dbNamePools_16K, dbPools_16K :> IGeneSortDb);
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

        let PoolSz_16Kp2 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePools_16K
            runName = sprintf @"PoolSz_16Kp2_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Pool size 16K for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [11] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["16384";])
                (runParameters.mutationModKey, [0 .. 1;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


        let PoolSz_16Kp4 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePools_16K
            runName = sprintf @"PoolSz_16Kp4_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Pool size 16K for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [11] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["16384";])
                (runParameters.mutationModKey, [0 .. 3;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


        let PoolSz_8Kp8 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePools_16K
            runName = sprintf @"PoolSz_16Kp8_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Pool size 16K for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [1] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["16384";])
                (runParameters.mutationModKey, [0 .. 7;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }
