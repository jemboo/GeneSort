namespace GeneSort.Dispatch.V1.SorterSgd.p32.OnePool
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
open GeneSort.Dispatch.V1.SorterSgd.p32.OnePool.Common

module Standard =

    let executionQueryCatalogName = standardExecutionQueryCatalogName

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let TestStandard ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                (ParamKeys.orthoRateKey, ["4.001"])
                (ParamKeys.sorterChildCountKey, ["1"])
                (ParamKeys.distinctSorterHashesKey, ["true"])
                (ParamKeys.prioritizeNewMutantsKey, ["true"])
                (ParamKeys.sortedFractionKey, ["0.99"])
                (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                (ParamKeys.channelCountKey, [string (int channelCount)])
                (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                (ParamKeys.paraRateKey,    [1.15;] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75;]  |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020;] |> List.map string)
                (ParamKeys.modificationRateKey, [0.035;]  |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2;] |> List.map (MutatorVariant.toString))
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
                    ParameterSpans = onePoolSetupParams @ parameterSpans
                    QueryCatalogName = standardSetupQueryCatalogName
                    EnhancerFunctionName = standardSetupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = standardExecutionFunctionName
                    ParameterSpans = []
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
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.OnePool.Standard", description, channelCount, executionQueryCatalogName, [|
            runExample("TestStandard", TestStandard)
        |])
