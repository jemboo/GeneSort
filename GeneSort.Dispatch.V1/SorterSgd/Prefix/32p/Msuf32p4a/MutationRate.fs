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
let dbPool512name = "Pool_512" |> UMX.tag<databaseName>







module VarModR_32 =




    let Pool_32_Test (executorType: sorterSgdExecutorType)  : run =
        run.SgdRun (
            SgdRun.create
                dbPool32name
                projName
                (sprintf @"Pool_32_Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "Rate comp for Msrs32p4a Msuf4"
                [
            (runParameters.sorterCountPerPoolSetKey, [128] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationIntervalLastKey, [9] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey,    [1.15;] |> List.map string)
            (runParameters.selfSymRateKey, [1.75;]  |> List.map string)
            (runParameters.mutationModKey, [0; 2; 3; 4; 5; 6;] |> List.map string)
            (runParameters.seedModificationRateKey, [0.020; 0.050;] |> List.map string)
            (runParameters.modificationRateKey, [0.020; 0.050;]  |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1; mutatorVariant.V2;] |> List.map (MutatorVariant.toString))
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                "sorter-sgd.msuf32-mutation-rate"
                RunParamBuilderNames.Filter.seedModificationRateDiffers
                RunParamBuilderNames.Enhancer.msuf32MutationRate
                false
        )


    let Pool_32_V1 (executorType: sorterSgdExecutorType)  : run =
        run.SgdRun (
            SgdRun.create
                dbPool32name
                projName
                (sprintf @"Pool_32_V1%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "Rate comp for Msrs32p4a Msuf4"
                [
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
                "sorter-sgd.msuf32-mutation-rate"
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
                "Rate comp for Msrs32p4a Msuf4"
                [
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
                "sorter-sgd.msuf32-mutation-rate"
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.msuf32MutationRate
                false
        )
