namespace GeneSort.Dispatch.V1.SortableTests

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1


module Merge =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
    let executionFunctionName = ExecuteFunctionNames.sortableTests
    let executionQueryCatalogName = QueryCatalogNamesRun.sortableTestMerge


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let All_16  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 16<channelCount> 2<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 16<channelCount> 4<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 16<channelCount> 8<mergeDimension> sorterLibVariant.VariantA
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.DirectRun (
            DirectRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "16 channels, 2, 4, 8 way merges"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let HiMD_24  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 24<channelCount> 6<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 24<channelCount> 8<mergeDimension> sorterLibVariant.VariantA
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.DirectRun (
            DirectRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "24 channels, 6, 8 way merges"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let LowMD_24  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 24<channelCount> 2<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 24<channelCount> 2<mergeDimension> sorterLibVariant.VariantB
                     mergeLibId.create 24<channelCount> 3<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 24<channelCount> 4<mergeDimension> sorterLibVariant.VariantA
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.DirectRun (
            DirectRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "24 channels, 2, 3, 4 way merges; variants A and B for 2 way"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let HiMD_48  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 48<channelCount> 6<mergeDimension> sorterLibVariant.VariantA
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.DirectRun (
            DirectRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "48 channels, 6 way merges"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let LowMD_48  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 48<channelCount> 2<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 48<channelCount> 2<mergeDimension> sorterLibVariant.VariantB
                     mergeLibId.create 48<channelCount> 3<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 48<channelCount> 3<mergeDimension> sorterLibVariant.VariantB
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.DirectRun (
            DirectRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "48 channels, 2, 3 way merges; variants A and B"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let HiMD_64  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 64<channelCount> 4<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 64<channelCount> 4<mergeDimension> sorterLibVariant.VariantB
                     mergeLibId.create 64<channelCount> 8<mergeDimension> sorterLibVariant.VariantA
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.DirectRun (
            DirectRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "64 channels, 4, 8 way merges; variants A and B for 4 way"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let LowMD_64  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 64<channelCount> 2<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 64<channelCount> 2<mergeDimension> sorterLibVariant.VariantB
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.DirectRun (
            DirectRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "64 channels, 2 way merges; variants A and B"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let HiMD_32  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 32<channelCount> 4<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 32<channelCount> 8<mergeDimension> sorterLibVariant.VariantA
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.DirectRun (
            DirectRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "32 channels, 4 and 8 way merges"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let LowMD_32  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 32<channelCount> 2<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 32<channelCount> 2<mergeDimension> sorterLibVariant.VariantB
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.DirectRun (
            DirectRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "32 channels, 2 way merge, 2 Variants"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            }
        )

    [<RequireQualifiedAccess>]
    type configType =
        | All_16
        | HiMD_24
        | LowMD_24
        | HiMD_48
        | LowMD_48
        | HiMD_64
        | LowMD_64
        | HiMD_32
        | LowMD_32

    let Configs = Map.ofList
                    [
                        (configType.All_16, All_16);
                        (configType.HiMD_24, HiMD_24);
                        (configType.LowMD_24, LowMD_24);
                        (configType.HiMD_48, HiMD_48);
                        (configType.LowMD_48, LowMD_48);
                        (configType.HiMD_64, HiMD_64);
                        (configType.LowMD_64, LowMD_64);
                        (configType.HiMD_32, HiMD_32);
                        (configType.LowMD_32, LowMD_32)
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()

    let description = "Int8 tests for merge networks."

    let runType =
        GeneSort.Dispatch.V1.runType("SortableTests.Merge", description, [|
            runExample("All_16", All_16)
            runExample("HiMD_24", HiMD_24)
            runExample("LowMD_24", LowMD_24)
            runExample("HiMD_48", HiMD_48)
            runExample("LowMD_48", LowMD_48)
            runExample("HiMD_64", HiMD_64)
            runExample("LowMD_64", LowMD_64)
            runExample("LowMD_32", LowMD_32)
            runExample("HiMD_32", HiMD_32)
        |])
