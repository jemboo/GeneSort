namespace GeneSort.Dispatch.V1.SorterEval

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Sorting
open GeneSort.SortingOps

module SorterEvalSpecsRm =

    module Specs =

        let Rand_MergeTest_Test () : run =
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
                    DatabaseName = SorterEvalDbs.Merge.dbName
                    ProjectName = SorterEvalDbs.projectName
                    RunName = (sprintf @"Rand_MergeTest-Test_%s" "GenMerge" |> UMX.tag)
                    Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.merge
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                        OutputTypes = [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                    }
                }
            )


        let Rand_MergeTest_Small () : run =
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
                    DatabaseName = SorterEvalDbs.Merge.dbName
                    ProjectName = SorterEvalDbs.projectName
                    RunName = (sprintf @"Rand_MergeTest-Small_%s" "GenMerge" |> UMX.tag)
                    Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.merge
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                        OutputTypes = [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                    }
                }
            )


        let Rand_MergeTest_MediumLd () : run =
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
                    DatabaseName = SorterEvalDbs.Merge.dbName
                    ProjectName = SorterEvalDbs.projectName
                    RunName = (sprintf @"Rand_MergeTest-MediumLd_%s" "GenMerge" |> UMX.tag)
                    Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.merge
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                        OutputTypes = [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                    }
                }
            )


        let Rand_MergeTest_MediumHd () : run =
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
                    DatabaseName = SorterEvalDbs.Merge.dbName
                    ProjectName = SorterEvalDbs.projectName
                    RunName = (sprintf @"Rand_MergeTest-MediumHd_%s" "GenMerge" |> UMX.tag)
                    Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.merge
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                        OutputTypes = [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                    }
                }
            )


        let Rand_MergeTest_LargeLd () : run =
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
                    DatabaseName = SorterEvalDbs.Merge.dbName
                    ProjectName = SorterEvalDbs.projectName
                    RunName = (sprintf @"Rand_MergeTest-LargeLd_%s" "GenMerge" |> UMX.tag)
                    Description = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterEval
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalMerge
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.merge
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterEvalMerge
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                        OutputTypes = [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                    }
                }
            )


    type configType =
        | Rand_MergeTest_Test
        | Rand_MergeTest_Small
        | Rand_MergeTest_MediumLd
        | Rand_MergeTest_MediumHd
        | Rand_MergeTest_LargeLd


    let Configs = Map.ofList
                    [
                        (configType.Rand_MergeTest_Test, Specs.Rand_MergeTest_Test);
                        (configType.Rand_MergeTest_Small, Specs.Rand_MergeTest_Small);
                        (configType.Rand_MergeTest_MediumLd, Specs.Rand_MergeTest_MediumLd);
                        (configType.Rand_MergeTest_MediumHd, Specs.Rand_MergeTest_MediumHd);
                        (configType.Rand_MergeTest_LargeLd, Specs.Rand_MergeTest_LargeLd);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
