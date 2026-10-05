namespace GeneSort.Dispatch.V1.SorterMutate


open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams

module MutateSpecsRm =

    let seedSorterPoolSelectionType = 
            (runParameters.seedSorterPoolSelectionTypeKey, 
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)

    module Specs =

        let Test_Msce (executorType: sorterMutateExecutorType)  : runHostSpec = {
            queryCatalogName = "sorter-mutate.merge"
            databaseName = SorterMutateDbs.RandomMerge.Uniform.dbName
            projectName = SorterMutateDbs.projectName
            runName = sprintf @"Test-Msce_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msce"
            spans = [
                msceModelType
                rngTypeLcg
                dataFormatInt8v512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                mergeLib_Merge32s
                (mutatorSpansMsce 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
            filterCatalogName = RunParamBuilderNames.Filter.mergeSorterModelCompatibility
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sorterMutateStandard
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


        let Test_Mssi (executorType: sorterMutateExecutorType)  : runHostSpec = {
            queryCatalogName = "sorter-mutate.merge"
            databaseName = SorterMutateDbs.RandomMerge.Uniform.dbName
            projectName = SorterMutateDbs.projectName
            runName = sprintf @"Test-Mssi_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                mssiModelType
                rngTypeLcg
                dataFormatInt8v512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                mergeLib_Merge32s
                (mutatorSpansMssi 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
            filterCatalogName = RunParamBuilderNames.Filter.mergeSorterModelCompatibility
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sorterMutateStandard
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


        let Test_Msrs (executorType: sorterMutateExecutorType)  : runHostSpec = {
            queryCatalogName = "sorter-mutate.merge"
            databaseName = SorterMutateDbs.RandomMerge.Uniform.dbName
            projectName = SorterMutateDbs.projectName
            runName = sprintf @"Test-Msrs_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msrs"
            spans = [
                msrsModelType
                rngTypeLcg
                dataFormatInt8v512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                mergeLib_Merge32s
                (mutatorSpansMsrs 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
            filterCatalogName = RunParamBuilderNames.Filter.mergeSorterModelCompatibility
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sorterMutateStandard
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        let Test_Msuf4 (executorType: sorterMutateExecutorType)  : runHostSpec = {
            queryCatalogName = "sorter-mutate.merge"
            databaseName = SorterMutateDbs.RandomMerge.Uniform.dbName
            projectName = SorterMutateDbs.projectName
            runName = sprintf @"Test-Msuf4_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                msuf4ModelType
                rngTypeLcg
                dataFormatInt8v512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                mergeLib_Merge32s
                (mutatorSpansMsuf4 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
            filterCatalogName = RunParamBuilderNames.Filter.mergeSorterModelCompatibility
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sorterMutateStandard
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


    type configType =
        | Test_Msce
        | Test_Mssi
        | Test_Msrs
        | Test_Msuf4
        | Test_Msuf6

    let Configs = Map.ofList 
                    [ 
                        (configType.Test_Msce, Specs.Test_Msce); 
                        (configType.Test_Mssi, Specs.Test_Mssi); 
                        (configType.Test_Msrs, Specs.Test_Msrs); 
                        (configType.Test_Msuf4, Specs.Test_Msuf4);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


