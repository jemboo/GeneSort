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


module PoolSzCompL =

    let globalSorterCount = 8192 |> UMX.tag<sorterCount> //(2^13)
   // let globalSorterCount = 65536 |> UMX.tag<sorterCount> //(2^16)
    //let globalSorterCount = 131072 |> UMX.tag<sorterCount> //(2^17)
    //let globalSorterCount = 262144 |> UMX.tag<sorterCount> //(2^18)

    let dbNamePools_8K = "PoolSz_8K" |> UMX.tag<databaseName>
    let dbNamePools_64K = "PoolSz_64K" |> UMX.tag<databaseName>
    let dbNamePools_256K = "PoolSz_256K" |> UMX.tag<databaseName>
    let dbFolderPoolSz_8K = @$"c:\Projects\{%projName}\{%dbNamePools_8K}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolderPoolSz_64K = @$"c:\Projects\{%projName}\{%dbNamePools_64K}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolderPoolSz_256K = @$"c:\Projects\{%projName}\{%dbNamePools_256K}\Data" |> UMX.tag<pathToRootFolder>

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

    let dbPools_8K = new GeneSortGenDbMp(dbFolderPoolSz_8K, queryParamsFromRunParams dbNamePools_8K, saveIntervals, saveSubIntervals)
    let dbPools_64K = new GeneSortGenDbMp(dbFolderPoolSz_64K, queryParamsFromRunParams dbNamePools_64K, saveIntervals, saveSubIntervals)
    let dbPools_256K = new GeneSortGenDbMp(dbFolderPoolSz_256K, queryParamsFromRunParams dbNamePools_256K, saveIntervals, saveSubIntervals)


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (dbNamePools_8K, dbPools_8K :> IGeneSortDb);
            (dbNamePools_64K, dbPools_64K :> IGeneSortDb);
            (dbNamePools_256K, dbPools_256K :> IGeneSortDb);
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

        let PoolSz_8Kp4 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePools_8K
            runName = sprintf @"PoolSz_8K_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Pool size 8K for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [1] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["8192";])
                (runParameters.mutationModKey, [0 .. 3;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


        let PoolSz_8Kp8 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePools_8K
            runName = sprintf @"PoolSz_8Kp8_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Pool size 8K for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [1] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["8192";])
                (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }


        let PoolSz_8Kp16 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbNamePools_8K
            runName = sprintf @"PoolSz_8Kp16_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Pool size 8K for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [1] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["8192";])
                (runParameters.mutationModKey, [12 .. 27;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }
