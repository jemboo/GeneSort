module GeneSort.Dispatch.V1.SorterSgd.p24.Mssi3b.OrthoPara

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
open GeneSort.Dispatch.V1.SorterSgd.p24.Mssi3b.Common




module OrthoPara64 =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let NoMods ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [1] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                (ParamKeys.paraRateKey, [0.5; 1.01; 1.5; 2.01] |> List.map string)
                (ParamKeys.modificationRateKey, [ 0.09; 0.11; ] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, [64;] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                }
                Setup = {
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let SymForceDiff1 ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.codeModKey, ["SymForce_Diff1"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                (ParamKeys.paraRateKey, [0.5; 1.01;] |> List.map string)
                (ParamKeys.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, [64;] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                }
                Setup = {
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let SymForce ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.codeModKey, ["SymForce"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                (ParamKeys.paraRateKey, [0.5; 1.01;] |> List.map string)
                (ParamKeys.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, [64;] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                }
                Setup = {
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )


module OrthoPara128 =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let NoMods ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [6] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [128] |>  List.map string)
                (ParamKeys.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
                (ParamKeys.modificationRateKey, [0.09; 0.11;] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, [128;] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                }
                Setup = {
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )
