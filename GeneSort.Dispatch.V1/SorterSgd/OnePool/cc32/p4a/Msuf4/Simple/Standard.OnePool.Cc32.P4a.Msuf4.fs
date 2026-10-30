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

module Standard =

    let executionQueryCatalogName = standardExecutionQueryCatalogName

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let TestStandard ()  : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Rate comp for 32p4a Msuf4"
                AllowOverwrite = false
                Setup = {
                    FunctionName = standardSetupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = SimpleParams.simpleStandardSetupParams @ parameterSpansSetup
                    QueryCatalogName = standardSetupQueryCatalogName
                    ParametersEnhancerName = standardSetupParametersEnhancerName
                }
                Execution = {
                    FunctionName = standardExecutionFunctionName
                    ParameterSpans = []
                    QueryCatalogName = executionQueryCatalogName
                    ParametersEnhancerName = standardRunParametersEnhancerName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            }
        )

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.Standard", description, channelCount, executionQueryCatalogName, [|
            runExample("TestStandard", TestStandard)
        |])
