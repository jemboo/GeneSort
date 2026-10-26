namespace GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msrs4a

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
open GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msrs4a.Common


module MutationRate =








    module VarModR_32 =



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.generationCurrentKey, [0] |> List.map string)
                    (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.paraRateKey, [4.001] |> List.map string)
                    (ParamKeys.selfSymRateKey, [4.001]  |> List.map string)
                    (ParamKeys.mutationModKey, [0] |> List.map string)
                    (ParamKeys.selectedSorterCountPerPoolKey, [32;] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.25;] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
                ]
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs32MutationRate
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
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
                [
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.generationCurrentKey, [0] |> List.map string)
                    (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.paraRateKey, [3.001; 4.001; 5.001] |> List.map string)
                    (ParamKeys.selfSymRateKey, [3.001; 4.001; 5.001]  |> List.map string)
                    (ParamKeys.mutationModKey, [0] |> List.map string)
                    (ParamKeys.selectedSorterCountPerPoolKey, [32;] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.015; 0.20; 0.25; 0.30; 0.40; 0.50;] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
                ]
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs32MutationRate
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
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
                [
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.generationCurrentKey, [0] |> List.map string)
                    (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.paraRateKey, [1.5; 2.5; 3.5] |> List.map string)
                    (ParamKeys.selfSymRateKey, [4.5; 5.5; 6.5]  |> List.map string)
                    (ParamKeys.mutationModKey, [0] |> List.map string)
                    (ParamKeys.selectedSorterCountPerPoolKey, [32;] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.225; 0.25; 0.275;] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
                ]
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs32MutationRate
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )


    module VarModR_64 =



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.generationCurrentKey, [0] |> List.map string)
                    (ParamKeys.generationIntervalCountKey, [12] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                    (ParamKeys.paraRateKey, [4.001] |> List.map string)
                    (ParamKeys.selfSymRateKey, [4.001]  |> List.map string)
                    (ParamKeys.mutationModKey, [0] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.25;] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
                ]
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs32MutationRate
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
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
                [
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.generationCurrentKey, [0] |> List.map string)
                    (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                    (ParamKeys.paraRateKey,    [0.05;  0.1;   0.5;   1.001; 1.5;   2.001; 3.001;] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.001; 1.5;   2.001; 3.001; 4.001; 5.001; 6.001]  |> List.map string)
                    (ParamKeys.mutationModKey, [0] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.015; 0.20; 0.25; 0.30; 0.40; 0.50; 0.99] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
                ]
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs32MutationRate
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
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
                [
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.generationCurrentKey, [0] |> List.map string)
                    (ParamKeys.generationIntervalCountKey, [4] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                    (ParamKeys.paraRateKey,    [0.05;  0.1;   0.5;   1.001; ] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.5;   2.001;]  |> List.map string)
                    (ParamKeys.mutationModKey, [0] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.25; 0.30; 0.40; 0.50;] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
                ]
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs32MutationRate
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )




    module MaxModR_32 =



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test2 () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.generationCurrentKey, [0] |> List.map string)
                    (ParamKeys.generationIntervalCountKey, [10] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.paraRateKey, [0.05; 0.075; 0.1; 0.125] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.25; 1.5; 1.75; 2.5] |> List.map string)
                    (ParamKeys.mutationModKey, [0] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
                ]
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs32MutationRateMax
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )



    module MaxModR_64 =



        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let WideTest () : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.generationCurrentKey, [0] |> List.map string)
                    (ParamKeys.generationIntervalCountKey, [10] |> List.map string)
                    (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                    (ParamKeys.paraRateKey, [0.025; 0.05; 0.75; 0.1] |> List.map string)
                    (ParamKeys.selfSymRateKey, [0.75; 1.001; 1.5; 2.001] |> List.map string)
                    (ParamKeys.mutationModKey, [0] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
                ]
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OrthroPara rate comp for Msrs32p4a Msrs, Test"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs32MutationRateMax
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                    GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
                }
            )
