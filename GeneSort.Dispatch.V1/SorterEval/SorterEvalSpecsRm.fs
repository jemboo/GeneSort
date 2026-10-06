namespace GeneSort.Dispatch.V1.SorterEval

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

        let Rand_MergeTest_Test (executorType: sorterEvalExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-Test_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    [   
                rngTypeLcg
                mergeLib_Merge32s
                dataFormatInt8v512
                allSimpleSorterModelTypes
                noSuffixSuffixType
                sorterEvalTypeV2
                smallSorterCount
            ]
                    "sorter-eval.merge"
                    RunParamBuilderNames.Filter.mergeSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalMerge
                    false
            )


        let Rand_MergeTest_Small (executorType: sorterEvalExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-Small_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    [
                rngTypeLcg
                dataFormatInt8v512
                allSimpleSorterModelTypes
                noSuffixSuffixType
                sorterEvalTypeV2
                smallMergeSortingWidths
                allMergeDimensions
                extraLargeSorterCount
            ]
                    "sorter-eval.merge"
                    RunParamBuilderNames.Filter.mergeSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalMerge
                    false
            )


        let Rand_MergeTest_MediumLd (executorType: sorterEvalExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-MediumLd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    [
                rngTypeLcg
                dataFormatInt8v512
                allSimpleSorterModelTypes
                noSuffixSuffixType
                sorterEvalTypeV2
                mediumMergeSortingWidths
                lowMergeDimensions
                largeSorterCount
            ]
                    "sorter-eval.merge"
                    RunParamBuilderNames.Filter.mergeSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalMerge
                    false
            )


        let Rand_MergeTest_MediumHd (executorType: sorterEvalExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-MediumHd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    [
                rngTypeLcg
                dataFormatInt8v512
                noSuffixSuffixType
                allSimpleSorterModelTypes
                sorterEvalTypeV2
                sortingWidth96
                mergeDimension6
                largeSorterCount
            ]
                    "sorter-eval.merge"
                    RunParamBuilderNames.Filter.mergeSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalMerge
                    false
            )


        let Rand_MergeTest_LargeLd (executorType: sorterEvalExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-LargeLd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
                    [
                rngTypeLcg
                dataFormatInt8v512
                noSuffixSuffixType
                allSimpleSorterModelTypes
                sorterEvalTypeV2
                largeMergeSortingWidths
                mergeDimension2
                largeSorterCount
            ]
                    "sorter-eval.merge"
                    RunParamBuilderNames.Filter.mergeSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalMerge
                    false
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

    let getRun (config: configType) (executorType: sorterEvalExecutorType) : run =
        let specFunc = Configs.[config]
        specFunc executorType
