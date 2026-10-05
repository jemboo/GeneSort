namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Sorting
open GeneSort.SortingOps

module SorterEvalSpecsTestPrefix =

    module Specs =

        let Prefix_24s (executorType: sorterEvalExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-eval.prefix"
            databaseName = SorterEvalDbs.Prefix.dbName
            projectName = SorterEvalDbs.projectName
            runName = sprintf @"Prefix_24s_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf6"
            spans = [   
                rngTypeLcg
                prefixLib_Prefix24s
                allSimpleSorterModelTypes
                dataFomatBitv512
                sorterEvalTypeV2
                largeSorterCount
            ]
            filterCatalogName = RunParamBuilderNames.Filter.prefixSorterModelCompatibility
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sorterEvalPrefix
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let Prefix_32 (executorType: sorterEvalExecutorType) : runHostSpec = {
            queryCatalogName = "sorter-eval.prefix"
            databaseName = SorterEvalDbs.Prefix.dbName
            projectName = SorterEvalDbs.projectName
            runName = sprintf @"Prefix_32_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [   
                rngTypeLcg
                prefixLib_Prefix32_4
                allSimpleSorterModelTypes
                dataFomatBitv512
                sorterEvalTypeV2
                largeSorterCount
            ]
            filterCatalogName = RunParamBuilderNames.Filter.prefixSorterModelCompatibility
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sorterEvalPrefix
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }


    type configType =
        | Prefix_24s
        | Prefix_32

    let Configs = Map.ofList 
                    [ 
                        (configType.Prefix_24s, Specs.Prefix_24s);
                        (configType.Prefix_32, Specs.Prefix_32);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterEvalExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType
