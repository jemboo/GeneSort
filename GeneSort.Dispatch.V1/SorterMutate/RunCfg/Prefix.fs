namespace GeneSort.Dispatch.V1.SorterMutate.RunCfg

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterMutate


module Prefix =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Msce ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.simpleSorterModelTypeKey, [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.seedSorterPoolSelectionTypeKey, [sorterSelectionType.ValueSpan 5<sorterCount>] |> List.map SorterSelectionType.toString)
                (ParamKeys.sorterEvalMeasureKey, [SorterEvalMeasure.stageBiasedFilterUnsorted] |> List.map SorterEvalMeasure.toCompactString)
                (ParamKeys.prefixLibIdKey,
                 [prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA]
                 |> List.map PrefixLibId.toString)
                (ParamKeys.mutatorParamsKey,
                 [MutatorParams.msceParamsR5 32<sortingWidth> rngType.Lcg mutatorVariant.V1]
                 |> List.map MutatorParams.toString)
                (ParamKeys.modificationRateKey, [0.03] |> List.map string)
                (ParamKeys.sorterChildCountKey, [1000] |> List.map string)
                (ParamKeys.mutationModKey, [1] |> List.map string)
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
                (ParamKeys.simpleSorterModelTypeKey, [simpleSorterModelType.Mssi] |> List.map SimpleSorterModelType.toString)
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.seedSorterPoolSelectionTypeKey, [sorterSelectionType.ValueSpan 5<sorterCount>] |> List.map SorterSelectionType.toString)
                (ParamKeys.sorterEvalMeasureKey, [SorterEvalMeasure.stageBiasedFilterUnsorted] |> List.map SorterEvalMeasure.toCompactString)
                (ParamKeys.prefixLibIdKey,
                 [prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA]
                 |> List.map PrefixLibId.toString)
                (ParamKeys.mutatorParamsKey,
                 [MutatorParams.mssiParamsRL 32<sortingWidth> rngType.Lcg mutatorVariant.V1]
                 |> List.map MutatorParams.toString)
                (ParamKeys.modificationRateKey, [0.03] |> List.map string)
                (ParamKeys.sorterChildCountKey, [1000] |> List.map string)
                (ParamKeys.mutationModKey, [1] |> List.map string)
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
                (ParamKeys.simpleSorterModelTypeKey, [simpleSorterModelType.Msrs] |> List.map SimpleSorterModelType.toString)
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.seedSorterPoolSelectionTypeKey, [sorterSelectionType.ValueSpan 5<sorterCount>] |> List.map SorterSelectionType.toString)
                (ParamKeys.sorterEvalMeasureKey, [SorterEvalMeasure.stageBiasedFilterUnsorted] |> List.map SorterEvalMeasure.toCompactString)
                (ParamKeys.prefixLibIdKey,
                 [prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA]
                 |> List.map PrefixLibId.toString)
                (ParamKeys.mutatorParamsKey,
                 [MutatorParams.msrsParamsRL 32<sortingWidth> rngType.Lcg mutatorVariant.V1]
                 |> List.map MutatorParams.toString)
                (ParamKeys.modificationRateKey, [0.03] |> List.map string)
                (ParamKeys.sorterChildCountKey, [1000] |> List.map string)
                (ParamKeys.mutationModKey, [1] |> List.map string)
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
                (ParamKeys.simpleSorterModelTypeKey, [simpleSorterModelType.Msuf4] |> List.map SimpleSorterModelType.toString)
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.seedSorterPoolSelectionTypeKey, [sorterSelectionType.ValueSpan 5<sorterCount>] |> List.map SorterSelectionType.toString)
                (ParamKeys.sorterEvalMeasureKey, [SorterEvalMeasure.stageBiasedFilterUnsorted] |> List.map SorterEvalMeasure.toCompactString)
                (ParamKeys.prefixLibIdKey,
                 [prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA]
                 |> List.map PrefixLibId.toString)
                (ParamKeys.mutatorParamsKey,
                 [MutatorParams.msuf4ParamsRL 32<sortingWidth> rngType.Lcg mutatorVariant.V1]
                 |> List.map MutatorParams.toString)
                (ParamKeys.modificationRateKey, [0.03] |> List.map string)
                (ParamKeys.sorterChildCountKey, [1000] |> List.map string)
                (ParamKeys.mutationModKey, [1] |> List.map string)
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
