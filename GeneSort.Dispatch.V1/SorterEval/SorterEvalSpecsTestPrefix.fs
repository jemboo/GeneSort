namespace GeneSort.Dispatch.V1.SorterEval

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Sorting
open GeneSort.SortingOps

module SorterEvalSpecsTestPrefix =

    module Specs =

        let Prefix_24s () : run =
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
                    DatabaseName = SorterEvalDbs.Prefix.dbName
                    ProjectName = SorterEvalDbs.projectName
                    RunName = (sprintf @"Prefix_24s_%s" "GenPrefix" |> UMX.tag)
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


        let Prefix_32 () : run =
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
                    DatabaseName = SorterEvalDbs.Prefix.dbName
                    ProjectName = SorterEvalDbs.projectName
                    RunName = (sprintf @"Prefix_32_%s" "GenPrefix" |> UMX.tag)
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
