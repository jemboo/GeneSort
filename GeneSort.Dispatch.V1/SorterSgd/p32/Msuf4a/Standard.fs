
module GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.Standard
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



module Pool_32 =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                (ParamKeys.paraRateKey,    [1.15;] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75;]  |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 7] |> List.map string)
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
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = []
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval2
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let V1 ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [512] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [3] |> List.map string)
                (ParamKeys.paraRateKey,    [0.85; 0.95; 1.05; 1.15;] |> List.map string)
                (ParamKeys.selfSymRateKey, [0.75; 1.25; 1.5; 1.75;]  |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020; 0.030; 0.040; 0.050;] |> List.map string)
                (ParamKeys.modificationRateKey, [0.015; 0.020; 0.025; 0.030; 0.040; 0.050;] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Rate comp for 32p4a Msuf4"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = []
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )


    let description = "Standard evolutionary runs for the 32-input Msuf4a configuration (Pool_32)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.Msuf4a.Standard.Pool_32", description, [|
            runExample("Test", Test)
            runExample("V1", V1)
        |])

module Pool_512 =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let V2a ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sorterCountPerPoolSetKey, [512] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [3] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [512] |>  List.map string)
                (ParamKeys.paraRateKey,    [0.85; 0.95; 1.05; 1.15;] |> List.map string)
                (ParamKeys.selfSymRateKey, [0.75; 1.25; 1.5; 1.75;]  |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.015; 0.02; 0.025;] |> List.map string)
                (ParamKeys.modificationRateKey, [0.020; 0.025; 0.030; 0.035; 0.040;] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map (MutatorVariant.toString))
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Rate comp for 32p4a Msuf4"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = []
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    let description = "Standard evolutionary runs for the 32-input Msuf4a configuration (Pool_512)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.Msuf4a.Standard.Pool_512", description, [|
            runExample("V2a", V2a)
        |])
