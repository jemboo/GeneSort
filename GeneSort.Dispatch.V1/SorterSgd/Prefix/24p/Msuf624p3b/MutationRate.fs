module GeneSort.Dispatch.V1.SorterSgd.Msuf624p3b.MutationRate

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
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

let private sorterEvalMeasures =
    [ 4.5; 5.5; 7.5; 9.5 ]
    |> List.map (fun unsortedWeight ->
        ceStUcMeasure.create
            (1.1<stageWeight>)
            (unsortedWeight |> UMX.tag<unsortedWeight>)
            (false |> UMX.tag<filterReflectionSymmetric>)
        |> sorterEvalMeasure.CeStUc
        |> SorterEvalMeasure.toCompactString
    )


let private withLocalParams (rp: runParameters) =
    let rpn = projectParams rp
    let sem = rpn.GetSorterEvalMeasure().Value
    rpn.WithSorterEvalMeasureInitial(Some sem)
       .WithOrthoRate(Some 4.001<orthoRate>)

let private paramMapFilter (rp: runParameters) =
    Some rp

do QueryParamsBuilders.registerAll ()

let makeDatabase (dbName: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName dbName, "sorter-sgd.uf6-mutation-rate")

let createRunHost (spec: runHostSpec) : IRunHost =
    let db = makeDatabase spec.databaseName
    let run = run.createWithCatalogName spec.databaseName projName spec.runName spec.runDescription spec.spans spec.queryCatalogName
    runHost.Create db spec run :> IRunHost

module VarModR_32 =

    let private finishRunParams (host: IRunHost) (rp: runParameters) =
        let rp2 = withLocalParams rp
        let scpp = rp.GetSorterCountPerPool().Value
        let scpps = rp.GetSorterCountPerPoolSet().Value
        let spc = (%scpps / %scpp) |> UMX.tag<sorterPoolCount> |> Option.Some
        let rp3 = rp2.WithSorterPoolCount(spc)
        let qp = host.QueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

        rp3.WithRunFinished(Some false)
            .WithId(Some qp.Value.Id)
            .WithRunName(Some host.Run.RunName)
            .WithSelectedSorterCountPerPool(Some scpp)

    let Test (executorType: sorterSgdExecutorType) : runHostSpec = {
        queryCatalogName = "sorter-sgd.uf6-mutation-rate"
        databaseName = dbVariableModR_32Name
        runName = sprintf @"Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Mutation rate test for Msuf6 24p3b"
        spans = [
            (runParameters.sorterCountPerPoolSetKey, [256] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [2] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |> List.map string)
            (runParameters.sorterEvalMeasureKey, sorterEvalMeasures)
            (runParameters.paraRateKey, [1.001] |> List.map string)
            (runParameters.selfSymRateKey, [2.001] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.seedModificationRateKey, [0.05; 0.1] |> List.map string)
            (runParameters.modificationRateKey, [0.05; 0.075; 0.1; 0.125;] |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }

    let WideTest (executorType: sorterSgdExecutorType) : runHostSpec = {
        queryCatalogName = "sorter-sgd.uf6-mutation-rate"
        databaseName = dbVariableModR_32Name
        runName = sprintf @"WideTest%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Wide mutation rate test for Msuf6 24p3b"
        spans = [
            (runParameters.sorterCountPerPoolSetKey, [512] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [10] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [128] |> List.map string)
            (runParameters.sorterEvalMeasureKey, sorterEvalMeasures)
            (runParameters.paraRateKey, [0.1; 0.5; 1.001; 1.5] |> List.map string)
            (runParameters.selfSymRateKey, [1.001; 1.5; 2.001; 3.001] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.seedModificationRateKey, [0.05] |> List.map string)
            (runParameters.modificationRateKey, [0.05; 0.075; 0.1; 0.125;] |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }
