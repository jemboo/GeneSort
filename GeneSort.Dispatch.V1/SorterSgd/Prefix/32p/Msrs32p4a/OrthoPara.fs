namespace GeneSort.Dispatch.V1.SorterSgd.Msrs32p4a

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Msrs32p4a.Common
open GeneSort.Dispatch.V1.SorterSgd


module OrthoPara =

    let dbOrthoParaTestName = "OrthoParaTest" |> UMX.tag<databaseName>
    let dbOrthoParaRatesName = "OrthoParaRates" |> UMX.tag<databaseName>

    let saveIntervals = SampleRegistry.samplingConfigsDict["expInterval100_L50ss"]
    let saveSubIntervals = SampleRegistry.samplingConfigsDict["summaryInterval_C.1p5C"]



    let private makeQueryParams
            (dbName: string<databaseName>)
            (repl: int<replNumber>)
            (codeMod: string<codeModKey>)
            (genCurrent: int<generationNumber>)
            (sorterCtPerPool: int<sorterCountPerPool>)
            (sorterPoolCt: int<sorterPoolCount>)
            (para: float<paraRate>)
            (selfSym: float<selfSymRate>)
            (mmod: int<mutationMod>)
            (outDt: outputDataType) : queryParams =

        queryParams.create 
            dbOrthoParaTestName 
            projName
            (Some repl)
            (Some genCurrent)
            outDt
            [|
                (runParameters.codeModKey, (Some codeMod) |> CodeModKey.toString)
                (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
                (runParameters.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
            |]


    let queryParamsFromRunParams
                    (dbName: string<databaseName>)
                    (rp: runParameters)
                    (odt: outputDataType) : queryParams option =
        maybe {
            let! repl = rp.GetRepl()
            let! codeMod = rp.GetCodeModKey()
            let! curGen = rp.GetGenerationCurrent()
            let! scPP = rp.GetSorterCountPerPool()
            let! spc = rp.GetSorterPoolCount()
            let! mmod = rp.GetMutationMod()
            let! para = rp.GetParaRate()
            let! self = rp.GetSelfSymRate()
            return makeQueryParams dbName repl codeMod curGen scPP spc para self mmod odt
        }



    let private withLocalParams (rp:runParameters) =
        let rpn = standardParams rp
        rpn.WithOrthoRate(Some 4.001<orthoRate>)


    let private paramMapFilter (rp: runParameters) =
        Some rp



    let makeDatabase (dbName: string<databaseName>) : IGeneSortDb =
        new GeneSortGenDbMp(makeFolderFromDbName dbName, queryParamsFromRunParams dbName, saveIntervals, saveSubIntervals)


    let createRunHost (spec: runHostSpec) : IRunHost =
        let db = makeDatabase spec.databaseName
        let run = run.create spec.databaseName projName spec.runName spec.runDescription
        runHost.Create db spec run :> IRunHost



    module SpecsTest =
    
        let globalSorterCount = 128 |> UMX.tag<sorterCount>

        let private finishRunParams (host: IRunHost) (rp:runParameters) =
            let rp2 = withLocalParams rp
            let scpp = rp.GetSorterCountPerPool().Value
            let selScpp = scpp
            let spc = (%globalSorterCount / %scpp) |> UMX.tag<sorterPoolCount> |> Option.Some
            let rp3 = rp2.WithSorterPoolCount(spc)
            let qp = host.RunDb.MakeQueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

            rp3.WithRunFinished(Some false)
                    .WithId(Some qp.Value.Id)
                    .WithRunName(Some host.Run.RunName)
                    .WithModificationRate(Some 0.99<modificationRate>)
                    .WithSelectedSorterCountPerPool(Some selScpp)


        let Test (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = dbOrthoParaTestName
            runName = sprintf @"Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [1] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
                (runParameters.paraRateKey, [0.15] |> List.map string)
                (runParameters.selfSymRateKey, [2.75] |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


    module Rates32 =
    
        let globalSorterCount = 512 |> UMX.tag<sorterCount>

        let private finishRunParams (host: IRunHost) (rp:runParameters) =
            let rp2 = withLocalParams rp
            let scpp = rp.GetSorterCountPerPool().Value
            let selScpp = scpp
            let spc = (%globalSorterCount / %scpp) |> UMX.tag<sorterPoolCount> |> Option.Some
            let rp3 = rp2.WithSorterPoolCount(spc)
            let qp = host.RunDb.MakeQueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

            rp3.WithRunFinished(Some false)
                    .WithId(Some qp.Value.Id)
                    .WithRunName(Some host.Run.RunName)
                    .WithModificationRate(Some 0.99<modificationRate>)
                    .WithSelectedSorterCountPerPool(Some selScpp)


        let Test (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = dbOrthoParaTestName
            runName = sprintf @"Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [4] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
                (runParameters.paraRateKey, [0.15; 0.3; 1.01; 2.01] |> List.map string)
                (runParameters.selfSymRateKey, [1.01; 2.01; 4.01; 7.01] |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

