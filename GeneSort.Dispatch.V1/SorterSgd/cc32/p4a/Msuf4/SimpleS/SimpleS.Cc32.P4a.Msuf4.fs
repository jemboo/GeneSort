module GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS

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

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc32.Cc32Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.simpleSSetupParams

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
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.standardSetupDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.defaultSetupParams
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.simpleSExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.adjustmentExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.activationExecutionParams
            @ [
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
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
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.standardSetupDefaults
            @ [
                (ParamKeys.sorterCountPerPoolKey, [sorterCountPerPool] |> List.map string)
                (ParamKeys.sorterPoolCountKey, [sorterPoolCount] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.activationProbabilityKey, [0.825; 0.875; 0.925; 0.95] |> List.map string)
            ]
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.simpleSExecutionParams
            @ [
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
        GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Standard", description, channelCount, executionQueryCatalogName, [|
            runExample("Test", Test)
            runExample("ActivatorRates32", ActivatorRates32)
            runExample("ActivatorRates64", ActivatorRates64)
        |])

module Soss =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc32.Cc32Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.simpleSSetupParams

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
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.defaultSetupParams
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.simpleSExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sossScheduleExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.activationExecutionParams
            @ [
                (ParamKeys.generationIntervalLastKey, [19] |> List.map string)
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
                (ParamKeys.sorterCountPerPoolKey, [64] |> List.map string)
                (ParamKeys.sorterPoolCountKey, [4] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.activationProbabilityKey, [0.95] |> List.map string)
            ]
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.simpleSExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.longSossScheduleExecutionParams
            @ [
                (ParamKeys.partitionCountKey, [8; 16;] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [159] |> List.map string)
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
                (ParamKeys.sorterCountPerPoolKey, [128] |> List.map string)
                (ParamKeys.sorterPoolCountKey, [4] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.activationProbabilityKey, [0.95] |> List.map string)
            ]
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleSParams.simpleSExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.longSossScheduleExecutionParams
            @ [
                (ParamKeys.partitionCountKey, [8] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [99] |> List.map string)
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
        GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Soss", description, channelCount, executionQueryCatalogName, [|
            runExample("Test", Test)
            runExample("ActivatorRates", ActivatorRates)
            runExample("SossS", SossS)
        |])
