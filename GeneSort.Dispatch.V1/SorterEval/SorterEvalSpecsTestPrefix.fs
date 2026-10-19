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
            let parameterSpans =
                [
                    rngTypeLcg
                    prefixLib_Prefix24s
                    withMsuf6
                    dataFomatBitv512
                    sorterEvalTypeV2
                    largeSorterCount
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = SorterEvalDbs.Prefix.dbName
                    ProjectName = SorterEvalDbs.projectName
                    RunName = (sprintf @"Prefix_24s_%s" "GenPrefix" |> UMX.tag)
                    Description = "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf6"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterEvalPrefix
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalPrefix
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.prefix
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterEvalPrefix
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                        OutputTypes = [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                    }
                }
            )


        let Prefix_32 () : run =
            let parameterSpans =
                [
                    rngTypeLcg
                    prefixLib_Prefix32_4
                    withMsuf4
                    dataFomatBitv512
                    sorterEvalTypeV2
                    largeSorterCount
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = SorterEvalDbs.Prefix.dbName
                    ProjectName = SorterEvalDbs.projectName
                    RunName = (sprintf @"Prefix_32_%s" "GenPrefix" |> UMX.tag)
                    Description = "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterEvalPrefix
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalPrefix
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.prefix
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterEvalPrefix
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                        OutputTypes = [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
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
