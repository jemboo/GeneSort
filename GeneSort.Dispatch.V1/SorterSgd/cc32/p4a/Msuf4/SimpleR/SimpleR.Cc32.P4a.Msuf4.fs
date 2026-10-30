module GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleR

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

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.simpleRSetupParams

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
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.standardSetupDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.defaultSetupParams
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.simpleRExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.adjustmentExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.activationExecutionParams
            @ [
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
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
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )

    // 100 generations; 3 bounds x 3 activator rates x 4 mutation variants per replica.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.standardSetupDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.runSetupDefaults
            @ [
                (ParamKeys.aMaxKey, [1; 3; 7] |> List.map string)
            ]
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.simpleRExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.adjustmentExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.activationSweepExecutionParams
            @ [
                (ParamKeys.generationIntervalLastKey, [9] |> List.map string)
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
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )


    let description = "Standard evolutionary runs using SimpleR models with the 32-input Msuf4a configuration."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleR.Standard", description, channelCount, executionQueryCatalogName, [|
            runExample("Test", Test)
            runExample("ActivatorRates", ActivatorRates)
        |])

module Soss =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc32.Cc32Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.simpleRSetupParams

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
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.defaultSetupParams
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.simpleRExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sossScheduleExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.activationExecutionParams
            @ [
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
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
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )

    // 100 generations; 3 bounds x 3 activator rates x 4 mutation variants per replica.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.runSetupDefaults
            @ [
                (ParamKeys.aMaxKey, [1; 3; 7] |> List.map string)
            ]
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.simpleRExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sossScheduleExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleRParams.activationSweepExecutionParams
            @ [
                (ParamKeys.generationIntervalLastKey, [9] |> List.map string)
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
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )

    let description = "Soss evolutionary runs using SimpleR models with the 32-input Msuf4a configuration."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleR.Soss", description, channelCount, executionQueryCatalogName, [|
            runExample("Test", Test)
            runExample("ActivatorRates", ActivatorRates)
        |])
