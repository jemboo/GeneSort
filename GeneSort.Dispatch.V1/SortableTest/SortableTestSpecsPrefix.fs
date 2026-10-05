namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestSpecsPrefix =

    module Specs =

        let Prefix_24s  (executorType: sortableTestExecutorType) : runHostSpec = {
            queryCatalogName = "sortable-test.prefix"
            databaseName = SortableTestDbs.Prefix.dbName
            projectName = CommonSortableTest.projectName
            runName = sprintf @"Prefix-24s_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Bitv512 prefix sorter test sets"
            spans = [
                dataFomatBitv512
                prefixLib_Prefix24s
            ]
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sortableTest
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }

        let Prefix_32  (executorType: sortableTestExecutorType) : runHostSpec = {
            queryCatalogName = "sortable-test.prefix"
            databaseName = SortableTestDbs.Prefix.dbName
            projectName = CommonSortableTest.projectName
            runName = sprintf @"Prefix-32_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Bitv512 prefix sorter test sets"
            spans = [
                dataFomatBitv512
                prefixLib_Prefix32_4
            ]
            filterCatalogName = RunParamBuilderNames.Filter.identity
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sortableTest
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


    type configType =
        | Prefix_24s
        | Prefix_32

    let Configs = Map.ofList 
                    [ 
                        (configType.Prefix_24s, Specs.Prefix_24s);
                        (configType.Prefix_32, Specs.Prefix_32);
                    ]

    let getRunHostSpec (config: configType) (executorType: sortableTestExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType
