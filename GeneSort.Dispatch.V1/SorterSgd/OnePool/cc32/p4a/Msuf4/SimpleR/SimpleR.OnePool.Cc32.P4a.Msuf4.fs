namespace GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR
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

module SimpleR =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.Cc32Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = SimpleRParams.simpleRSetupParams

    let standardRunParametersPipelineName = RunParametersPipelineNames.evoMsuf32
    let sossRunParametersPipelineName = RunParametersPipelineNames.sossMsuf32
    let standardExecutionFunctionName = ExecuteFunctionNames.standard
    let standardSetupFunctionName = SetupCatalogNames.sgdStandard
    let sossExecutionFunctionName = ExecuteFunctionNames.soss
    let sossSetupFunctionName = SetupCatalogNames.sgdSoss
    let standardExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
    let sossExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
    let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
    let standardSetupParametersPipelineName = SetupParametersPipelineNames.standardMsuf32
    let sossSetupParametersPipelineName = SetupParametersPipelineNames.sossMsuf32
    let standardSetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
    let sossSetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    // AMax belongs to setup; the independent activator mutation rate belongs to execution.
    let aMax = SimpleRParams.aMax
    let activatorMutationRate = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleRParams.activatorMutationRate

    let description = "Single-pool SimpleR runs and their append and condense continuations."

    module Standard =

        let executionQueryCatalogName = standardExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Standard () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                Msuf4Params.standardSetupDefaults
                @ [
                    (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                    (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleRParams.simpleRExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleRParams.activationExecutionParams
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OnePool SimpleR generation and evolution"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = standardSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = standardSetupQueryCatalogName
                        ParametersPipelineName = standardSetupParametersPipelineName
                    }
                    Execution = {
                        FunctionName = standardExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        ParametersPipelineName = standardRunParametersPipelineName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleR.Standard", description, channelCount, executionQueryCatalogName, [|
                runExample("Standard", Standard)
            |])

    module Soss =

        let executionQueryCatalogName = sossExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Soss () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                ]
            let parameterSpansRun =
                GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleRParams.simpleRExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
                @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sossScheduleExecutionParams
                @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleRParams.activationExecutionParams
                @ [
                    (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OnePool SimpleR generation and evolution"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = sossSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = sossSetupQueryCatalogName
                        ParametersPipelineName = sossSetupParametersPipelineName
                    }
                    Execution = {
                        FunctionName = sossExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        ParametersPipelineName = sossRunParametersPipelineName
                    }
                } {
                    MakerNames = EvolutionMakerNames.sossPrefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleR.Soss", description, channelCount, executionQueryCatalogName, [|
                runExample("Soss", Soss)
            |])

    module StandardAppend =

        let executionQueryCatalogName = standardAppendExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let StandardAppend () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Standard.Standard ()
                    GenerationInterval = 0
                    SourceMutationMods = [0 .. 3]
                    SelectedSorterPoolCount = 4
                }
                ExecutionParameterSpans = [ParamKeys.generationIntervalLastKey, ["1"]]
            } |> EvoAppend.createStandard

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleR.StandardAppend", description, channelCount, executionQueryCatalogName, [|
                runExample("StandardAppend", StandardAppend)
            |])

    module SossAppend =

        let executionQueryCatalogName = sossAppendExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let SossCondense () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Soss.Soss ()
                    GenerationInterval = 0
                    SourceMutationMods = [0 .. 3]
                    SelectedSorterPoolCount = 2
                }
                ExecutionParameterSpans = [
                    ParamKeys.generationIntervalLastKey, ["1"]
                    ParamKeys.mutationModKey, [0; 1] |> List.map string
                ]
            } |> EvoCondense.createSoss

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleR.SossAppend", description, channelCount, executionQueryCatalogName, [|
                runExample("SossCondense", SossCondense)
            |])
