namespace GeneSort.Dispatch.V1.SorterEval.RunCfg

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams

module Merge =


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                rngTypeLcg
                mergeLib_Merge32s
                dataFormatInt8v512
                withMsuf4
                noSuffixSuffixType
                sorterEvalTypeV2
                smallSorterCount
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterEval
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterEvalMerge
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                }
                Setup = {
                    FunctionName = SetupCatalogNames.merge
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterEvalMerge
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Small () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                rngTypeLcg
                dataFormatInt8v512
                withMsuf4
                noSuffixSuffixType
                sorterEvalTypeV2
                smallMergeSortingWidths
                allMergeDimensions
                extraLargeSorterCount
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterEval
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterEvalMerge
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                }
                Setup = {
                    FunctionName = SetupCatalogNames.merge
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterEvalMerge
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let MediumLd () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                rngTypeLcg
                dataFormatInt8v512
                withMsuf4
                noSuffixSuffixType
                sorterEvalTypeV2
                mediumMergeSortingWidths
                lowMergeDimensions
                largeSorterCount
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterEval
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterEvalMerge
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                }
                Setup = {
                    FunctionName = SetupCatalogNames.merge
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterEvalMerge
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let MediumHd () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                rngTypeLcg
                dataFormatInt8v512
                noSuffixSuffixType
                withMsuf4
                sorterEvalTypeV2
                sortingWidth96
                mergeDimension6
                largeSorterCount
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterEval
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterEvalMerge
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                }
                Setup = {
                    FunctionName = SetupCatalogNames.merge
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterEvalMerge
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let LargeLd () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                rngTypeLcg
                dataFormatInt8v512
                noSuffixSuffixType
                withMsuf4
                sorterEvalTypeV2
                largeMergeSortingWidths
                mergeDimension2
                largeSorterCount
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterEval
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sorterEvalMerge
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                }
                Setup = {
                    FunctionName = SetupCatalogNames.merge
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sorterEvalMerge
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
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
