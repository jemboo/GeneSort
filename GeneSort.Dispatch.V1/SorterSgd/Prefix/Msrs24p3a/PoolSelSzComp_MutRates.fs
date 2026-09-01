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


module PoolSelSzComp_ModRates =

    let globalSorterCount = 4096 |> UMX.tag<sorterCount>
    let dbNameTest = "PoolSelSzComp_ModRatesTest" |> UMX.tag<databaseName>
    let dbName_256 = "PoolSelSzComp_ModRates_256" |> UMX.tag<databaseName>

    let dbFolderTest = @$"c:\Projects\{%projName}\{%dbNameTest}\Data" |> UMX.tag<pathToRootFolder>
    let dbFolder256 = @$"c:\Projects\{%projName}\{%dbName_256}\Data" |> UMX.tag<pathToRootFolder>


    let makeQueryParams
            (repl: int<replNumber>)
            (genCurrent: int<generationNumber>)
            (sorterCtPerPool: int<sorterCountPerPool>)
            (sorterPoolCt: int<sorterPoolCount>)
            (mdr: float<modificationRate>)
            (ses:sorterEvalSelectionType)
            (selSz:int<sorterCountPerPool>)
            (mmod: int<mutationMod>)
            (outDt: outputDataType) : queryParams =

        match outDt with
        | outputDataType.RunParameters _ ->
            queryParams.create 
                dbNameTest 
                projName
                (Some repl)
                outDt
                [|
                    (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                    (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                    (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                    (runParameters.seedPoolSorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                    (runParameters.selectedSorterCountPerPoolKey, (Some selSz) |> SorterCountPerPool.toString)
                    (runParameters.mutationModKey, (Some %mmod) |> string)
                |]
        | _ ->
            queryParams.create 
                dbNameTest 
                projName
                (Some repl)
                outDt
                [| 
                    (runParameters.generationCurrentKey, (Some genCurrent) |> GenerationNumber.toString)
                    (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                    (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                    (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                    (runParameters.seedPoolSorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                    (runParameters.selectedSorterCountPerPoolKey, (Some selSz) |> SorterCountPerPool.toString)
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
            let! mdr = rp.GetModificationRate()
            let! spsev = rp.GetSeedPoolSorterEvalSelectionType()
            let! mmod = rp.GetMutationMod()
            let! selSz = rp.GetSelectedSorterCountPerPool()
            return makeQueryParams repl curGen scPP spc mdr spsev selSz mmod odt
        }

    let private withLocalParams (rp:runParameters) =
        let rpn = standardPoolSzParams rp
        rpn.WithOrthoRate(Some 4.001<orthoRate>)
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
    let dbPools256 = new GeneSortGenDbMp(dbFolder256, queryParamsFromRunParams, saveIntervals, saveSubIntervals)



    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (dbNameTest, dbTest :> IGeneSortDb);
            (dbName_256, dbPools256 :> IGeneSortDb);
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
            databaseName = dbNameTest
            runName = sprintf @"PoolSelSzComp_ModRates_Testa_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Selection size comp for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [3] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["128"])
                (runParameters.modificationRateKey, [0.01; 0.02; 0.03; 0.04] |> List.map string)
                (runParameters.mutationModKey, [1; 2; 3; 4] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, ["16"; "32"; "64"; "128"] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }


        let Sz_256 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbName_256
            runName = sprintf @"PoolSelSzComp_ModRates_256_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Selection size comp for 24pfx3a Msrs"
            spans = [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [12] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["256";])
                (runParameters.modificationRateKey, [0.01; 0.03; 0.05; 0.07] |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, ["128"; "256"] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

