module GeneSort.Dispatch.V1.SorterSgd.Msrs24p3b.OrthoPara

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Msrs24p3b.Common
open GeneSort.Dispatch.V1.SorterSgd



let globalSorterCount = 8192 |> UMX.tag<sorterCount>
let dbOrthoPara32Name = "OrthoPara32" |> UMX.tag<databaseName>


let makeQueryParams
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

let private finishRunParams (host: IRunHost) (rp:runParameters) =
    let rp2 = withLocalParams rp
    let scpp = rp.GetSorterCountPerPool().Value
    let selScpp = scpp
    let spc = (%globalSorterCount / %scpp) |> UMX.tag<sorterPoolCount> |> Option.Some
    let rp3 = rp2.WithSorterPoolCount(spc)
    let qp = host.QueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

    rp3.WithRunFinished(Some false)
            .WithId(Some qp.Value.Id)
            .WithRunName(Some host.Run.RunName)
            .WithModificationRate(Some 0.99<modificationRate>)
            .WithSelectedSorterCountPerPool(Some selScpp)



do QueryParamsCatalog.register (QueryParamsCatalog.nameForDatabase %projName %dbOrthoPara32Name) (queryParamsFromRunParams dbOrthoPara32Name)

let makeDatabase (dbName: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName dbName)


let createRunHost (spec: runHostSpec) : IRunHost =
    let db = makeDatabase spec.databaseName
    let run = run.create spec.databaseName projName spec.runName spec.runDescription spec.spans
    runHost.Create db spec run :> IRunHost


module Specs =

    let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
        databaseName = dbOrthoPara32Name
        runName = sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3b Msrs, NoMods"
        spans = [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
            (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }
