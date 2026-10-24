module GeneSort.Dispatch.V1.SorterSgd.RunCfg.p24.Msuf63b.MutationRate

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
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
open GeneSort.Dispatch.V1.SorterSgd.RunCfg.p24.Msuf63b.Common


let private sorterEvalMeasures =
    [ 4.5; 5.5; 7.5; 9.5 ]
    |> List.map (fun unsortedWeight ->
        ceStUcMeasure.create {
            StageWeight = 1.1<stageWeight>
            UnsortedWeight = unsortedWeight |> UMX.tag<unsortedWeight>
            FilterReflectionSymmetric = false |> UMX.tag<filterReflectionSymmetric>
        }
        |> sorterEvalMeasure.CeStUc
        |> SorterEvalMeasure.toCompactString
    )



module VarModR_32 =


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sorterCountPerPoolSetKey, [256] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [2] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [64] |> List.map string)
                (ParamKeys.sorterEvalMeasureKey, sorterEvalMeasures)
                (ParamKeys.paraRateKey, [1.001] |> List.map string)
                (ParamKeys.selfSymRateKey, [2.001] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.05; 0.1] |> List.map string)
                (ParamKeys.modificationRateKey, [0.05; 0.075; 0.1; 0.125;] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Mutation rate test for Msuf6 24p3b"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdUf6MutationRate
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msuf624p3bMutationRate
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdUf6MutationRate
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
                (ParamKeys.sorterCountPerPoolSetKey, [512] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [10] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [128] |> List.map string)
                (ParamKeys.sorterEvalMeasureKey, sorterEvalMeasures)
                (ParamKeys.paraRateKey, [0.1; 0.5; 1.001; 1.5] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.001; 1.5; 2.001; 3.001] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.05] |> List.map string)
                (ParamKeys.modificationRateKey, [0.05; 0.075; 0.1; 0.125;] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
            ]
        run.SgdRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Wide mutation rate test for Msuf6 24p3b"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdUf6MutationRate
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.msuf624p3bMutationRate
                }
                Setup = {
                    FunctionName = SetupCatalogNames.standard
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdUf6MutationRate
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )
