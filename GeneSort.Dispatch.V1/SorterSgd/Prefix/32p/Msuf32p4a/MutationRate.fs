module GeneSort.Dispatch.V1.SorterSgd.Msuf32p4a.MutationRate

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Msuf32p4a.Common
open GeneSort.Dispatch.V1.SorterSgd



let dbVariableModR_32Name = "VariableModRates_32" |> UMX.tag<databaseName>
let dbVariableModR_64Name = "VariableModRates_64" |> UMX.tag<databaseName>



let private withLocalParams (rp:runParameters) =
    let rpn = projectParams rp
    rpn.WithOrthoRate(Some 4.001<orthoRate>)


let private paramMapFilter (rp: runParameters) =
    Some rp
do QueryParamsBuilders.registerAll ()

let makeDatabase (dbName: string<databaseName>) : IGeneSortDb =
    new GeneSortDbMp(makeFolderFromDbName dbName, "sorter-sgd.msuf32-mutation-rate")


let createRunHost (spec: runHostSpec) : IRunHost =
    let db = makeDatabase spec.databaseName
    let run = run.createWithCatalogName spec.databaseName projName spec.runName spec.runDescription spec.spans spec.queryCatalogName
    runHost.Create db spec run :> IRunHost


module VarModR_32 =

    let private finishRunParams (host: IRunHost) (rp:runParameters) =
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


    let Test (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msuf32-mutation-rate"
        databaseName = dbVariableModR_32Name
        runName = sprintf @"Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Rate comp for Msrs32p4a Msuf4"
        spans = [
            (runParameters.sorterCountPerPoolSetKey, [256] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [2] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey,    [1.001;] |> List.map string)
            (runParameters.selfSymRateKey, [2.001;]  |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.seedModificationRateKey, [0.005;] |> List.map string)
            (runParameters.modificationRateKey, [0.0075;] |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 1
    }


    let WideTest (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msuf32-mutation-rate"
        databaseName = dbVariableModR_32Name
        runName = sprintf @"WideTest%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Rate comp for Msrs32p4a Msuf4"
        spans = [
            (runParameters.sorterCountPerPoolSetKey, [512] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [8] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey,    [0.1;   0.5; 1.001; 1.5;  ] |> List.map string)
            (runParameters.selfSymRateKey, [1.001; 1.5; 2.001; 3.001;]  |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.seedModificationRateKey, [0.05;] |> List.map string)
            (runParameters.modificationRateKey, [0.0025; 0.0035; 0.005; 0.0075; 0.0125; 0.02; 0.035; 0.06;] |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 8
    }
