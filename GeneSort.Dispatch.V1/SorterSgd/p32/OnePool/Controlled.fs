namespace GeneSort.Dispatch.V1.SorterSgd.p32.OnePool
open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.p32.OnePool.Common

module Controlled =

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

    let standardExecutionEnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32
    let sossExecutionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32
    let standardExecutionFunctionName = ExecuteFunctionNames.standard
    let standardSetupFunctionName = SetupCatalogNames.sgdStandard
    let sossExecutionFunctionName = ExecuteFunctionNames.soss
    let sossSetupFunctionName = SetupCatalogNames.sgdSoss
    let standardExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
    let sossExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let standardSetupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32
    let sossSetupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
    let standardSetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
    let sossSetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    let replicaCount = 5
    let generationCount = 100
    let aMax = 3
    let activationProbability = float aMax / (2.0 * float aMax + 1.0)
    let activatorMutationRate = 0.1

    // Only 0 -> 1 and 1 -> 0 change activity; each has probability rate / 2.
    // This matches the uniform seed distribution, not an arbitrary evolved distribution.
    let matchedFlipRate = activatorMutationRate / float (2 * aMax + 1)

    let description = "Controlled single-pool comparisons of evolutionary models and settings."

    module Standard =

        let executionQueryCatalogName = standardExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let StandardSimple () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                [
                    (ParamKeys.orthoRateKey, ["4.001"])
                    (ParamKeys.sorterChildCountKey, ["1"])
                    (ParamKeys.distinctSorterHashesKey, ["true"])
                    (ParamKeys.prioritizeNewMutantsKey, ["true"])
                    (ParamKeys.sortedFractionKey, ["0.99"])
                    (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                    (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = standardSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = standardSetupQueryCatalogName
                        EnhancerFunctionName = standardSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = standardExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = standardExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let StandardR () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.aMaxKey, [aMax] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                [
                    (ParamKeys.orthoRateKey, ["4.001"])
                    (ParamKeys.sorterChildCountKey, ["1"])
                    (ParamKeys.distinctSorterHashesKey, ["true"])
                    (ParamKeys.prioritizeNewMutantsKey, ["true"])
                    (ParamKeys.sortedFractionKey, ["0.99"])
                    (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                    (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                    (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = standardSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = standardSetupQueryCatalogName
                        EnhancerFunctionName = standardSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = standardExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = standardExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let CompareStandardS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                [
                    (ParamKeys.orthoRateKey, ["4.001"])
                    (ParamKeys.sorterChildCountKey, ["1"])
                    (ParamKeys.distinctSorterHashesKey, ["true"])
                    (ParamKeys.prioritizeNewMutantsKey, ["true"])
                    (ParamKeys.sortedFractionKey, ["0.99"])
                    (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                    (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                    (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = standardSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = standardSetupQueryCatalogName
                        EnhancerFunctionName = standardSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = standardExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = standardExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let MatchedStandardS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                [
                    (ParamKeys.orthoRateKey, ["4.001"])
                    (ParamKeys.sorterChildCountKey, ["1"])
                    (ParamKeys.distinctSorterHashesKey, ["true"])
                    (ParamKeys.prioritizeNewMutantsKey, ["true"])
                    (ParamKeys.sortedFractionKey, ["0.99"])
                    (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                    (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                    (ParamKeys.activatorMutationRateKey, [matchedFlipRate] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = standardSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = standardSetupQueryCatalogName
                        EnhancerFunctionName = standardSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = standardExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = standardExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.p32.OnePool.Controlled.Standard", description, channelCount, executionQueryCatalogName, [|
                runExample("StandardSimple", StandardSimple)
                runExample("StandardR", StandardR)
                runExample("CompareStandardS", CompareStandardS)
                runExample("MatchedStandardS", MatchedStandardS)
            |])

    module Soss =

        let executionQueryCatalogName = sossExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let SossSimple () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
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
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = sossSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = sossSetupQueryCatalogName
                        EnhancerFunctionName = sossSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = sossExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = sossExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.sossPrefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let SossR () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.aMaxKey, [aMax] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
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
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                    (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = sossSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = sossSetupQueryCatalogName
                        EnhancerFunctionName = sossSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = sossExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = sossExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.sossPrefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let CompareSossS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
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
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                    (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = sossSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = sossSetupQueryCatalogName
                        EnhancerFunctionName = sossSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = sossExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = sossExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.sossPrefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let MatchedSossS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
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
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                    (ParamKeys.activatorMutationRateKey, [matchedFlipRate] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = sossSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = sossSetupQueryCatalogName
                        EnhancerFunctionName = sossSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = sossExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = sossExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.sossPrefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.p32.OnePool.Controlled.Soss", description, channelCount, executionQueryCatalogName, [|
                runExample("SossSimple", SossSimple)
                runExample("SossR", SossR)
                runExample("CompareSossS", CompareSossS)
                runExample("MatchedSossS", MatchedSossS)
            |])
