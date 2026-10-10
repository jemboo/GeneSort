namespace GeneSort.Dispatch.V1.SortableTests

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestsSpecsPrefix =

    module Specs =

        let Prefix_Test  (executorType: sortableTestsExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SortableTestsDbs.Prefix.dbNameTest
                    CommonSortableTests.projectName
                    (sprintf @"Prefix24-4b_%s" (SortableTestsExecutorType.toString executorType) |> UMX.tag)
                    "Bitv512 prefix sorter test sets"
                    [
                dataFomatBitv512
                prefixLib_Prefix24_3a
            ]
                    QueryCatalogNames.sortableTestPrefix
                    RunParamBuilderNames.Filter.identity
                    RunParamBuilderNames.Enhancer.sortableTests
                    false
            )



        let Prefix_24s  (executorType: sortableTestsExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SortableTestsDbs.Prefix.dbName
                    CommonSortableTests.projectName
                    (sprintf @"Prefix-24s_%s" (SortableTestsExecutorType.toString executorType) |> UMX.tag)
                    "Bitv512 prefix sorter test sets"
                    [
                dataFomatBitv512
                prefixLib_Prefix24s
            ]
                    QueryCatalogNames.sortableTestPrefix
                    RunParamBuilderNames.Filter.identity
                    RunParamBuilderNames.Enhancer.sortableTests
                    false
            )

        let Prefix_32  (executorType: sortableTestsExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SortableTestsDbs.Prefix.dbName
                    CommonSortableTests.projectName
                    (sprintf @"Prefix-32_%s" (SortableTestsExecutorType.toString executorType) |> UMX.tag)
                    "Bitv512 prefix sorter test sets"
                    [
                dataFomatBitv512
                prefixLib_Prefix32_4
            ]
                    QueryCatalogNames.sortableTestPrefix
                    RunParamBuilderNames.Filter.identity
                    RunParamBuilderNames.Enhancer.sortableTests
                    false
            )


    type configType =
        | Prefix_24s
        | Prefix_32

    let Configs = Map.ofList 
                    [ 
                        (configType.Prefix_24s, Specs.Prefix_24s);
                        (configType.Prefix_32, Specs.Prefix_32);
                    ]

    let getRun (config: configType) (executorType: sortableTestsExecutorType) : run =
        let specFunc = Configs.[config]
        specFunc executorType
