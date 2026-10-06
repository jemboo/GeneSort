namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestSpecsMerge =

    module Specs =

        let Merge_Test  (executorType: sortableTestExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SortableTestDbs.Merge.dbName
                    CommonSortableTest.projectName
                    (sprintf @"Merge-Test_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag)
                    "Int8 merge sorter test sets"
                    [
                mergeLib_Merge32s
                dataFormatInt8v512
            ]
                    "sortable-test.merge"
                    RunParamBuilderNames.Filter.mergeDimensionDividesSortingWidth
                    RunParamBuilderNames.Enhancer.sortableTest
                    false
            )


    type configType =
        | Merge_Test
        | Merge_Small

    let Configs = Map.ofList 
                    [ 
                        (configType.Merge_Test, Specs.Merge_Test); 
                    ]

    let getRun (config: configType) (executorType: sortableTestExecutorType) : run =
        let specFunc = Configs.[config]
        specFunc executorType
