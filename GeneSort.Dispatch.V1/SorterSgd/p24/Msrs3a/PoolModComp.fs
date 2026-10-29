module GeneSort.Dispatch.V1.SorterSgd.p24.Msrs3a.PoolModComp

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.p24.Msrs3a.Common




module PoolSelSz128 =

    let channelCount = 24<GeneSort.Sorting.channelCount>

    let moduleParameters =
        [
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.simpleSorterModelTypeKey, ["Msrs"]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
            ParamKeys.prefixLibIdKey, [GeneSort.SortingLib.Sorter.prefixLibId.create channelCount 4<GeneSort.Sorting.stageLength> GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixA |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]


    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsPoolModComp
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsPoolModComp

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let TestSpec ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.sorterCountPerPoolSetKey, ["512"])
                (ParamKeys.orthoRateKey, ["4.001"])
                (ParamKeys.sorterChildCountKey, ["1"])
                (ParamKeys.distinctSorterHashesKey, ["true"])
                (ParamKeys.prioritizeNewMutantsKey, ["true"])
                (ParamKeys.sortedFractionKey, ["0.99"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [8] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, ["32"] |> List.map string)
                (ParamKeys.modificationRateKey, [0.99;] |> List.map string)
                (ParamKeys.paraRateKey, [0.075; 0.1; 0.125;] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 1] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, ["32";] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Para rate comp for 24pfx3a Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )


    let description = "Compare pool and selection settings for 24-input Msrs3a evolution (PoolSelSz128)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p24.Msrs3a.PoolModComp.PoolSelSz128", description, channelCount, [|
            runExample("TestSpec", TestSpec)
        |])

module Sz_2048_Of_4096 =

    let channelCount = 24<GeneSort.Sorting.channelCount>

    let moduleParameters =
        [
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.simpleSorterModelTypeKey, ["Msrs"]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
            ParamKeys.prefixLibIdKey, [GeneSort.SortingLib.Sorter.prefixLibId.create channelCount 4<GeneSort.Sorting.stageLength> GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixA |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]


    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsPoolModComp
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsPoolModComp

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Sz_2048_Of_4096 ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.sorterCountPerPoolSetKey, ["4096"])
                (ParamKeys.orthoRateKey, ["4.001"])
                (ParamKeys.sorterChildCountKey, ["1"])
                (ParamKeys.distinctSorterHashesKey, ["true"])
                (ParamKeys.prioritizeNewMutantsKey, ["true"])
                (ParamKeys.sortedFractionKey, ["0.99"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, ["4096";])
                (ParamKeys.mutationModKey, [0 .. 7;] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, [2048;] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Selection size comp for 24pfx3a Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )


    let description = "Compare pool and selection settings for 24-input Msrs3a evolution (Sz_2048_Of_4096)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p24.Msrs3a.PoolModComp.Sz_2048_Of_4096", description, channelCount, [|
            runExample("Sz_2048_Of_4096", Sz_2048_Of_4096)
        |])

module PoolsSelSzTest_2_vs_256 =

    let channelCount = 24<GeneSort.Sorting.channelCount>

    let moduleParameters =
        [
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.simpleSorterModelTypeKey, ["Msrs"]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
            ParamKeys.prefixLibIdKey, [GeneSort.SortingLib.Sorter.prefixLibId.create channelCount 4<GeneSort.Sorting.stageLength> GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixA |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]


    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsPoolModComp
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsPoolModComp

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let PoolSz_2n256 ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.sorterCountPerPoolSetKey, ["512"])
                (ParamKeys.orthoRateKey, ["4.001"])
                (ParamKeys.sorterChildCountKey, ["1"])
                (ParamKeys.distinctSorterHashesKey, ["true"])
                (ParamKeys.prioritizeNewMutantsKey, ["true"])
                (ParamKeys.sortedFractionKey, ["0.99"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, ["2"; "256";])
                (ParamKeys.mutationModKey, [0 .. 63;] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, ["64"; "128"; "256"] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Pool size comp (2 vs 256) for 24pfx3a Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    let description = "Compare pool and selection settings for 24-input Msrs3a evolution (PoolsSelSzTest_2_vs_256)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p24.Msrs3a.PoolModComp.PoolsSelSzTest_2_vs_256", description, channelCount, [|
            runExample("PoolSz_2n256", PoolSz_2n256)
        |])
