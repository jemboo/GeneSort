module GeneSort.Dispatch.V1.SorterSgd.RunCfg.p24.Msrs3a.PoolModComp

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
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd
open GeneSort.Dispatch.V1.SorterSgd.RunCfg.p24.Msrs3a.Common




module PoolSelSz128 =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let TestSpec ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [8] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, ["32"] |> List.map string)
                (ParamKeys.modificationRateKey, [0.99;] |> List.map string)
                (ParamKeys.paraRateKey, [0.075; 0.1; 0.125;] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 1] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, ["32";] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Para rate comp for 24pfx3a Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )

module Sz_2048_Of_4096 =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Sz_2048_Of_4096 ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, ["4096";])
                (ParamKeys.mutationModKey, [0 .. 7;] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, [2048;] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Selection size comp for 24pfx3a Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )

module PoolsSelSzTest_2_vs_256 =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let PoolSz_2n256 ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, ["2"; "256";])
                (ParamKeys.mutationModKey, [0 .. 63;] |> List.map string)
                (ParamKeys.selectedSorterCountPerPoolKey, ["64"; "128"; "256"] |> List.map string)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Pool size comp (2 vs 256) for 24pfx3a Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsPoolModComp
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = "expInterval100_L50ss"
                GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
            }
        )
