namespace GeneSort.Dispatch.V1.SorterEval

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
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1


module Standard =


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.sortingWidthKey, [16] |> List.map string)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.sorterCountKey, [10000] |> List.map string)
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
    let Small () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.sortingWidthKey, [4; 5; 6; 7; 8; 9; 10; 11; 12] |> List.map string)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.sorterCountKey, [100000] |> List.map string)
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
    let Medium () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.sortingWidthKey, [14; 16; 18; 20; 22] |> List.map string)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.sorterCountKey, [100000] |> List.map string)
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

    [<RequireQualifiedAccess>]
    type configType =
        | Test
        | Small
        | Medium

    let Configs = Map.ofList
                    [
                        (configType.Test, Test);
                        (configType.Small, Small);
                        (configType.Medium, Medium);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
