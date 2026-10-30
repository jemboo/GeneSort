module GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.SimpleS

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
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

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

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32S
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32S
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard

    // Explicit run defaults: ActivationProbability = 0.5 and ActivatorMutationRate = 0.1.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.activationProbabilityKey, ["0.5"])
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterPoolCountKey, [8] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 1] |> List.map string)
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
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; eight pools of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
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
                GenSaveIntervalsName = GenIntervalNames.uniformInterval100
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )

    // 2000 generations; 4 activation probabilities x 4 activator rates x 2 mutation variants per replica.
    let private activatorRates (names: runNames) sorterPoolCount sorterCountPerPool : run =
        let parameterSpansSetup =
            [
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [sorterCountPerPool] |> List.map string)
                (ParamKeys.sorterPoolCountKey, [sorterPoolCount] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.paraRateKey, [1.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                (ParamKeys.activationProbabilityKey, [0.825; 0.875; 0.925; 0.95] |> List.map string)
            ]
        let parameterSpansRun =
            [
                (ParamKeys.orthoRateKey, ["4.001"])
                (ParamKeys.sorterChildCountKey, ["1"])
                (ParamKeys.distinctSorterHashesKey, ["true"])
                (ParamKeys.prioritizeNewMutantsKey, ["true"])
                (ParamKeys.sortedFractionKey, ["0.99"])
                (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                (ParamKeys.generationIntervalLastKey, [4] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [100] |> List.map string)
                (ParamKeys.activatorMutationRateKey, [0.0; 0.01; 0.02; 0.03] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; eight pools of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
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
                GenSaveIntervalsName = GenIntervalNames.uniformInterval100
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )




    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates32 () = activatorRates (RunNaming.create()) 16 32

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates64 () = activatorRates (RunNaming.create()) 8 64

    let description = "Standard evolutionary runs using SimpleS models with the 32-input Msuf4a configuration."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.Msuf4a.SimpleS.Standard", description, channelCount, executionQueryCatalogName, [|
            runExample("Test", Test)
            runExample("ActivatorRates32", ActivatorRates32)
            runExample("ActivatorRates64", ActivatorRates64)
        |])

module Soss =

    let channelCount = 32<GeneSort.Sorting.channelCount>
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

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

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32S
    let executionFunctionName = ExecuteFunctionNames.soss
    let setupFunctionName = SetupCatalogNames.sgdSoss
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32S
    let setupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    // Explicit run defaults: ActivationProbability = 0.5 and ActivatorMutationRate = 0.1.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.activationProbabilityKey, ["0.5"])
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterPoolCountKey, [8] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 1] |> List.map string)
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
                (ParamKeys.generationIntervalLastKey, [19] |> List.map string)
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
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; eight pools of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
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
                GenSaveIntervalsName = GenIntervalNames.uniformInterval100
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )

    // 1000 generations; 4 activation probabilities x 4 activator rates x 2 mutation variants per replica.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [64] |> List.map string)
                (ParamKeys.sorterPoolCountKey, [4] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.activationProbabilityKey, [0.95] |> List.map string)
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
                (ParamKeys.partitionCountKey, [8; 16;] |> List.map string)
                (ParamKeys.generationsPerSubsetKey, [100;] |> List.map string)
                (ParamKeys.generationsPerMergedSetKey, [100;] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [159] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.paraRateKey, [1.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                (ParamKeys.activatorMutationRateKey, [0.01;] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; eight pools of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
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
                GenSaveIntervalsName = GenIntervalNames.uniformInterval100
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )

    // 1000 generations; 4 activation probabilities x 4 activator rates x 2 mutation variants per replica.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let SossS () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [128] |> List.map string)
                (ParamKeys.sorterPoolCountKey, [4] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.activationProbabilityKey, [0.95] |> List.map string)
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
                (ParamKeys.partitionCountKey, [8] |> List.map string)
                (ParamKeys.generationsPerSubsetKey, [100;] |> List.map string)
                (ParamKeys.generationsPerMergedSetKey, [100;] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [99] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.paraRateKey, [1.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                (ParamKeys.activatorMutationRateKey, [0.0; 0.01;] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; four pools of 128 sorters"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
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
                GenSaveIntervalsName = GenIntervalNames.uniformInterval100
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )

    let description = "Soss evolutionary runs using SimpleS models with the 32-input Msuf4a configuration."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.Msuf4a.SimpleS.Soss", description, channelCount, executionQueryCatalogName, [|
            runExample("Test", Test)
            runExample("ActivatorRates", ActivatorRates)
            runExample("SossS", SossS)
        |])
