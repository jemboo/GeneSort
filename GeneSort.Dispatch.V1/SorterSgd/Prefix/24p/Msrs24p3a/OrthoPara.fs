module GeneSort.Dispatch.V1.SorterSgd.Msrs24p3a.OrthoPara

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



let globalSorterCount = 8192 |> UMX.tag<sorterCount>
let dbOrthoPara32Name = "OrthoPara32" |> UMX.tag<databaseName>


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



do QueryParamsBuilders.registerAll ()

let makeDatabase (name: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName name, "sorter-sgd.msrs-ortho-para")


let createRunHost (spec: runHostSpec) : IRunHost =
    let db = makeDatabase spec.databaseName
    let run = run.createWithCatalogName spec.databaseName projName spec.runName spec.runDescription spec.spans spec.queryCatalogName
    runHost.Create db spec run :> IRunHost


module Specs =

    let PickMode2_2 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-ortho-para"
        databaseName = dbOrthoPara32Name
        runName = sprintf @"PickMode2_2_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3a Msrs, PickMode2_2"
        spans = [
            (runParameters.codeModKey, ["PickMode2_2"] |> List.map string)
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

    let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msrs-ortho-para"
        databaseName = dbOrthoPara32Name
        runName = sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "OrthroPara rate comp for 24pfx3a Msrs, NoMods"
        spans = [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [7] |> List.map string)
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
