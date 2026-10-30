namespace GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple
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
open GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4
open GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.Common

module Controlled =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.Cc32Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = SimpleParams.simpleSetupParams

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
    let activatorMutationRate = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.controlledActivatorMutationRate

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
                Msuf4Params.standardSetupDefaults
                @ [
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
                @ [
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
                Msuf4Params.standardSetupDefaults
                @ [
                    (ParamKeys.aMaxKey, [aMax] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.controlledActivationExecutionParams
                @ [
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
        let CompareStandardS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                Msuf4Params.standardSetupDefaults
                @ [
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.controlledActivationExecutionParams
                @ [
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
        let MatchedStandardS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                Msuf4Params.standardSetupDefaults
                @ [
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
                @ [
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
            GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.Controlled.Standard", description, channelCount, executionQueryCatalogName, [|
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
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
                @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sossScheduleExecutionParams
                @ [
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
                    (ParamKeys.aMaxKey, [aMax] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
                @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sossScheduleExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.controlledActivationExecutionParams
                @ [
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
        let CompareSossS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
                @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sossScheduleExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.controlledActivationExecutionParams
                @ [
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
        let MatchedSossS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                    (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                    (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
                @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sossScheduleExecutionParams
                @ [
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
            GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.Controlled.Soss", description, channelCount, executionQueryCatalogName, [|
                runExample("SossSimple", SossSimple)
                runExample("SossR", SossR)
                runExample("CompareSossS", CompareSossS)
                runExample("MatchedSossS", MatchedSossS)
            |])
