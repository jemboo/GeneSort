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


module MaxModRate =

    let dbVariableModR_32Name = "VariableModRates_32" |> UMX.tag<databaseName>
    let dbMaxModRate_32Name = "MaxModRate_32" |> UMX.tag<databaseName>

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
            (modR: float<modificationRate>)
            (mmod: int<mutationMod>)
            (outDt: outputDataType) : queryParams =

        queryParams.create 
            dbVariableModR_32Name 
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
                (runParameters.modificationRateKey, (Some modR) |> ModificationRate.toString)
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
            let! para = rp.GetParaRate()
            let! self = rp.GetSelfSymRate()
            let! modR = rp.GetModificationRate()
            let! mmod = rp.GetMutationMod()
            return makeQueryParams dbName repl codeMod curGen scPP spc para self modR mmod  odt
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



    module VarModR_32 =
    
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
                    .WithSelectedSorterCountPerPool(Some selScpp)


        let Test (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = dbVariableModR_32Name
            runName = sprintf @"Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [12] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
                (runParameters.paraRateKey, [4.001] |> List.map string)
                (runParameters.selfSymRateKey, [4.001]  |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
                (runParameters.modificationRateKey, [0.25;] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let EqualOPS (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = dbVariableModR_32Name
            runName = sprintf @"EqualOPS%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [12] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
                (runParameters.paraRateKey, [3.001; 4.001; 5.001] |> List.map string)
                (runParameters.selfSymRateKey, [3.001; 4.001; 5.001]  |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
                (runParameters.modificationRateKey, [0.015; 0.20; 0.25; 0.30; 0.40; 0.50;] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }


    module MaxModR_32 =
    
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

        
        let Test2 (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = dbMaxModRate_32Name
            runName = sprintf @"Test2%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [10] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
                (runParameters.paraRateKey, [0.05; 0.075; 0.1; 0.125] |> List.map string)
                (runParameters.selfSymRateKey, [1.25; 1.5; 1.75; 2.5] |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }