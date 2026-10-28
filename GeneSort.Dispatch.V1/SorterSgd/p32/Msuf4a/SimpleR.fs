module GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.SimpleR

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

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32R
    let executionFunctionName = ExecuteFunctionNames.standard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32R
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard

    // Uses the catalog defaults: AMax = 3 and ActivatorMutationRate = 0.1.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
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
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleR Msuf4, width 32, four-stage PrefixA; one pool of 32 sorters"
                AllowOverwrite = false
                Setup = {
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
                (ParamKeys.generationIntervalLastKey, [9] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                (ParamKeys.activatorMutationRateKey, [0.05; 0.1; 0.2] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleR Msuf4, width 32, four-stage PrefixA; one pool of 32 sorters"
                AllowOverwrite = false
                Setup = {
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
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )

module Soss =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32R
    let executionFunctionName = ExecuteFunctionNames.soss
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32R
    let setupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    // Uses the catalog defaults: AMax = 3 and ActivatorMutationRate = 0.1.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
            ]
        let parameterSpansRun =
            [
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
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleR Msuf4, width 32, four-stage PrefixA; one pool of 32 sorters"
                AllowOverwrite = false
                Setup = {
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
                (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.aMaxKey, [1; 3; 7] |> List.map string)
            ]
        let parameterSpansRun =
            [
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
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "SimpleR Msuf4, width 32, four-stage PrefixA; one pool of 32 sorters"
                AllowOverwrite = false
                Setup = {
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
            } {
                MakerNames = EvolutionMakerNames.sossPrefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )
