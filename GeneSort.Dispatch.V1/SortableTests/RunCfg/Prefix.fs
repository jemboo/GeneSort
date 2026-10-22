namespace GeneSort.Dispatch.V1.SortableTests.RunCfg

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



module PrefixTest =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test  () : run =
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
    let Width24s  () : run =
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
    let Width32  () : run =
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

    [<RequireQualifiedAccess>]
    type configType =
        | Width24s
        | Width32

    let Configs = Map.ofList
                    [
                        (configType.Width24s, Width24s);
                        (configType.Width32, Width32);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
