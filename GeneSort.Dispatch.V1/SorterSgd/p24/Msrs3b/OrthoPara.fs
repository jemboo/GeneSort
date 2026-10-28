module GeneSort.Dispatch.V1.SorterSgd.p24.Msrs3b.OrthoPara

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
open GeneSort.Dispatch.V1.SorterSgd.p24.Msrs3b.Common




module OrthoPara32 =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3bOrthoPara
    let executionFunctionName = ExecuteFunctionNames.standard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsOrthoPara
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsOrthoPara

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let NoMods ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                (ParamKeys.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, [32;] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Msrs, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    ParameterSpans = parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p24.Msrs3b.OrthoPara.OrthoPara32", [|
            runExample("NoMods", NoMods)
        |])
