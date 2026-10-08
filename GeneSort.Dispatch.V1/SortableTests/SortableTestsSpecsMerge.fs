namespace GeneSort.Dispatch.V1.SortableTests

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestsSpecsMerge =

    module Specs =

        let Merge_Test  (executorType: sortableTestsExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SortableTestsDbs.Merge.dbName
                    CommonSortableTests.projectName
                    (sprintf @"Merge-Test_%s" (SortableTestsExecutorType.toString executorType) |> UMX.tag)
                    "Int8 merge sorter test sets"
                    [
                mergeLib_Merge32s
                dataFormatInt8v512
            ]
                    QueryCatalogNames.sortableTestMerge
                    RunParamBuilderNames.Filter.mergeDimensionDividesSortingWidth
                    RunParamBuilderNames.Enhancer.sortableTests
                    false
            )


    type configType =
        | Merge_Test
        | Merge_Small

    let Configs = Map.ofList 
                    [ 
                        (configType.Merge_Test, Specs.Merge_Test); 
                    ]

    let getRun (config: configType) (executorType: sortableTestsExecutorType) : run =
        let specFunc = Configs.[config]
        specFunc executorType
