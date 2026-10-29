module GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.SimpleR

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1

module Standard =

    let channelCount = 32<GeneSort.Sorting.channelCount>

    let moduleParameters =
        [
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
            ParamKeys.prefixLibIdKey, [GeneSort.SortingLib.Sorter.prefixLibId.create channelCount 4<GeneSort.Sorting.stageLength> GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixA |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32R
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32R
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard

    // Explicit run defaults: AMax = 3 and ActivatorMutationRate = 0.1.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.aMaxKey, ["3"])
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.paraRateKey, [1.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
            ]
        let parameterSpansRun =
            [
                (ParamKeys.orthoRateKey, ["4.001"])
                (ParamKeys.sorterChildCountKey, ["1"])
                (ParamKeys.distinctSorterHashesKey, ["true"])
                (ParamKeys.prioritizeNewMutantsKey, ["true"])
                (ParamKeys.sortedFractionKey, ["0.99"])
                (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                (ParamKeys.activatorMutationRateKey, ["0.1"])
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleR Msuf4, width 32, four-stage PrefixA; one pool of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = moduleParameters @ parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )

    // 100 generations; 3 bounds x 3 activator rates x 4 mutation variants per replica.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.paraRateKey, [1.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                (ParamKeys.aMaxKey, [1; 3; 7] |> List.map string)
            ]
        let parameterSpansRun =
            [
                (ParamKeys.orthoRateKey, ["4.001"])
                (ParamKeys.sorterChildCountKey, ["1"])
                (ParamKeys.distinctSorterHashesKey, ["true"])
                (ParamKeys.prioritizeNewMutantsKey, ["true"])
                (ParamKeys.sortedFractionKey, ["0.99"])
                (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                (ParamKeys.generationIntervalLastKey, [9] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                (ParamKeys.activatorMutationRateKey, [0.05; 0.1; 0.2] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleR Msuf4, width 32, four-stage PrefixA; one pool of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = moduleParameters @ parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )


    let description = "Standard evolutionary runs using SimpleR models with the 32-input Msuf4a configuration."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.Msuf4a.SimpleR.Standard", description, channelCount, [|
            runExample("Test", Test)
            runExample("ActivatorRates", ActivatorRates)
        |])

module Soss =

    let channelCount = 32<GeneSort.Sorting.channelCount>

    let moduleParameters =
        [
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
            ParamKeys.prefixLibIdKey, [GeneSort.SortingLib.Sorter.prefixLibId.create channelCount 4<GeneSort.Sorting.stageLength> GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixA |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32R
    let executionFunctionName = ExecuteFunctionNames.soss
    let setupFunctionName = SetupCatalogNames.sgdSoss
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32R
    let setupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    // Explicit run defaults: AMax = 3 and ActivatorMutationRate = 0.1.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.aMaxKey, ["3"])
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
            ]
        let parameterSpansRun =
            [
                (ParamKeys.orthoRateKey, ["4.001"])
                (ParamKeys.sorterChildCountKey, ["1"])
                (ParamKeys.distinctSorterHashesKey, ["true"])
                (ParamKeys.prioritizeNewMutantsKey, ["true"])
                (ParamKeys.sortedFractionKey, ["0.99"])
                (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                (ParamKeys.activatorMutationRateKey, ["0.1"])
                (ParamKeys.seedSossKey, [1234UL] |> List.map string)
                (ParamKeys.partitionCountKey, [2] |> List.map string)
                (ParamKeys.generationsPerSubsetKey, [10] |> List.map string)
                (ParamKeys.generationsPerMergedSetKey, [20] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.paraRateKey, [1.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleR Msuf4, width 32, four-stage PrefixA; one pool of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = moduleParameters @ parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.sossPrefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )

    // 100 generations; 3 bounds x 3 activator rates x 4 mutation variants per replica.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.aMaxKey, [1; 3; 7] |> List.map string)
            ]
        let parameterSpansRun =
            [
                (ParamKeys.orthoRateKey, ["4.001"])
                (ParamKeys.sorterChildCountKey, ["1"])
                (ParamKeys.distinctSorterHashesKey, ["true"])
                (ParamKeys.prioritizeNewMutantsKey, ["true"])
                (ParamKeys.sortedFractionKey, ["0.99"])
                (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                (ParamKeys.seedSossKey, [1234UL] |> List.map string)
                (ParamKeys.partitionCountKey, [2] |> List.map string)
                (ParamKeys.generationsPerSubsetKey, [10] |> List.map string)
                (ParamKeys.generationsPerMergedSetKey, [20] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [9] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.paraRateKey, [1.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                (ParamKeys.activatorMutationRateKey, [0.05; 0.1; 0.2] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleR Msuf4, width 32, four-stage PrefixA; one pool of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = moduleParameters @ parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.sossPrefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )

    let description = "Soss evolutionary runs using SimpleR models with the 32-input Msuf4a configuration."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.Msuf4a.SimpleR.Soss", description, channelCount, [|
            runExample("Test", Test)
            runExample("ActivatorRates", ActivatorRates)
        |])
