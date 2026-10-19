namespace GeneSort.Dispatch.V1.SortableTests

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestsSpecsMerge =

    module Specs =

        let Merge_Test  () : run =
            let parameterSpans =
                [
                    mergeLib_Merge32s
                    dataFormatInt8v512
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = SortableTestsDbs.Merge.dbName
                    ProjectName = CommonSortableTests.projectName
                    RunName = (sprintf @"Merge-Test_%s" "GenMerge" |> UMX.tag)
                    Description = "Int8 merge sorter test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sortableTests
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sortableTestMerge
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.merge
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sortableTestMerge
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
                        OutputTypes = []
                    }
                }
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
