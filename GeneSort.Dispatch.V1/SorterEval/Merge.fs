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

module Merge =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
    let executionFunctionName = ExecuteFunctionNames.sorterEval
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterEvalMerge
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
    let setupFunctionName = SetupCatalogNames.merge
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterEvalMerge


    let private makeSmall (channelCount: int<channelCount>) (names: runNames) : run =
        let parameterSpansSetup =
            [
                (ParamKeys.excludeSelfCeKey, ["true"])
                (ParamKeys.collectSortableTestsKey, ["false"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.filter (SimpleSorterModelType.supportsChannelCount (int channelCount))
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.mergeLibIdKey,
                    [ for width in [int channelCount] do
                        for dimension in [2; 3; 4; 6; 8] do
                            if width % dimension = 0 then
                                yield mergeLibId.create (UMX.tag width) (UMX.tag dimension) sorterLibVariant.VariantA ]
                    |> List.map MergeLibId.toString)
                (ParamKeys.sorterCountKey, [100000] |> List.map string)
            ]
        let parameterSpansRun = []
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    let private makeTest (channelCount: int<channelCount>) (names: runNames) : run =
        let parameterSpansSetup =
            [
                (ParamKeys.excludeSelfCeKey, ["true"])
                (ParamKeys.collectSortableTestsKey, ["false"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create channelCount 2<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create channelCount 2<mergeDimension> sorterLibVariant.VariantB
                     mergeLibId.create channelCount 4<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create channelCount 8<mergeDimension> sorterLibVariant.VariantA
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.filter (SimpleSorterModelType.supportsChannelCount (int channelCount))
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.sorterCountKey, [100] |> List.map string)
            ]
        let parameterSpansRun = []
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    let private makeMediumLd (channelCount: int<channelCount>) (names: runNames) : run =
        let parameterSpansSetup =
            [
                (ParamKeys.excludeSelfCeKey, ["true"])
                (ParamKeys.collectSortableTestsKey, ["false"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.filter (SimpleSorterModelType.supportsChannelCount (int channelCount))
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.mergeLibIdKey,
                    [ for width in [int channelCount] do
                        for dimension in [2; 3; 4] do
                            if width % dimension = 0 then
                                yield mergeLibId.create (UMX.tag width) (UMX.tag dimension) sorterLibVariant.VariantA ]
                    |> List.map MergeLibId.toString)
                (ParamKeys.sorterCountKey, [10000] |> List.map string)
            ]
        let parameterSpansRun = []
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    let private makeMediumHd (channelCount: int<channelCount>) (names: runNames) : run =
        let parameterSpansSetup =
            [
                (ParamKeys.excludeSelfCeKey, ["true"])
                (ParamKeys.collectSortableTestsKey, ["false"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.filter (SimpleSorterModelType.supportsChannelCount (int channelCount))
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.mergeLibIdKey,
                    [mergeLibId.create channelCount 6<mergeDimension> sorterLibVariant.VariantA]
                    |> List.map MergeLibId.toString)
                (ParamKeys.sorterCountKey, [10000] |> List.map string)
            ]
        let parameterSpansRun = []
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    let private makeLargeLd (channelCount: int<channelCount>) (names: runNames) : run =
        let parameterSpansSetup =
            [
                (ParamKeys.excludeSelfCeKey, ["true"])
                (ParamKeys.collectSortableTestsKey, ["false"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.filter (SimpleSorterModelType.supportsChannelCount (int channelCount))
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.mergeLibIdKey,
                    [ for width in [int channelCount] do
                        yield mergeLibId.create (UMX.tag width) 2<mergeDimension> sorterLibVariant.VariantA ]
                    |> List.map MergeLibId.toString)
                (ParamKeys.sorterCountKey, [10000] |> List.map string)
            ]
        let parameterSpansRun = []
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    module Channels16 =

        let channelCount = 16<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_16 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_16

        let Configs = Map.ofList [
            configType.Small_16, Small_16
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels16", description, channelCount, [|
                runExample("Small_16", Small_16)
            |])

    module Channels18 =

        let channelCount = 18<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_18 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_18

        let Configs = Map.ofList [
            configType.Small_18, Small_18
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels18", description, channelCount, [|
                runExample("Small_18", Small_18)
            |])

    module Channels24 =

        let channelCount = 24<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_24 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_24

        let Configs = Map.ofList [
            configType.Small_24, Small_24
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels24", description, channelCount, [|
                runExample("Small_24", Small_24)
            |])

    module Channels32 =

        let channelCount = 32<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test () : run =
            makeTest channelCount (RunNaming.create())

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_32 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Test
            | Small_32

        let Configs = Map.ofList [
            configType.Test, Test
            configType.Small_32, Small_32
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels32", description, channelCount, [|
                runExample("Test", Test)
                runExample("Small_32", Small_32)
            |])

    module Channels36 =

        let channelCount = 36<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_36 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_36

        let Configs = Map.ofList [
            configType.Small_36, Small_36
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels36", description, channelCount, [|
                runExample("Small_36", Small_36)
            |])

    module Channels48 =

        let channelCount = 48<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_48 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_48

        let Configs = Map.ofList [
            configType.Small_48, Small_48
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels48", description, channelCount, [|
                runExample("Small_48", Small_48)
            |])

    module Channels64 =

        let channelCount = 64<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Small_64 () : run =
            makeSmall channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | Small_64

        let Configs = Map.ofList [
            configType.Small_64, Small_64
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels64", description, channelCount, [|
                runExample("Small_64", Small_64)
            |])

    module Channels96 =

        let channelCount = 96<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let MediumLd_96 () : run =
            makeMediumLd channelCount (RunNaming.create())

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let MediumHd () : run =
            makeMediumHd channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | MediumLd_96
            | MediumHd

        let Configs = Map.ofList [
            configType.MediumLd_96, MediumLd_96
            configType.MediumHd, MediumHd
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels96", description, channelCount, [|
                runExample("MediumLd_96", MediumLd_96)
                runExample("MediumHd", MediumHd)
            |])

    module Channels128 =

        let channelCount = 128<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let MediumLd_128 () : run =
            makeMediumLd channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | MediumLd_128

        let Configs = Map.ofList [
            configType.MediumLd_128, MediumLd_128
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels128", description, channelCount, [|
                runExample("MediumLd_128", MediumLd_128)
            |])

    module Channels192 =

        let channelCount = 192<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let LargeLd_192 () : run =
            makeLargeLd channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | LargeLd_192

        let Configs = Map.ofList [
            configType.LargeLd_192, LargeLd_192
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels192", description, channelCount, [|
                runExample("LargeLd_192", LargeLd_192)
            |])

    module Channels256 =

        let channelCount = 256<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let LargeLd_256 () : run =
            makeLargeLd channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | LargeLd_256

        let Configs = Map.ofList [
            configType.LargeLd_256, LargeLd_256
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels256", description, channelCount, [|
                runExample("LargeLd_256", LargeLd_256)
            |])

    module Channels512 =

        let channelCount = 512<channelCount>


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let LargeLd_512 () : run =
            makeLargeLd channelCount (RunNaming.create())

        [<RequireQualifiedAccess>]
        type configType =
            | LargeLd_512

        let Configs = Map.ofList [
            configType.LargeLd_512, LargeLd_512
        ]

        let getRun (config: configType) () : run = Configs.[config] ()

        let description = "Evaluate sorting networks against merge test inputs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterEval.Merge.Channels512", description, channelCount, [|
                runExample("LargeLd_512", LargeLd_512)
            |])
