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



module Prefix =

    let runParametersEnhancerName = RunParametersEnhancerNames.sortableTests
    let executionFunctionName = ExecuteFunctionNames.sortableTests
    let executionQueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix

    module Channels24 =

        let executionQueryCatalogName = executionQueryCatalogName

        let channelCount = 24<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Channels24  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                    (ParamKeys.prefixLibIdKey,
                     [
                         prefixLibId.create channelCount 4<stageLength> prefixLibVariant.PrefixA
                         prefixLibId.create channelCount 4<stageLength> prefixLibVariant.PrefixB
                         prefixLibId.create channelCount 3<stageLength> prefixLibVariant.PrefixA
                         prefixLibId.create channelCount 3<stageLength> prefixLibVariant.PrefixB
                     ]
                     |> List.map PrefixLibId.toString)
                ]
            run.DirectRun (
                DirectRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "24 channels, 3 and 4 stages, variants A and B; BitVector512 test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TwoStage_24  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                    (ParamKeys.prefixLibIdKey,
                     [
                         prefixLibId.create channelCount 2<stageLength> prefixLibVariant.PrefixA
                         prefixLibId.create channelCount 2<stageLength> prefixLibVariant.PrefixB
                     ]
                     |> List.map PrefixLibId.toString)
                ]
            run.DirectRun (
                DirectRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "24 channels, 2 stages, variants A and B; BitVector512 test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let ThreeStage_24  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                    (ParamKeys.prefixLibIdKey,
                     [
                         prefixLibId.create channelCount 3<stageLength> prefixLibVariant.PrefixA
                         prefixLibId.create channelCount 3<stageLength> prefixLibVariant.PrefixB
                     ]
                     |> List.map PrefixLibId.toString)
                ]
            run.DirectRun (
                DirectRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "24 channels, 3 stages, variants A and B; BitVector512 test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                }
            )

        [<RequireQualifiedAccess>]
        type configType =
            | Channels24
            | TwoStage_24
            | ThreeStage_24

        let Configs = Map.ofList [
            configType.Channels24, Channels24
            configType.TwoStage_24, TwoStage_24
            configType.ThreeStage_24, ThreeStage_24
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Prefix filtered Bitv512 tests."

        let runType =
            GeneSort.Dispatch.V1.runType("SortableTests.Prefix.Channels24", description, channelCount, executionQueryCatalogName, [|
                runExample("Channels24", Channels24)
                runExample("TwoStage_24", TwoStage_24)
                runExample("ThreeStage_24", ThreeStage_24)
            |])

    module Channels28 =

        let executionQueryCatalogName = executionQueryCatalogName

        let channelCount = 28<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let FourStage_28  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                    (ParamKeys.prefixLibIdKey,
                     [prefixLibId.create channelCount 4<stageLength> prefixLibVariant.PrefixA]
                     |> List.map PrefixLibId.toString)
                ]
            run.DirectRun (
                DirectRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "28 channels, 4 stages, variant A; BitVector512 test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                }
            )

        [<RequireQualifiedAccess>]
        type configType =
            | FourStage_28

        let Configs = Map.ofList [
            configType.FourStage_28, FourStage_28
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Prefix filtered Bitv512 tests."

        let runType =
            GeneSort.Dispatch.V1.runType("SortableTests.Prefix.Channels28", description, channelCount, executionQueryCatalogName, [|
                runExample("FourStage_28", FourStage_28)
            |])

    module Channels32 =

        let executionQueryCatalogName = executionQueryCatalogName

        let channelCount = 32<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let ThreeStage_32  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                    (ParamKeys.prefixLibIdKey,
                     [
                         prefixLibId.create channelCount 3<stageLength> prefixLibVariant.PrefixA
                     ]
                     |> List.map PrefixLibId.toString)
                ]
            run.DirectRun (
                DirectRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "32 channels, 3 stages, variant A; BitVector512 test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let FourStage_32  () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                    (ParamKeys.prefixLibIdKey,
                     [prefixLibId.create channelCount 4<stageLength> prefixLibVariant.PrefixA]
                     |> List.map PrefixLibId.toString)
                ]
            run.DirectRun (
                DirectRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "32 channels, 4 stages, variant A; BitVector512 test sets"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                }
            )

        [<RequireQualifiedAccess>]
        type configType =
            | ThreeStage_32
            | FourStage_32

        let Configs = Map.ofList [
            configType.ThreeStage_32, ThreeStage_32
            configType.FourStage_32, FourStage_32
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Prefix filtered Bitv512 tests."

        let runType =
            GeneSort.Dispatch.V1.runType("SortableTests.Prefix.Channels32", description, channelCount, executionQueryCatalogName, [|
                runExample("ThreeStage_32", ThreeStage_32)
                runExample("FourStage_32", FourStage_32)
            |])
