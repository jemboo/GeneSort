namespace GeneSort.Dispatch.V1.SorterEval

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams


module SorterEvalSpecsRs =

    module Standard =

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Rand_Test ()  : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    rngTypeLcg
                    sorterEvalTypeV2
                    sortingWidth16
                    withMsuf4
                    largeSorterCount
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalStandard
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                    }
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Rand_Small () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    rngTypeLcg
                    sorterEvalTypeV2
                    smallSortingWidths
                    withMsuf4
                    extraLargeSorterCount
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalStandard
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                    }
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Rand_Medium () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    rngTypeLcg
                    sorterEvalTypeV2
                    mediumSortingWidths
                    withMsuf4
                    extraLargeSorterCount
                ]
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalStandard
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                    }
                }
            )

    type configType =
        | Rand_Test
        | Rand_Small
        | Rand_Medium

    let Configs = Map.ofList
                    [
                        (configType.Rand_Test, Standard.Rand_Test);
                        (configType.Rand_Small, Standard.Rand_Small);
                        (configType.Rand_Medium, Standard.Rand_Medium);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
