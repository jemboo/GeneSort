namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams


module SorterEvalSpecsRs =

    module Specs =

        let Rand_Test (executorType: sorterEvalExecutorType)  : runHostSpec = {
            queryCatalogName = "sorter-eval.standard"
            databaseName = SorterEvalDbs.Standard.dbName
            projectName = SorterEvalDbs.projectName
            runName = sprintf @"Rand-Test16_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalTypeV2
                sortingWidth16
                allSimpleSorterModelTypes
                largeSorterCount
            ]
            filterCatalogName = RunParamBuilderNames.Filter.standardSorterModelCompatibility
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sorterEvalStandard
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }

        let Rand_Small (executorType: sorterEvalExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-eval.standard"
            databaseName = SorterEvalDbs.Standard.dbName
            projectName = SorterEvalDbs.projectName
            runName = sprintf @"Rand-Small_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalTypeV2
                smallSortingWidths
                allSimpleSorterModelTypes
                extraLargeSorterCount
            ]
            filterCatalogName = RunParamBuilderNames.Filter.standardSorterModelCompatibility
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sorterEvalStandard
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_Medium (executorType: sorterEvalExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-eval.standard"
            databaseName = SorterEvalDbs.Standard.dbName
            projectName = SorterEvalDbs.projectName
            runName = sprintf @"Rand-Medium_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalTypeV2
                mediumSortingWidths
                allSimpleSorterModelTypes
                extraLargeSorterCount
            ]
            filterCatalogName = RunParamBuilderNames.Filter.standardSorterModelCompatibility
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sorterEvalStandard
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

    type configType =
        | Rand_Test
        | Rand_Small
        | Rand_Medium

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_Medium, Specs.Rand_Medium);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterEvalExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType
