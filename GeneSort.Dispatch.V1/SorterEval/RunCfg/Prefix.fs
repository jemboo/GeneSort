namespace GeneSort.Dispatch.V1.SorterEval.RunCfg

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams

module Prefix =


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Width24s () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
                [
                    rngTypeLcg
                    prefixLib_Prefix32_4
                    withMsuf4
                    dataFomatBitv512
                    largeSorterCount
                ]
        let parameterSpansRun =
                [
                    sorterEvalTypeV2
                ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf6"
                AllowOverwrite = false
                Setup = {
                    FunctionName = SetupCatalogNames.prefix
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = QueryCatalogNamesSetup.sorterEvalPrefix
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEvalPrefix
                }
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterEval
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = QueryCatalogNamesRun.sorterEvalPrefix
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalPrefix
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Width32 () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
                [
                    rngTypeLcg
                    prefixLib_Prefix32_4
                    withMsuf4
                    dataFomatBitv512
                    largeSorterCount
                ]
        let parameterSpansRun =
                [
                    sorterEvalTypeV2
                ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Setup = {
                    FunctionName = SetupCatalogNames.prefix
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = QueryCatalogNamesSetup.sorterEvalPrefix
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEvalPrefix
                }
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterEval
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = QueryCatalogNamesRun.sorterEvalPrefix
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalPrefix
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
