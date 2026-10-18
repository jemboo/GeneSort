namespace GeneSort.Dispatch.V1.SortableTests

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestsSpecsPrefix =

    module Specs =

        let Prefix_Test  () : run =
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sortableTests
                    SortableTestsDbs.Prefix.dbNameTest
                    CommonSortableTests.projectName
                    (sprintf @"Prefix24-4b_%s" "GenPrefix" |> UMX.tag)
                    "Bitv512 prefix sorter test sets"
                    [
                        dataFomatBitv512
                        prefixLib_Prefix24_3a
                    ]
                    QueryCatalogNames.sortableTestPrefix
                    SetupCatalogNames.prefix
                    RunParamBuilderNames.Filter.identity
                    RunParamBuilderNames.Enhancer.sortableTests
                    false
            )



        let Prefix_24s  () : run =
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sortableTests
                    SortableTestsDbs.Prefix.dbName
                    CommonSortableTests.projectName
                    (sprintf @"Prefix-24s_%s" "GenPrefix" |> UMX.tag)
                    "Bitv512 prefix sorter test sets"
                    [
                        dataFomatBitv512
                        prefixLib_Prefix24s
                    ]
                    QueryCatalogNames.sortableTestPrefix
                    SetupCatalogNames.prefix
                    RunParamBuilderNames.Filter.identity
                    RunParamBuilderNames.Enhancer.sortableTests
                    false
            )

        let Prefix_32  () : run =
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sortableTests
                    SortableTestsDbs.Prefix.dbName
                    CommonSortableTests.projectName
                    (sprintf @"Prefix-32_%s" "GenPrefix" |> UMX.tag)
                    "Bitv512 prefix sorter test sets"
                    [
                        dataFomatBitv512
                        prefixLib_Prefix32_4
                    ]
                    QueryCatalogNames.sortableTestPrefix
                    SetupCatalogNames.prefix
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

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
