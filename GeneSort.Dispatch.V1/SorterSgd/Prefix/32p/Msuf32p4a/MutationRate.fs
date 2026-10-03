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



let dbPool32name = "Pool_32" |> UMX.tag<databaseName>
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
                     .WithGenerationCurrent(Some 0<generationNumber>)
        let qp = host.QueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

        rp3.WithRunFinished(Some false)
                .WithId(Some qp.Value.Id)
                .WithRunName(Some host.Run.RunName)
                .WithSelectedSorterCountPerPool(Some scpp)


    let Pool_32_V1 (executorType: sorterSgdExecutorType)  : runHostSpec = {
        queryCatalogName = "sorter-sgd.msuf32-mutation-rate"
        databaseName = dbPool32name
        runName = sprintf @"Pool_32_V1%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
        runDescription = "Rate comp for Msrs32p4a Msuf4"
        spans = [
            (runParameters.sorterCountPerPoolSetKey, [512] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationIntervalLastKey, [1] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey,    [0.85; 0.95; 1.05; 1.15;] |> List.map string)
            (runParameters.selfSymRateKey, [0.75; 1.25; 1.5; 1.75;]  |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.seedModificationRateKey, [0.020; 0.030; 0.040; 0.050;] |> List.map string)
            (runParameters.modificationRateKey, [0.020; 0.030; 0.040; 0.050;] |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
        ]
        filter = paramMapFilter
        enhancer = finishRunParams
        allowOverwrite = false |> UMX.tag
        maxParallel = 1
    }