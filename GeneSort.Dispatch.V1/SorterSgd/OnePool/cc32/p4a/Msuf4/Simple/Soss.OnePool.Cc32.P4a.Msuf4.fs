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
open GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.Common

module Soss =

    let executionQueryCatalogName = sossExecutionQueryCatalogName

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let TestSoss ()  : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
            ]
        let parameterSpansRun =
            GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SimpleParams.simpleExecutionParams
            @ GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.mutationExecutionDefaults
            @ GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sossScheduleExecutionParams
            @ [
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Soss rate comparison for Msuf32p4a"
                AllowOverwrite = false
                Setup = {
                    FunctionName = sossSetupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = onePoolSetupParams @ parameterSpansSetup
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
        GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.Soss", description, channelCount, executionQueryCatalogName, [|
            runExample("TestSoss", TestSoss)
        |])
