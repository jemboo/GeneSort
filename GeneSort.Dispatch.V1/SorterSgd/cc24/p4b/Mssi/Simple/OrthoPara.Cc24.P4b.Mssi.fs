module GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.Simple.OrthoPara

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
open GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.Simple.Common




module OrthoPara64 =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc24.Cc24Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.Simple.SimpleParams.simpleSetupParams


    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let NoMods ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.Simple.SimpleParams.standardExecutionDefaults
            @ [
                (ParamKeys.sorterPoolCountKey, ["16"])
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [1] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                (ParamKeys.paraRateKey, [0.5; 1.01; 1.5; 2.01] |> List.map string)
                (ParamKeys.modificationRateKey, [ 0.09; 0.11; ] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
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
            GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.Simple.SimpleParams.standardExecutionDefaults
            @ [
                (ParamKeys.sorterPoolCountKey, ["16"])
                (ParamKeys.codeModKey, ["SymForce_Diff1"] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                (ParamKeys.paraRateKey, [0.5; 1.01;] |> List.map string)
                (ParamKeys.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
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
            GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.Simple.SimpleParams.standardExecutionDefaults
            @ [
                (ParamKeys.sorterPoolCountKey, ["16"])
                (ParamKeys.codeModKey, ["SymForce"] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                (ParamKeys.paraRateKey, [0.5; 1.01;] |> List.map string)
                (ParamKeys.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )



    let description = "Compare ortho and para mutation settings for 24-input Mssi3b evolution (OrthoPara64)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.cc24.p4b.Mssi.Simple.OrthoPara.OrthoPara64", description, channelCount, executionQueryCatalogName, [|
            runExample("NoMods", NoMods)
            runExample("SymForceDiff1", SymForceDiff1)
            runExample("SymForce", SymForce)
        |])

module OrthoPara128 =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc24.Cc24Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.Simple.SimpleParams.simpleSetupParams


    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMssiOrthoPara
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMssiOrthoPara

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let NoMods ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.Simple.SimpleParams.standardExecutionDefaults
            @ [
                (ParamKeys.sorterPoolCountKey, ["8"])
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [6] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [128] |>  List.map string)
                (ParamKeys.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
                (ParamKeys.modificationRateKey, [0.09; 0.11;] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    let description = "Compare ortho and para mutation settings for 24-input Mssi3b evolution (OrthoPara128)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.cc24.p4b.Mssi.Simple.OrthoPara.OrthoPara128", description, channelCount, executionQueryCatalogName, [|
            runExample("NoMods", NoMods)
        |])
