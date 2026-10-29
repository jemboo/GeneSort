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

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalStandard
    let executionFunctionName = ExecuteFunctionNames.sorterEval
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
    let setupFunctionName = SetupCatalogNames.standard
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard


    let private makeSmall (channelCount: int<channelCount>) (names: runNames) : run =
        let parameterSpans =
            [
                (ParamKeys.excludeSelfCeKey, ["true"])
                (ParamKeys.collectSortableTestsKey, ["false"])
                (ParamKeys.sortableDataFormatKey, ["BitVector512"])
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.channelCountKey, [int channelCount] |> List.map string)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.filter (SimpleSorterModelType.supportsChannelCount (int channelCount))
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.sorterCountKey, [100000] |> List.map string)
            ]
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    let private makeMedium (channelCount: int<channelCount>) (names: runNames) : run =
        let parameterSpans =
            [
                (ParamKeys.excludeSelfCeKey, ["true"])
                (ParamKeys.collectSortableTestsKey, ["false"])
                (ParamKeys.sortableDataFormatKey, ["BitVector512"])
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.channelCountKey, [int channelCount] |> List.map string)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.filter (SimpleSorterModelType.supportsChannelCount (int channelCount))
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.sorterCountKey, [100000] |> List.map string)
            ]
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    module Channels4 =

        let channelCount = 4<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_4 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_4

        let Configs = Map.ofList [
            configType.Small_4, Small_4
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels4", description, channelCount, [|
                runExample("Small_4", Small_4)
            |])

    module Channels5 =

        let channelCount = 5<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_5 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_5

        let Configs = Map.ofList [
            configType.Small_5, Small_5
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels5", description, channelCount, [|
                runExample("Small_5", Small_5)
            |])

    module Channels6 =

        let channelCount = 6<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_6 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_6

        let Configs = Map.ofList [
            configType.Small_6, Small_6
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels6", description, channelCount, [|
                runExample("Small_6", Small_6)
            |])

    module Channels7 =

        let channelCount = 7<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_7 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_7

        let Configs = Map.ofList [
            configType.Small_7, Small_7
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels7", description, channelCount, [|
                runExample("Small_7", Small_7)
            |])

    module Channels8 =

        let channelCount = 8<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_8 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_8

        let Configs = Map.ofList [
            configType.Small_8, Small_8
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels8", description, channelCount, [|
                runExample("Small_8", Small_8)
            |])

    module Channels9 =

        let channelCount = 9<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_9 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_9

        let Configs = Map.ofList [
            configType.Small_9, Small_9
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels9", description, channelCount, [|
                runExample("Small_9", Small_9)
            |])

    module Channels10 =

        let channelCount = 10<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_10 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_10

        let Configs = Map.ofList [
            configType.Small_10, Small_10
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels10", description, channelCount, [|
                runExample("Small_10", Small_10)
            |])

    module Channels11 =

        let channelCount = 11<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_11 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_11

        let Configs = Map.ofList [
            configType.Small_11, Small_11
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels11", description, channelCount, [|
                runExample("Small_11", Small_11)
            |])

    module Channels12 =

        let channelCount = 12<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_12 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_12

        let Configs = Map.ofList [
            configType.Small_12, Small_12
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels12", description, channelCount, [|
                runExample("Small_12", Small_12)
            |])

    module Channels14 =

        let channelCount = 14<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Medium_14 () : run =
            makeMedium channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Medium_14

        let Configs = Map.ofList [
            configType.Medium_14, Medium_14
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels14", description, channelCount, [|
                runExample("Medium_14", Medium_14)
            |])

    module Channels16 =

        let channelCount = 16<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test ()  : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.excludeSelfCeKey, ["true"])
                    (ParamKeys.collectSortableTestsKey, ["false"])
                    (ParamKeys.sortableDataFormatKey, ["BitVector512"])
                    (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                    (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                    (ParamKeys.channelCountKey, [int channelCount] |> List.map string)
                    (ParamKeys.simpleSorterModelTypeKey,
                     [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                     |> List.filter (SimpleSorterModelType.supportsChannelCount (int channelCount))
                 |> List.map SimpleSorterModelType.toString)
                    (ParamKeys.sorterCountKey, [10000] |> List.map string)
                ]
            run.PreparedRun (
                SimpleRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = executionEnhancerFunctionName
                    }
                    Setup = {
                        FunctionName = setupFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = setupQueryCatalogName
                        EnhancerFunctionName = setupEnhancerFunctionName
                    }
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Medium_16 () : run =
            makeMedium channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Test
            | Medium_16

        let Configs = Map.ofList [
            configType.Test, Test
            configType.Medium_16, Medium_16
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels16", description, channelCount, [|
                runExample("Test", Test)
                runExample("Medium_16", Medium_16)
            |])

    module Channels18 =

        let channelCount = 18<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Medium_18 () : run =
            makeMedium channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Medium_18

        let Configs = Map.ofList [
            configType.Medium_18, Medium_18
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels18", description, channelCount, [|
                runExample("Medium_18", Medium_18)
            |])

    module Channels20 =

        let channelCount = 20<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Medium_20 () : run =
            makeMedium channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Medium_20

        let Configs = Map.ofList [
            configType.Medium_20, Medium_20
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels20", description, channelCount, [|
                runExample("Medium_20", Medium_20)
            |])

    module Channels22 =

        let channelCount = 22<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Medium_22 () : run =
            makeMedium channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Medium_22

        let Configs = Map.ofList [
            configType.Medium_22, Medium_22
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks with standard test configurations."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Standard.Channels22", description, channelCount, [|
                runExample("Medium_22", Medium_22)
            |])
