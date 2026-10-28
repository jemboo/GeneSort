namespace GeneSort.Dispatch.V1.SorterEval

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1

module Prefix =


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Width24s () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
                [
                    (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                    (ParamKeys.prefixLibIdKey,
                     [prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA]
                     |> List.map PrefixLibId.toString)
                    (ParamKeys.simpleSorterModelTypeKey,
                     [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                     |> List.map SimpleSorterModelType.toString)
                    (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                    (ParamKeys.sorterCountKey, [10000] |> List.map string)
                ]
        let parameterSpansRun =
                [
                    (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
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
                    (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                    (ParamKeys.prefixLibIdKey,
                     [prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA]
                     |> List.map PrefixLibId.toString)
                    (ParamKeys.simpleSorterModelTypeKey,
                     [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                     |> List.map SimpleSorterModelType.toString)
                    (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                    (ParamKeys.sorterCountKey, [10000] |> List.map string)
                ]
        let parameterSpansRun =
                [
                    (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
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
