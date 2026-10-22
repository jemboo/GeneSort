namespace GeneSort.Dispatch.V1.SorterMutate.RunCfg

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
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterMutate


module Prefix =

    let seedSorterPoolSelectionType =
            (ParamKeys.seedSorterPoolSelectionTypeKey,
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Msce ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                msceModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMsce 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Mutation analysis for merge Msce"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterMutate
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterMutatePrefix
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandard
                }
                Setup = {
                    FunctionName = SetupCatalogNames.mutantPrefix
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterMutatePrefix
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Mssi ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                mssiModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMssi 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Mutation analysis for merge Mssi"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterMutate
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterMutatePrefix
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandard
                }
                Setup = {
                    FunctionName = SetupCatalogNames.mutantPrefix
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterMutatePrefix
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Msrs ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                msrsModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMsrs 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Mutation analysis for merge Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterMutate
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterMutatePrefix
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandard
                }
                Setup = {
                    FunctionName = SetupCatalogNames.mutantPrefix
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterMutatePrefix
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Msuf4 ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                msuf4ModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMsuf4 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Mutation analysis for merge Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterMutate
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterMutatePrefix
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandard
                }
                Setup = {
                    FunctionName = SetupCatalogNames.mutantPrefix
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterMutatePrefix
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
                }
            }
        )

    [<RequireQualifiedAccess>]
    type configType =
        | Msce
        | Mssi
        | Msrs
        | Msuf4
        | Msuf6

    let Configs = Map.ofList
                    [
                        (configType.Msce, Msce);
                        (configType.Mssi, Mssi);
                        (configType.Msrs, Msrs);
                        (configType.Msuf4, Msuf4);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
