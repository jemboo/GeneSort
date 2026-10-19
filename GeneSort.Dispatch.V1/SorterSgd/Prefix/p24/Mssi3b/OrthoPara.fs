module GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Mssi3b.OrthoPara

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
open GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Mssi3b.Common
open GeneSort.Dispatch.V1.SorterSgd
let dbOrthoPara64Name = "OrthoPara64" |> UMX.tag<databaseName>
let dbOrthoPara128Name = "OrthoPara128" |> UMX.tag<databaseName>




module Specs64 =

    let NoMods ()  : run =
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
                DatabaseName = dbOrthoPara64Name
                ProjectName = projName
                RunName = (sprintf @"NoMods%s" "GenStandard" |> UMX.tag)
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    OutputTypes = [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )


    let SymForceDiff1 ()  : run =
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
                DatabaseName = dbOrthoPara64Name
                ProjectName = projName
                RunName = (sprintf @"SymForce_Diff1%s" "GenStandard" |> UMX.tag)
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    OutputTypes = [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )


    let SymForce ()  : run =
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
                DatabaseName = dbOrthoPara64Name
                ProjectName = projName
                RunName = (sprintf @"SymForce%s" "GenStandard" |> UMX.tag)
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    OutputTypes = [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )


module Specs128 =

    let NoMods ()  : run =
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
                DatabaseName = dbOrthoPara128Name
                ProjectName = projName
                RunName = (sprintf @"NoMods%s" "GenStandard" |> UMX.tag)
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    OutputTypes = [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )
