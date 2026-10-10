module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_32

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Common
open GeneSort.Dispatch.V1.SorterSgd

let projName = "SorterSgd.Soss.32p4a.Msuf" |> UMX.tag<projectName>

let dbPool32name = "Pool_32" |> UMX.tag<databaseName>
let dbPool32TestName = "Pool_32_Test" |> UMX.tag<databaseName>
let dbPool512name = "Pool_512" |> UMX.tag<databaseName>

let Pool_32_Test (executorType: sorterSgdExecutorType)  : run =
    run.SgdRun (
        SgdRun.create
            dbPool32TestName
            projName
            (sprintf @"Pool_32_Testa%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag<runName>)
            "Soss rate comparison for Msuf32p4a"
            [
        (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
        (runParameters.sorterCountPerPoolSetKey, [128] |> List.map string)
        (runParameters.seedSossKey, [1234UL] |> List.map string)
        (runParameters.codeModKey, ["NoMods"] |> List.map string)
        (runParameters.generationIntervalLastKey, [2] |> List.map string)
        (runParameters.paraRateKey,    [1.15;] |> List.map string)
        (runParameters.selfSymRateKey, [1.75;]  |> List.map string)
        (runParameters.mutationModKey, [0;] |> List.map string)
        (runParameters.seedModificationRateKey, [0.020;] |> List.map string)
        (runParameters.modificationRateKey, [0.035;]  |> List.map string)
        (runParameters.mutatorVariantKey, [mutatorVariant.V2;] |> List.map (MutatorVariant.toString))
    ]
            "expInterval100_L50ss"
            "summaryInterval_C.1p5C"
            QueryCatalogNames.sorterSgdMsuf32MutationRate
            RunParamBuilderNames.Filter.identity
            RunParamBuilderNames.Enhancer.msuf32MutationRate
            false
    )

let Pool_32_V1 (executorType: sorterSgdExecutorType)  : run =
    run.SgdRun (
        SgdRun.create
            dbPool32name
            projName
            (sprintf @"Pool_32_V1%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
            "Soss rate comparison for Msuf32p4a"
            [
        (runParameters.seedSossKey, [1234UL] |> List.map string)
        (runParameters.sorterCountPerPoolSetKey, [512] |> List.map string)
        (runParameters.codeModKey, ["NoMods"] |> List.map string)
        (runParameters.generationIntervalLastKey, [3] |> List.map string)
        (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
        (runParameters.paraRateKey,    [0.85; 0.95; 1.05; 1.15;] |> List.map string)
        (runParameters.selfSymRateKey, [0.75; 1.25; 1.5; 1.75;]  |> List.map string)
        (runParameters.mutationModKey, [0] |> List.map string)
        (runParameters.seedModificationRateKey, [0.020; 0.030; 0.040; 0.050;] |> List.map string)
        (runParameters.modificationRateKey, [0.015; 0.020; 0.025; 0.030; 0.040; 0.050;] |> List.map string)
        (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
    ]
            "expInterval100_L50ss"
            "summaryInterval_C.1p5C"
            QueryCatalogNames.sorterSgdMsuf32MutationRate
            RunParamBuilderNames.Filter.identity
            RunParamBuilderNames.Enhancer.msuf32MutationRate
            false
    )

let Pool_512_V2a (executorType: sorterSgdExecutorType)  : run =
    run.SgdRun (
        SgdRun.create
            dbPool512name
            projName
            (sprintf @"Pool_512_V2a%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
            "Soss rate comparison for Msuf32p4a"
            [
        (runParameters.seedSossKey, [1234UL] |> List.map string)
        (runParameters.sorterCountPerPoolSetKey, [512] |> List.map string)
        (runParameters.codeModKey, ["NoMods"] |> List.map string)
        (runParameters.generationIntervalLastKey, [3] |> List.map string)
        (runParameters.sorterCountPerPoolKey, [512] |>  List.map string)
        (runParameters.paraRateKey,    [0.85; 0.95; 1.05; 1.15;] |> List.map string)
        (runParameters.selfSymRateKey, [0.75; 1.25; 1.5; 1.75;]  |> List.map string)
        (runParameters.mutationModKey, [0] |> List.map string)
        (runParameters.seedModificationRateKey, [0.015; 0.02; 0.025;] |> List.map string)
        (runParameters.modificationRateKey, [0.020; 0.025; 0.030; 0.035; 0.040;] |> List.map string)
        (runParameters.mutatorVariantKey, [mutatorVariant.V2] |> List.map (MutatorVariant.toString))
    ]
            "expInterval100_L50ss"
            "summaryInterval_C.1p5C"
            QueryCatalogNames.sorterSgdMsuf32MutationRate
            RunParamBuilderNames.Filter.identity
            RunParamBuilderNames.Enhancer.msuf32MutationRate
            false
    )
