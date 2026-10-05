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


module MutationRate =

    let dbVariableModR_32Name = "VariableModRates_32" |> UMX.tag<databaseName>
    let dbVariableModR_64Name = "VariableModRates_64" |> UMX.tag<databaseName>

    let dbMaxModRate_32Name = "MaxModRate_32" |> UMX.tag<databaseName>
    let dbMaxModRate_64Name = "MaxModRate_64" |> UMX.tag<databaseName>

    do QueryParamsBuilders.registerAll ()





    module VarModR_32 =



        let Test (executorType: sorterSgdExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-sgd.msrs32-mutation-rate"
            databaseName = dbVariableModR_32Name
            projectName = projName
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
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs32MutationRate
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let EqualOPS (executorType: sorterSgdExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-sgd.msrs32-mutation-rate"
            databaseName = dbVariableModR_32Name
            projectName = projName
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
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs32MutationRate
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }

        

        let EqualOPS2 (executorType: sorterSgdExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-sgd.msrs32-mutation-rate"
            databaseName = dbVariableModR_32Name
            projectName = projName
            runName = sprintf @"EqualOPS2%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [12] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
                (runParameters.paraRateKey, [1.5; 2.5; 3.5] |> List.map string)
                (runParameters.selfSymRateKey, [4.5; 5.5; 6.5]  |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
                (runParameters.modificationRateKey, [0.225; 0.25; 0.275;] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs32MutationRate
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }

        
    module VarModR_64 =



        let Test (executorType: sorterSgdExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-sgd.msrs32-mutation-rate"
            databaseName = dbVariableModR_64Name
            projectName = projName
            runName = sprintf @"Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [12] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
                (runParameters.paraRateKey, [4.001] |> List.map string)
                (runParameters.selfSymRateKey, [4.001]  |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.modificationRateKey, [0.25;] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs32MutationRate
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let WideTest (executorType: sorterSgdExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-sgd.msrs32-mutation-rate"
            databaseName = dbVariableModR_64Name
            projectName = projName
            runName = sprintf @"WideTest%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [5] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
                (runParameters.paraRateKey,    [0.05;  0.1;   0.5;   1.001; 1.5;   2.001; 3.001;] |> List.map string)
                (runParameters.selfSymRateKey, [1.001; 1.5;   2.001; 3.001; 4.001; 5.001; 6.001]  |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.modificationRateKey, [0.015; 0.20; 0.25; 0.30; 0.40; 0.50; 0.99] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs32MutationRate
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }

        

        let NarrowTest (executorType: sorterSgdExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-sgd.msrs32-mutation-rate"
            databaseName = dbVariableModR_64Name
            projectName = projName
            runName = sprintf @"NarrowTest%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [4] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
                (runParameters.paraRateKey,    [0.05;  0.1;   0.5;   1.001; ] |> List.map string)
                (runParameters.selfSymRateKey, [1.5;   2.001;]  |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.modificationRateKey, [0.25; 0.30; 0.40; 0.50;] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs32MutationRate
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }




    module MaxModR_32 =


        
        let Test2 (executorType: sorterSgdExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-sgd.msrs32-mutation-rate"
            databaseName = dbMaxModRate_32Name
            projectName = projName
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
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs32MutationRateMax
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }



    module MaxModR_64 =


        
        let WideTest (executorType: sorterSgdExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-sgd.msrs32-mutation-rate"
            databaseName = dbMaxModRate_64Name
            projectName = projName
            runName = sprintf @"WideTest2%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [10] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
                (runParameters.paraRateKey, [0.025; 0.05; 0.75; 0.1] |> List.map string)
                (runParameters.selfSymRateKey, [0.75; 1.001; 1.5; 2.001] |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.msrs32MutationRateMax
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }
