module GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.Msuf6.Simple.MutationRate

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
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
open GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.Msuf6.Simple.Common






module VarModR_32 =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc24.Cc24Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.Msuf6.Simple.SimpleParams.simpleSetupParams


    let runParametersPipelineName = RunParametersPipelineNames.msuf624p3bMutationRate
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdUf6MutationRate
    let setupParametersPipelineName = SetupParametersPipelineNames.sorterSgd
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdUf6MutationRate


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpans =
            GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.Msuf6.Simple.SimpleParams.standardExecutionDefaults
            @ [
                (ParamKeys.generationIntervalCountKey, [2] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [64] |> List.map string)
                (ParamKeys.paraRateKey, [1.001] |> List.map string)
                (ParamKeys.selfSymRateKey, [2.001] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.05; 0.1] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Mutation rate test for Msuf6 24p3b"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans |> List.filter (fun (key, _) -> key <> ParamKeys.sorterEvalMeasureKey)
                    QueryCatalogName = executionQueryCatalogName
                    ParametersPipelineName = runParametersPipelineName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    ParametersPipelineName = setupParametersPipelineName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let WideTest () : run =
        let names = RunNaming.create()
        let parameterSpans =
            GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.Msuf6.Simple.SimpleParams.standardExecutionDefaults
            @ [
                (ParamKeys.generationIntervalCountKey, [10] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [128] |> List.map string)
                (ParamKeys.paraRateKey, [0.1; 0.5; 1.001; 1.5] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.001; 1.5; 2.001; 3.001] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.05] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Wide mutation rate test for Msuf6 24p3b"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans |> List.filter (fun (key, _) -> key <> ParamKeys.sorterEvalMeasureKey)
                    QueryCatalogName = executionQueryCatalogName
                    ParametersPipelineName = runParametersPipelineName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    ParametersPipelineName = setupParametersPipelineName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    let description = "Compare mutation-rate settings for 24-input Msuf63b evolution (VarModR_32)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.cc24.p3b.Msuf6.Simple.MutationRate.VarModR_32", description, channelCount, executionQueryCatalogName, [|
            runExample("Test", Test)
            runExample("WideTest", WideTest)
        |])
