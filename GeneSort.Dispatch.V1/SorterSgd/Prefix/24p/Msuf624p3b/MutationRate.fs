module GeneSort.Dispatch.V1.SorterSgd.Msuf624p3b.MutationRate

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Msuf624p3b.Common
open GeneSort.Dispatch.V1.SorterSgd

let dbVariableModR_32Name = "VariableModRates_Uf6_32" |> UMX.tag<databaseName>

let saveIntervals = SampleRegistry.samplingConfigsDict["expInterval100_L50ss"]
let saveSubIntervals = SampleRegistry.samplingConfigsDict["summaryInterval_C.1p5C"]

let makeQueryParams
        (dbName: string<databaseName>)
        (repl: int<replNumber>)
        (codeMod: string<codeModKey>)
        (genCurrent: int<generationNumber>)
        (sorterCtPerPool: int<sorterCountPerPool>)
        (sorterPoolCt: int<sorterPoolCount>)
        (para: float<paraRate>)
        (selfSym: float<selfSymRate>)
        (seedModR: float<seedModificationRate>)
        (modR: float<modificationRate>)
        (mmod: int<mutationMod>)
        (outDt: outputDataType) : queryParams =

    queryParams.create
        dbName
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
            (runParameters.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString)
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
        let! seedModR = rp.GetSeedModificationRate()
        let! modR = rp.GetModificationRate()
        let! mmod = rp.GetMutationMod()
        return makeQueryParams dbName repl codeMod curGen scPP spc para self seedModR modR mmod odt
    }

let private withLocalParams (rp: runParameters) =
    let rpn = projectParams rp
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

    let private finishRunParams (host: IRunHost) (rp: runParameters) =
        let rp2 = withLocalParams rp
        let scpp = rp.GetSorterCountPerPool().Value
        let scpps = rp.GetSorterCountPerPoolSet().Value
        let spc = (%scpps / %scpp) |> UMX.tag<sorterPoolCount> |> Option.Some
        let rp3 = rp2.WithSorterPoolCount(spc)
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

        rp3.WithRunFinished(Some false)
            .WithId(Some qp.Value.Id)
            .WithRunName(Some host.Run.RunName)
            .WithSelectedSorterCountPerPool(Some scpp)

    let Test (executorType: sorterSgdExecutorType) : runHostSpec = {
        databaseName = dbVariableModR_32Name
        runName = sprintf @"Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Mutation rate test for Msuf6 24p3b"
        spans = [
            (runParameters.sorterCountPerPoolSetKey, [256] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [3] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |> List.map string)
            (runParameters.paraRateKey, [1.001] |> List.map string)
            (runParameters.selfSymRateKey, [2.001] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.seedModificationRateKey, [0.1] |> List.map string)
            (runParameters.modificationRateKey, [0.05; 0.075; 0.1; 0.125; 0.15; 0.175] |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 6
    }

    let WideTest (executorType: sorterSgdExecutorType) : runHostSpec = {
        databaseName = dbVariableModR_32Name
        runName = sprintf @"WideTest%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Wide mutation rate test for Msuf6 24p3b"
        spans = [
            (runParameters.sorterCountPerPoolSetKey, [512] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [3] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |> List.map string)
            (runParameters.paraRateKey, [0.1; 0.5; 1.001; 1.5] |> List.map string)
            (runParameters.selfSymRateKey, [1.001; 1.5; 2.001; 3.001] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.seedModificationRateKey, [0.05] |> List.map string)
            (runParameters.modificationRateKey, [0.0025; 0.0035; 0.005; 0.0075; 0.0125; 0.02; 0.035; 0.06] |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }
