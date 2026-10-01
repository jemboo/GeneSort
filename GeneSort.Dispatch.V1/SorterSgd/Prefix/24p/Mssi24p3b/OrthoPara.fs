module GeneSort.Dispatch.V1.SorterSgd.Mssi24p3b.OrthoPara

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Mssi24p3b.Common
open GeneSort.Dispatch.V1.SorterSgd



let globalSorterCount = 1024 |> UMX.tag<sorterCount>
let dbOrthoPara64Name = "OrthoPara64" |> UMX.tag<databaseName>
let dbOrthoPara128Name = "OrthoPara128" |> UMX.tag<databaseName>

let makeQueryParams
        (dbName: string<databaseName>)
        (repl: int<replNumber>)
        (codeMod: string<codeModKey>)
        (genCurrent: int<generationNumber>)
        (sorterCtPerPool: int<sorterCountPerPool>)
        (sorterPoolCt: int<sorterPoolCount>)
        (modR: float<modificationRate>)
        (para: float<paraRate>)
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
            (runParameters.modificationRateKey, (Some modR) |> ModificationRate.toString)
            (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
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
        let! mRate = rp.GetModificationRate()
        let! para = rp.GetParaRate()
        return makeQueryParams dbName repl codeMod curGen scPP spc mRate para mmod odt
    }


let private withLocalParams (rp:runParameters) =
    let rpn = standardParams rp
    rpn.WithOrthoRate(Some 1.001<orthoRate>)


let private paramMapFilter (rp: runParameters) =
    Some rp

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


let saveIntervals = SampleRegistry.samplingConfigsDict["expInterval100_L50ss"]
let saveSubIntervals = SampleRegistry.samplingConfigsDict["summaryInterval_C.1p5C"]

let makeDatabase (dbName: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName dbName, queryParamsFromRunParams dbName)


let createRunHost (spec: runHostSpec) : IRunHost =
    let db = makeDatabase spec.databaseName
    let run = run.createWithIntervalNames spec.databaseName projName spec.runName spec.runDescription spec.spans saveIntervals.Name saveSubIntervals.Name
    runHost.Create db spec run :> IRunHost


module Specs64 =

    let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
        databaseName = dbOrthoPara64Name
        runName = sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
        spans = [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [1] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
            (runParameters.paraRateKey, [0.5; 1.01; 1.5; 2.01] |> List.map string)
            (runParameters.modificationRateKey, [ 0.09; 0.11; ] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [64;] |> List.map string)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


    let SymForceDiff1 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        databaseName = dbOrthoPara64Name
        runName = sprintf @"SymForce_Diff1%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
        spans = [
            (runParameters.codeModKey, ["SymForce_Diff1"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
            (runParameters.paraRateKey, [0.5; 1.01;] |> List.map string)
            (runParameters.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [64;] |> List.map string)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


    let SymForce (executorType: sorterSgdExecutorType)  : runHostSpec = {
        databaseName = dbOrthoPara64Name
        runName = sprintf @"SymForce%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
        spans = [
            (runParameters.codeModKey, ["SymForce"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
            (runParameters.paraRateKey, [0.5; 1.01;] |> List.map string)
            (runParameters.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [64;] |> List.map string)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }


module Specs128 =

    let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
        databaseName = dbOrthoPara128Name
        runName = sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
        spans = [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [6] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [128] |>  List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
            (runParameters.modificationRateKey, [0.09; 0.11;] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [128;] |> List.map string)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }
