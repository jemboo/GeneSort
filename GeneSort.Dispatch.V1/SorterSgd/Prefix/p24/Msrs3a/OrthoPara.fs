module GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Msrs3a.OrthoPara

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Msrs3a.Common
open GeneSort.Dispatch.V1.SorterSgd
let dbOrthoPara32Name = "OrthoPara32" |> UMX.tag<databaseName>







module Specs =

    let PickMode2_2 ()  : run =
        let parameterSpans =
            [
                (ParamKeys.codeModKey, ["PickMode2_2"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                (ParamKeys.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, [32;] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = dbOrthoPara32Name
                ProjectName = projName
                RunName = (sprintf @"PickMode2_2_%s" "GenStandard" |> UMX.tag)
                Description = "OrthroPara rate comp for 24pfx3a Msrs, PickMode2_2"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aOrthoPara
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )

    let NoMods ()  : run =
        let parameterSpans =
            [
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [7] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                (ParamKeys.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, [32;] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = dbOrthoPara32Name
                ProjectName = projName
                RunName = (sprintf @"NoMods%s" "GenStandard" |> UMX.tag)
                Description = "OrthroPara rate comp for 24pfx3a Msrs, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aOrthoPara
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )
