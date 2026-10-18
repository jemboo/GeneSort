module GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Msrs3a.PoolModComp

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
let dbName_Sz_2048_Of_4096 = "Sz_2048_Of_4096" |> UMX.tag<databaseName>
let dbNamePools4096_2_vs_256 = "PoolsSelSzTest_2_vs_256" |> UMX.tag<databaseName>
let dbNamePools4096_4096 = "Pools4096_4096" |> UMX.tag<databaseName>
let dbNamePoolSz128 = "PoolSelSz128" |> UMX.tag<databaseName>




module Specs =

    let TestSpec ()  : run =
        let parameterSpans =
            [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [8] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["32"] |> List.map string)
                (runParameters.modificationRateKey, [0.99;] |> List.map string)
                (runParameters.paraRateKey, [0.075; 0.1; 0.125;] |> List.map string)
                (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
                (runParameters.mutationModKey, [0 .. 1] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, ["32";] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = dbNamePoolSz128
                ProjectName = projName
                RunName = (sprintf @"PoolSz32_Mod_Testc%s" "GenStandard" |> UMX.tag)
                Description = "Para rate comp for 24pfx3a Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNames.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNames.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    OutputTypes = [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )


    let Sz_2048_Of_4096 ()  : run =
        let parameterSpans =
            [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [12] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["4096";])
                (runParameters.mutationModKey, [0 .. 7;] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, [2048;] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = dbName_Sz_2048_Of_4096
                ProjectName = projName
                RunName = (sprintf @"Sz_2048_Of_4096_%s" "GenStandard" |> UMX.tag)
                Description = "Selection size comp for 24pfx3a Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNames.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNames.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    OutputTypes = [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )


    let PoolSz_2n256 ()  : run =
        let parameterSpans =
            [
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [5] |> List.map string)
                (runParameters.sorterCountPerPoolKey, ["2"; "256";])
                (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, ["64"; "128"; "256"] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = dbNamePools4096_2_vs_256
                ProjectName = projName
                RunName = (sprintf @"PoolSz_2_vs_256_%s" "GenStandard" |> UMX.tag)
                Description = "Pool size comp (2 vs 256) for 24pfx3a Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNames.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNames.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    OutputTypes = [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )
