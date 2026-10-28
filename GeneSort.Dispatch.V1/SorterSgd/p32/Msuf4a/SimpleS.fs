module GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.SimpleS

open System.Runtime.CompilerServices
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

    // Uses the catalog defaults: ActivationProbability = 0.5 and ActivatorMutationRate = 0.1.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [256] |> List.map string)
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
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; eight pools of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    EnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32S
                }
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32S
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval100
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )

    // 2000 generations; 4 activation probabilities x 4 activator rates x 2 mutation variants per replica.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.sorterCountPerPoolKey, [32; 64] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [512] |> List.map string)
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
                (ParamKeys.generationIntervalLastKey, [4] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [100] |> List.map string)
                (ParamKeys.activatorMutationRateKey, [0.0; 0.01; 0.02; 0.03] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; eight pools of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    EnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32S
                }
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32S
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval100
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )



module Soss =

    // Uses the catalog defaults: ActivationProbability = 0.5 and ActivatorMutationRate = 0.1.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [256] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 1] |> List.map string)
            ]
        let parameterSpansRun =
            [
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
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; eight pools of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32S
                }
                Execution = {
                    FunctionName = ExecuteFunctionNames.soss
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32S
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
                (ParamKeys.sorterCountPerPoolKey, [64] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [256] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.activationProbabilityKey, [0.95] |> List.map string)
            ]
        let parameterSpansRun =
            [
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
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; eight pools of 32 sorters"
                AllowOverwrite = false
                Setup = {
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32S
                }
                Execution = {
                    FunctionName = ExecuteFunctionNames.soss
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32S
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
                (ParamKeys.sorterCountPerPoolKey, [128] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [512] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.activationProbabilityKey, [0.95] |> List.map string)
            ]
        let parameterSpansRun =
            [
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
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleS Msuf4, width 32, four-stage PrefixA; four pools of 128 sorters"
                AllowOverwrite = false
                Setup = {
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32S
                }
                Execution = {
                    FunctionName = ExecuteFunctionNames.soss
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32S
                }
            } {
                MakerNames = EvolutionMakerNames.sossPrefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval100
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )