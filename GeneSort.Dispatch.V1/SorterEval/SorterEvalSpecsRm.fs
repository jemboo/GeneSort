namespace GeneSort.Dispatch.V1.SorterEval

open GeneSort.Project.V1.Catalogs.RunParameters
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
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-Test_%s" "GenMerge" |> UMX.tag)
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
                    QueryCatalogNames.sorterEvalMerge
                    SetupCatalogNames.merge
                    RunParamBuilderNames.Filter.mergeSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalMerge
                    false
            )


        let Rand_MergeTest_Small () : run =
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-Small_%s" "GenMerge" |> UMX.tag)
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
                    QueryCatalogNames.sorterEvalMerge
                    SetupCatalogNames.merge
                    RunParamBuilderNames.Filter.mergeSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalMerge
                    false
            )


        let Rand_MergeTest_MediumLd () : run =
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-MediumLd_%s" "GenMerge" |> UMX.tag)
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
                    QueryCatalogNames.sorterEvalMerge
                    SetupCatalogNames.merge
                    RunParamBuilderNames.Filter.mergeSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalMerge
                    false
            )


        let Rand_MergeTest_MediumHd () : run =
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-MediumHd_%s" "GenMerge" |> UMX.tag)
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
                    QueryCatalogNames.sorterEvalMerge
                    SetupCatalogNames.merge
                    RunParamBuilderNames.Filter.mergeSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalMerge
                    false
            )


        let Rand_MergeTest_LargeLd () : run =
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval
                    SorterEvalDbs.Merge.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand_MergeTest-LargeLd_%s" "GenMerge" |> UMX.tag)
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
                    QueryCatalogNames.sorterEvalMerge
                    SetupCatalogNames.merge
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

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
