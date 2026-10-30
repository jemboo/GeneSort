namespace GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple

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
open GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.Common


module MutationRate =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc32.Cc32Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.SimpleParams.simpleSetupParams









    module VarModR_32 =

        let runParametersEnhancerName = RunParametersEnhancerNames.msrs32MutationRate
        let executionFunctionName = ExecuteFunctionNames.standard
        let setupFunctionName = SetupCatalogNames.sgdStandard
        let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
        let setupParametersEnhancerName = SetupParametersEnhancerNames.sorterSgd
        let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test () : run =
            let names = RunNaming.create()
            let parameterSpans =
                GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.SimpleParams.standardExecutionDefaults
                @ [
                    (ParamKeys.sorterPoolCountKey, ["16"])
                    (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.paraRateKey, [4.001] |> List.map string)
                    (ParamKeys.selfSymRateKey, [4.001]  |> List.map string)
                    (ParamKeys.modificationRateKey, [0.25;] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                    Setup = {
                        FunctionName = setupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpans
                        QueryCatalogName = setupQueryCatalogName
                        ParametersEnhancerName = setupParametersEnhancerName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let EqualOPS () : run =
            let names = RunNaming.create()
            let parameterSpans =
                GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.SimpleParams.standardExecutionDefaults
                @ [
                    (ParamKeys.sorterPoolCountKey, ["16"])
                    (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.paraRateKey, [3.001; 4.001; 5.001] |> List.map string)
                    (ParamKeys.selfSymRateKey, [3.001; 4.001; 5.001]  |> List.map string)
                    (ParamKeys.modificationRateKey, [0.015; 0.20; 0.25; 0.30; 0.40; 0.50;] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                    Setup = {
                        FunctionName = setupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpans
                        QueryCatalogName = setupQueryCatalogName
                        ParametersEnhancerName = setupParametersEnhancerName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let EqualOPS2 () : run =
            let names = RunNaming.create()
            let parameterSpans =
                GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.SimpleParams.standardExecutionDefaults
                @ [
                    (ParamKeys.sorterPoolCountKey, ["16"])
                    (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.paraRateKey, [1.5; 2.5; 3.5] |> List.map string)
                    (ParamKeys.selfSymRateKey, [4.5; 5.5; 6.5]  |> List.map string)
                    (ParamKeys.modificationRateKey, [0.225; 0.25; 0.275;] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                    Setup = {
                        FunctionName = setupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpans
                        QueryCatalogName = setupQueryCatalogName
                        ParametersEnhancerName = setupParametersEnhancerName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )



        let description = "Compare mutation-rate settings for 32-input Msrs4a evolution (VarModR_32)."



        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msrs.Simple.MutationRate.VarModR_32", description, channelCount, executionQueryCatalogName, [|
                runExample("Test", Test)
                runExample("EqualOPS", EqualOPS)
                runExample("EqualOPS2", EqualOPS2)
            |])

    module VarModR_64 =

        let runParametersEnhancerName = RunParametersEnhancerNames.msrs32MutationRate
        let executionFunctionName = ExecuteFunctionNames.standard
        let setupFunctionName = SetupCatalogNames.sgdStandard
        let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
        let setupParametersEnhancerName = SetupParametersEnhancerNames.sorterSgd
        let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test () : run =
            let names = RunNaming.create()
            let parameterSpans =
                GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.SimpleParams.standardExecutionDefaults
                @ [
                    (ParamKeys.sorterPoolCountKey, ["8"])
                    (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                    (ParamKeys.paraRateKey, [4.001] |> List.map string)
                    (ParamKeys.selfSymRateKey, [4.001]  |> List.map string)
                    (ParamKeys.modificationRateKey, [0.25;] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                    Setup = {
                        FunctionName = setupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpans
                        QueryCatalogName = setupQueryCatalogName
                        ParametersEnhancerName = setupParametersEnhancerName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let WideTest () : run =
            let names = RunNaming.create()
            let parameterSpans =
                GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.SimpleParams.standardExecutionDefaults
                @ [
                    (ParamKeys.sorterPoolCountKey, ["8"])
                    (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                    (ParamKeys.paraRateKey,    [0.05;  0.1;   0.5;   1.001; 1.5;   2.001; 3.001;] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.001; 1.5;   2.001; 3.001; 4.001; 5.001; 6.001]  |> List.map string)
                    (ParamKeys.modificationRateKey, [0.015; 0.20; 0.25; 0.30; 0.40; 0.50; 0.99] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                    Setup = {
                        FunctionName = setupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpans
                        QueryCatalogName = setupQueryCatalogName
                        ParametersEnhancerName = setupParametersEnhancerName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let NarrowTest () : run =
            let names = RunNaming.create()
            let parameterSpans =
                GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.SimpleParams.standardExecutionDefaults
                @ [
                    (ParamKeys.sorterPoolCountKey, ["8"])
                    (ParamKeys.generationIntervalCountKey, [4] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                    (ParamKeys.paraRateKey,    [0.05;  0.1;   0.5;   1.001; ] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.5;   2.001;]  |> List.map string)
                    (ParamKeys.modificationRateKey, [0.25; 0.30; 0.40; 0.50;] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                    Setup = {
                        FunctionName = setupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpans
                        QueryCatalogName = setupQueryCatalogName
                        ParametersEnhancerName = setupParametersEnhancerName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )





        let description = "Compare mutation-rate settings for 32-input Msrs4a evolution (VarModR_64)."



        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msrs.Simple.MutationRate.VarModR_64", description, channelCount, executionQueryCatalogName, [|
                runExample("Test", Test)
                runExample("WideTest", WideTest)
                runExample("NarrowTest", NarrowTest)
            |])

    module MaxModR_32 =

        let runParametersEnhancerName = RunParametersEnhancerNames.msrs32MutationRateMax
        let executionFunctionName = ExecuteFunctionNames.standard
        let setupFunctionName = SetupCatalogNames.sgdStandard
        let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
        let setupParametersEnhancerName = SetupParametersEnhancerNames.sorterSgd
        let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test2 () : run =
            let names = RunNaming.create()
            let parameterSpans =
                GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.SimpleParams.standardExecutionDefaults
                @ [
                    (ParamKeys.sorterPoolCountKey, ["16"])
                    (ParamKeys.modificationRateKey, ["0.99"])
                    (ParamKeys.generationIntervalCountKey, [10] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.paraRateKey, [0.05; 0.075; 0.1; 0.125] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.25; 1.5; 1.75; 2.5] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                    Setup = {
                        FunctionName = setupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpans
                        QueryCatalogName = setupQueryCatalogName
                        ParametersEnhancerName = setupParametersEnhancerName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )




        let description = "Compare mutation-rate settings for 32-input Msrs4a evolution (MaxModR_32)."



        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msrs.Simple.MutationRate.MaxModR_32", description, channelCount, executionQueryCatalogName, [|
                runExample("Test2", Test2)
            |])

    module MaxModR_64 =

        let runParametersEnhancerName = RunParametersEnhancerNames.msrs32MutationRateMax
        let executionFunctionName = ExecuteFunctionNames.standard
        let setupFunctionName = SetupCatalogNames.sgdStandard
        let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
        let setupParametersEnhancerName = SetupParametersEnhancerNames.sorterSgd
        let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let WideTest () : run =
            let names = RunNaming.create()
            let parameterSpans =
                GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple.SimpleParams.standardExecutionDefaults
                @ [
                    (ParamKeys.sorterPoolCountKey, ["8"])
                    (ParamKeys.modificationRateKey, ["0.99"])
                    (ParamKeys.generationIntervalCountKey, [10] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                    (ParamKeys.paraRateKey, [0.025; 0.05; 0.75; 0.1] |> List.map string)
                    (ParamKeys.selfSymRateKey, [0.75; 1.001; 1.5; 2.001] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = executionFunctionName
                        ParameterSpans = parameterSpans
                        QueryCatalogName = executionQueryCatalogName
                        ParametersEnhancerName = runParametersEnhancerName
                    }
                    Setup = {
                        FunctionName = setupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpans
                        QueryCatalogName = setupQueryCatalogName
                        ParametersEnhancerName = setupParametersEnhancerName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )

        let description = "Compare mutation-rate settings for 32-input Msrs4a evolution (MaxModR_64)."



        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msrs.Simple.MutationRate.MaxModR_64", description, channelCount, executionQueryCatalogName, [|
                runExample("WideTest", WideTest)
            |])
