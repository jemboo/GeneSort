module GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.Soss

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

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32
    let executionFunctionName = ExecuteFunctionNames.soss
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
    let setupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Gens100 ()  : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.sorterCountPerPoolKey, [64] |>  List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [512] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. 7] |> List.map string)
            ]
        let parameterSpansRun =
            [
                (ParamKeys.seedSossKey, [1234UL] |> List.map string)
                (ParamKeys.partitionCountKey, [1; 4; 8;] |> List.map string)
                (ParamKeys.generationsPerSubsetKey, [100;] |> List.map string)
                (ParamKeys.generationsPerMergedSetKey, [100;] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [6] |> List.map string)
                (ParamKeys.paraRateKey,    [1.15;] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.75;]  |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.020;] |> List.map string)
                (ParamKeys.modificationRateKey, [0.035;]  |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2;] |> List.map (MutatorVariant.toString))
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Soss rate comparison for Msuf32p4a"
                AllowOverwrite = false
                Setup = {
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.sossPrefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )



    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.Msuf4a.Soss.Pool_32", [|
            runExample("Gens100", Gens100)
        |])

module Pool_512 =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32
    let executionFunctionName = ExecuteFunctionNames.soss
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
    let setupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let V2a ()  : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.sorterCountPerPoolKey, [512] |>  List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [512] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
            ]
        let parameterSpansRun =
            [
                (ParamKeys.seedSossKey, [1234UL] |> List.map string)
                (ParamKeys.partitionCountKey, [4] |> List.map string)
                (ParamKeys.generationsPerSubsetKey, [100] |> List.map string)
                (ParamKeys.generationsPerMergedSetKey, [100] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationIntervalLastKey, [3] |> List.map string)
                (ParamKeys.paraRateKey,    [0.85; 0.95; 1.05; 1.15;] |> List.map string)
                (ParamKeys.selfSymRateKey, [0.75; 1.25; 1.5; 1.75;]  |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.015; 0.02; 0.025;] |> List.map string)
                (ParamKeys.modificationRateKey, [0.020; 0.025; 0.030; 0.035; 0.040;] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map (MutatorVariant.toString))
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Soss rate comparison for Msuf32p4a"
                AllowOverwrite = false
                Setup = {
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.sossPrefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.Msuf4a.Soss.Pool_512", [|
            runExample("V2a", V2a)
        |])
