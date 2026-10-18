namespace GeneSort.Dispatch.V1.SortableTests

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestsSpecsMerge =

    module Specs =

        let Merge_Test  () : run =
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sortableTests
                    SortableTestsDbs.Merge.dbName
                    CommonSortableTests.projectName
                    (sprintf @"Merge-Test_%s" "GenMerge" |> UMX.tag)
                    "Int8 merge sorter test sets"
                    [
                        mergeLib_Merge32s
                        dataFormatInt8v512
                    ]
                    QueryCatalogNames.sortableTestMerge
                    SetupCatalogNames.merge
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

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
