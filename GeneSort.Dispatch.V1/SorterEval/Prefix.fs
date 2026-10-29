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

    let channelCount = 32<GeneSort.Sorting.channelCount>

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalPrefix
    let executionFunctionName = ExecuteFunctionNames.sorterEval
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterEvalPrefix
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterEvalPrefix
    let setupFunctionName = SetupCatalogNames.prefix
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterEvalPrefix


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Channels24 () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
                [
                    (ParamKeys.excludeSelfCeKey, ["true"])
                    (ParamKeys.collectSortableTestsKey, ["false"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                    (ParamKeys.prefixLibIdKey,
                     [prefixLibId.create 32<channelCount> 4<stageLength> prefixLibVariant.PrefixA]
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
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf6"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Channels32 () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
                [
                    (ParamKeys.excludeSelfCeKey, ["true"])
                    (ParamKeys.collectSortableTestsKey, ["false"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                    (ParamKeys.prefixLibIdKey,
                     [prefixLibId.create 32<channelCount> 4<stageLength> prefixLibVariant.PrefixA]
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
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<RequireQualifiedAccess>]
    type configType =
        | Channels24
        | Channels32

    let Configs = Map.ofList
                    [
                        (configType.Channels24, Channels24);
                        (configType.Channels32, Channels32);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()

    let description = "Evaluate sorting-network prefixes at different widths."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterEval.Prefix", description, channelCount, [|
            runExample("Channels24", Channels24)
            runExample("Channels32", Channels32)
        |])
