namespace GeneSort.Dispatch.V1.SorterEval

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1

module Merge =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
    let executionFunctionName = ExecuteFunctionNames.sorterEval
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterEvalMerge
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
    let setupFunctionName = SetupCatalogNames.merge
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterEvalMerge


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 32<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 32<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantB
                     mergeLibId.create 32<sortingWidth> 4<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 32<sortingWidth> 8<mergeDimension> sorterLibVariant.VariantA
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.sorterCountKey, [100] |> List.map string)
            ]
        let parameterSpansRun = []
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Small () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.mergeLibIdKey,
                    [ for width in [16; 18; 24; 32; 36; 48; 64] do
                        for dimension in [2; 3; 4; 6; 8] do
                            if width % dimension = 0 then
                                yield mergeLibId.create (UMX.tag width) (UMX.tag dimension) sorterLibVariant.VariantA ]
                    |> List.map MergeLibId.toString)
                (ParamKeys.sorterCountKey, [100000] |> List.map string)
            ]
        let parameterSpansRun = []
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let MediumLd () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.mergeLibIdKey,
                    [ for width in [96; 128] do
                        for dimension in [2; 3; 4] do
                            if width % dimension = 0 then
                                yield mergeLibId.create (UMX.tag width) (UMX.tag dimension) sorterLibVariant.VariantA ]
                    |> List.map MergeLibId.toString)
                (ParamKeys.sorterCountKey, [10000] |> List.map string)
            ]
        let parameterSpansRun = []
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let MediumHd () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.mergeLibIdKey,
                    [mergeLibId.create 96<sortingWidth> 6<mergeDimension> sorterLibVariant.VariantA]
                    |> List.map MergeLibId.toString)
                (ParamKeys.sorterCountKey, [10000] |> List.map string)
            ]
        let parameterSpansRun = []
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let LargeLd () : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
                (ParamKeys.simpleSorterModelTypeKey,
                 [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4]
                 |> List.map SimpleSorterModelType.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V2] |> List.map SorterEvalType.toString)
                (ParamKeys.mergeLibIdKey,
                    [ for width in [192; 256; 512] do
                        yield mergeLibId.create (UMX.tag width) 2<mergeDimension> sorterLibVariant.VariantA ]
                    |> List.map MergeLibId.toString)
                (ParamKeys.sorterCountKey, [10000] |> List.map string)
            ]
        let parameterSpansRun = []
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    [<RequireQualifiedAccess>]
    type configType =
        | Test
        | Small
        | MediumLd
        | MediumHd
        | LargeLd


    let Configs = Map.ofList
                    [
                        (configType.Test, Test);
                        (configType.Small, Small);
                        (configType.MediumLd, MediumLd);
                        (configType.MediumHd, MediumHd);
                        (configType.LargeLd, LargeLd);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
