module GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Simple.Soss

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



module Pool_32 =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc32.Cc32Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Simple.SimpleParams.simpleSetupParams

    let runParametersPipelineName = RunParametersPipelineNames.sossMsuf32
    let executionFunctionName = ExecuteFunctionNames.soss
    let setupFunctionName = SetupCatalogNames.sgdSoss
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let setupParametersPipelineName = SetupParametersPipelineNames.sossMsuf32
    let setupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Gens100 ()  : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                (ParamKeys.sorterPoolCountKey, [8] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 7] |> List.map string)
            ]
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.longSossScheduleExecutionParams
            @ [
                (ParamKeys.partitionCountKey, [1; 4; 8;] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [6] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Soss rate comparison for Msuf32p4a"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    ParametersPipelineName = setupParametersPipelineName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    ParametersPipelineName = runParametersPipelineName
                }
            } {
                MakerNames = EvolutionMakerNames.sossPrefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )



    let description = "Soss evolutionary runs for the 32-input Msuf4a configuration (Pool_32)."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msuf4.Simple.Soss.Pool_32", description, channelCount, executionQueryCatalogName, [|
            runExample("Gens100", Gens100)
        |])

module Pool_512 =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc32.Cc32Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Simple.SimpleParams.simpleSetupParams

    let runParametersPipelineName = RunParametersPipelineNames.sossMsuf32
    let executionFunctionName = ExecuteFunctionNames.soss
    let setupFunctionName = SetupCatalogNames.sgdSoss
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let setupParametersPipelineName = SetupParametersPipelineNames.sossMsuf32
    let setupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let V2a ()  : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.sorterCountPerPoolKey, [512] |>  List.map string)
                (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
            ]
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.sossMutationExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.longSossScheduleExecutionParams
            @ [
                (ParamKeys.partitionCountKey, [4] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [3] |> List.map string)
                (ParamKeys.paraRateKey,    [0.85; 0.95; 1.05; 1.15;] |> List.map string)
                (ParamKeys.selfSymRateKey, [0.75; 1.25; 1.5; 1.75;]  |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.015; 0.02; 0.025;] |> List.map string)
                (ParamKeys.modificationRateKey, [0.020; 0.025; 0.030; 0.035; 0.040;] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Soss rate comparison for Msuf32p4a"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    ParametersPipelineName = setupParametersPipelineName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    ParametersPipelineName = runParametersPipelineName
                }
            } {
                MakerNames = EvolutionMakerNames.sossPrefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    let description = "Soss evolutionary runs for the 32-input Msuf4a configuration (Pool_512)."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msuf4.Simple.Soss.Pool_512", description, channelCount, executionQueryCatalogName, [|
            runExample("V2a", V2a)
        |])
