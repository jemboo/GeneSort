namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestSpecsMerge =

    module Specs =

        let Merge_Test  (executorType: sortableTestExecutorType) : runHostSpec = {
            queryCatalogName = "sortable-test.merge"
            databaseName = SortableTestDbs.Merge.dbName
            projectName = CommonSortableTest.projectName
            runName = sprintf @"Merge-Test_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Int8 merge sorter test sets"
            spans = [
                mergeLib_Merge32s
                dataFormatInt8v512
            ]
            filterCatalogName = RunParamBuilderNames.Filter.mergeDimensionDividesSortingWidth
            enhancerCatalogName = RunParamBuilderNames.Enhancer.sortableTest
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


    type configType =
        | Merge_Test
        | Merge_Small

    let Configs = Map.ofList 
                    [ 
                        (configType.Merge_Test, Specs.Merge_Test); 
                    ]

    let getRunHostSpec (config: configType) (executorType: sortableTestExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType
