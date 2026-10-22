namespace GeneSort.Dispatch.V1.SortableTests

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestsSpecsMerge =

    module Merge =

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Merge_Test  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    mergeLib_Merge32s
                    dataFormatInt8v512
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
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
                    }
                }
            )

    type configType =
        | Merge_Test
        | Merge_Small

    let Configs = Map.ofList
                    [
                        (configType.Merge_Test, Merge.Merge_Test);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
