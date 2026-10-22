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


module SortableTestsSpecsPrefix =

    module PrefixTest =

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Prefix_Test  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    dataFomatBitv512
                    prefixLib_Prefix24_3a
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Bitv512 prefix sorter test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sortableTests
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.prefix
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sortableTestPrefix
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
                    }
                }
            )

    module Prefix =

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Prefix_24s  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    dataFomatBitv512
                    prefixLib_Prefix24s
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Bitv512 prefix sorter test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sortableTests
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.prefix
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sortableTestPrefix
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
                    }
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Prefix_32  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    dataFomatBitv512
                    prefixLib_Prefix32_4
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Bitv512 prefix sorter test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sortableTests
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.prefix
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sortableTestPrefix
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
                    }
                }
            )

    type configType =
        | Prefix_24s
        | Prefix_32

    let Configs = Map.ofList
                    [
                        (configType.Prefix_24s, Prefix.Prefix_24s);
                        (configType.Prefix_32, Prefix.Prefix_32);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
