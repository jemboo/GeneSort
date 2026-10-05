namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Sorting
open GeneSort.SortingOps

module SorterEvalSpecsRm =

    module Specs =

        let Rand_MergeTest_Test (executorType: sorterEvalExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-eval.merge"
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-Test_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [   
                rngTypeLcg
                mergeLib_Merge32s
                dataFormatInt8v512
                allSimpleSorterModelTypes
                noSuffixSuffixType
                sorterEvalTypeV2
                smallSorterCount
            ]
            filterCatalogName = RunParamFilterBuilders.register RunParamFilterBuilders.mergeSorterModelCompatibility
            enhancerCatalogName = RunParamEnhancerBuilders.register RunParamEnhancerBuilders.sorterEvalMerge
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let Rand_MergeTest_Small (executorType: sorterEvalExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-eval.merge"
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-Small_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                dataFormatInt8v512
                allSimpleSorterModelTypes
                noSuffixSuffixType
                sorterEvalTypeV2
                smallMergeSortingWidths
                allMergeDimensions
                extraLargeSorterCount
            ]
            filterCatalogName = RunParamFilterBuilders.register RunParamFilterBuilders.mergeSorterModelCompatibility
            enhancerCatalogName = RunParamEnhancerBuilders.register RunParamEnhancerBuilders.sorterEvalMerge
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }


        let Rand_MergeTest_MediumLd (executorType: sorterEvalExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-eval.merge"
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-MediumLd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                dataFormatInt8v512
                allSimpleSorterModelTypes
                noSuffixSuffixType
                sorterEvalTypeV2
                mediumMergeSortingWidths
                lowMergeDimensions
                largeSorterCount
            ]
            filterCatalogName = RunParamFilterBuilders.register RunParamFilterBuilders.mergeSorterModelCompatibility
            enhancerCatalogName = RunParamEnhancerBuilders.register RunParamEnhancerBuilders.sorterEvalMerge
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


        let Rand_MergeTest_MediumHd (executorType: sorterEvalExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-eval.merge"
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-MediumHd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                dataFormatInt8v512
                noSuffixSuffixType
                allSimpleSorterModelTypes
                sorterEvalTypeV2
                sortingWidth96
                mergeDimension6
                largeSorterCount
            ]
            filterCatalogName = RunParamFilterBuilders.register RunParamFilterBuilders.mergeSorterModelCompatibility
            enhancerCatalogName = RunParamEnhancerBuilders.register RunParamEnhancerBuilders.sorterEvalMerge
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


        let Rand_MergeTest_LargeLd (executorType: sorterEvalExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-eval.merge"
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-LargeLd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                dataFormatInt8v512
                noSuffixSuffixType
                allSimpleSorterModelTypes
                sorterEvalTypeV2
                largeMergeSortingWidths
                mergeDimension2
                largeSorterCount
            ]
            filterCatalogName = RunParamFilterBuilders.register RunParamFilterBuilders.mergeSorterModelCompatibility
            enhancerCatalogName = RunParamEnhancerBuilders.register RunParamEnhancerBuilders.sorterEvalMerge
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


    type configType =
        | Rand_MergeTest_Test
        | Rand_MergeTest_Small
        | Rand_MergeTest_MediumLd
        | Rand_MergeTest_MediumHd
        | Rand_MergeTest_LargeLd


    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_MergeTest_Test, Specs.Rand_MergeTest_Test); 
                        (configType.Rand_MergeTest_Small, Specs.Rand_MergeTest_Small);
                        (configType.Rand_MergeTest_MediumLd, Specs.Rand_MergeTest_MediumLd);
                        (configType.Rand_MergeTest_MediumHd, Specs.Rand_MergeTest_MediumHd);
                        (configType.Rand_MergeTest_LargeLd, Specs.Rand_MergeTest_LargeLd);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterEvalExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType
