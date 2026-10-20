module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.VarModR_32

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
open GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Common
open GeneSort.Dispatch.V1.SorterSgd

let projName = "SorterSgd.Prefix.32p4a.Msuf" |> UMX.tag<projectName>

let dbPool32name = "Pool_32" |> UMX.tag<databaseName>
let dbPool32TestName = "Pool_32_Test" |> UMX.tag<databaseName>
let dbPool512name = "Pool_512" |> UMX.tag<databaseName>

let Pool_32_Test ()  : run =
    let parameterSpans =
        [
            (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
            (ParamKeys.sorterCountPerPoolSetKey, [128] |> List.map string)
            (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
            (ParamKeys.generationIntervalLastKey, [2] |> List.map string)
            (ParamKeys.paraRateKey,    [1.15;] |> List.map string)
            (ParamKeys.selfSymRateKey, [1.75;]  |> List.map string)
            (ParamKeys.mutationModKey, [0;] |> List.map string)
            (ParamKeys.seedModificationRateKey, [0.020;] |> List.map string)
            (ParamKeys.modificationRateKey, [0.035;]  |> List.map string)
            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2;] |> List.map (MutatorVariant.toString))
        ]
    run.SgdRun (
        SgdRun.create {
            DatabaseName = dbPool32TestName
            ProjectName = projName
            RunName = (sprintf @"Pool_32_Test%s" "GenStandard" |> UMX.tag)
            Description = "Rate comp for 32p4a Msuf4"
            AllowOverwrite = false
            Execution = {
                FunctionName = ExecuteFunctionNames.standard
                ParameterSpans = parameterSpans
                QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
                EnhancerFunctionName = RunParamBuilderNames.Enhancer.msuf32MutationRate
            }
            Setup = {
                FunctionName = SetupCatalogNames.standard
                ParameterSpans = parameterSpans
                QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
            }
        } {
            MakerNames = EvolutionMakerNames.prefix
            GenSaveIntervalsName = "expInterval100_L50ss"
            GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
        }
    )

let Pool_32_V1 ()  : run =
    let parameterSpans =
        [
            (ParamKeys.sorterCountPerPoolSetKey, [512] |> List.map string)
            (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
            (ParamKeys.generationIntervalLastKey, [3] |> List.map string)
            (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
            (ParamKeys.paraRateKey,    [0.85; 0.95; 1.05; 1.15;] |> List.map string)
            (ParamKeys.selfSymRateKey, [0.75; 1.25; 1.5; 1.75;]  |> List.map string)
            (ParamKeys.mutationModKey, [0] |> List.map string)
            (ParamKeys.seedModificationRateKey, [0.020; 0.030; 0.040; 0.050;] |> List.map string)
            (ParamKeys.modificationRateKey, [0.015; 0.020; 0.025; 0.030; 0.040; 0.050;] |> List.map string)
            (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map (MutatorVariant.toString))
        ]
    run.SgdRun (
        SgdRun.create {
            DatabaseName = dbPool32name
            ProjectName = projName
            RunName = (sprintf @"Pool_32_V1%s" "GenStandard" |> UMX.tag)
            Description = "Rate comp for 32p4a Msuf4"
            AllowOverwrite = false
            Execution = {
                FunctionName = ExecuteFunctionNames.standard
                ParameterSpans = parameterSpans
                QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
                EnhancerFunctionName = RunParamBuilderNames.Enhancer.msuf32MutationRate
            }
            Setup = {
                FunctionName = SetupCatalogNames.standard
                ParameterSpans = parameterSpans
                QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
            }
        } {
            MakerNames = EvolutionMakerNames.prefix
            GenSaveIntervalsName = "expInterval100_L50ss"
            GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
        }
    )

let Pool_512_V2a ()  : run =
    let parameterSpans =
        [
            (ParamKeys.sorterCountPerPoolSetKey, [512] |> List.map string)
            (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
            (ParamKeys.generationIntervalLastKey, [3] |> List.map string)
            (ParamKeys.sorterCountPerPoolKey, [512] |>  List.map string)
            (ParamKeys.paraRateKey,    [0.85; 0.95; 1.05; 1.15;] |> List.map string)
            (ParamKeys.selfSymRateKey, [0.75; 1.25; 1.5; 1.75;]  |> List.map string)
            (ParamKeys.mutationModKey, [0] |> List.map string)
            (ParamKeys.seedModificationRateKey, [0.015; 0.02; 0.025;] |> List.map string)
            (ParamKeys.modificationRateKey, [0.020; 0.025; 0.030; 0.035; 0.040;] |> List.map string)
            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map (MutatorVariant.toString))
        ]
    run.SgdRun (
        SgdRun.create {
            DatabaseName = dbPool512name
            ProjectName = projName
            RunName = (sprintf @"Pool_512_V2a%s" "GenStandard" |> UMX.tag)
            Description = "Rate comp for 32p4a Msuf4"
            AllowOverwrite = false
            Execution = {
                FunctionName = ExecuteFunctionNames.standard
                ParameterSpans = parameterSpans
                QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
                EnhancerFunctionName = RunParamBuilderNames.Enhancer.msuf32MutationRate
            }
            Setup = {
                FunctionName = SetupCatalogNames.standard
                ParameterSpans = parameterSpans
                QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
            }
        } {
            MakerNames = EvolutionMakerNames.prefix
            GenSaveIntervalsName = "expInterval100_L50ss"
            GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
        }
    )
