namespace GeneSort.Dispatch.V1.SorterEval

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams


module SorterEvalSpecsRs =

    module Specs =

        let Rand_Test ()  : run =
            let parameterSpans =
                [
                    rngTypeLcg
                    sorterEvalTypeV2
                    sortingWidth16
                    withMsuf4
                    largeSorterCount
                ]
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval
                    SorterEvalDbs.Standard.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand-Test16_%s" "GenStandard" |> UMX.tag)
                    "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    parameterSpans
                    QueryCatalogNames.sorterEvalStandard
                    SetupCatalogNames.standard
                    RunParamBuilderNames.Enhancer.sorterEvalStandard
                    false
                    parameterSpans
                    QueryCatalogNames.sorterEvalStandard
                    SetupEnhancerFunctionNames.sorterEval
                    [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            )

        let Rand_Small () : run =
            let parameterSpans =
                [
                    rngTypeLcg
                    sorterEvalTypeV2
                    smallSortingWidths
                    withMsuf4
                    extraLargeSorterCount
                ]
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval
                    SorterEvalDbs.Standard.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand-Small_%s" "GenStandard" |> UMX.tag)
                    "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    parameterSpans
                    QueryCatalogNames.sorterEvalStandard
                    SetupCatalogNames.standard
                    RunParamBuilderNames.Enhancer.sorterEvalStandard
                    false
                    parameterSpans
                    QueryCatalogNames.sorterEvalStandard
                    SetupEnhancerFunctionNames.sorterEval
                    [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            )

        let Rand_Medium () : run =
            let parameterSpans =
                [
                    rngTypeLcg
                    sorterEvalTypeV2
                    mediumSortingWidths
                    withMsuf4
                    extraLargeSorterCount
                ]
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval
                    SorterEvalDbs.Standard.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand-Medium_%s" "GenStandard" |> UMX.tag)
                    "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    parameterSpans
                    QueryCatalogNames.sorterEvalStandard
                    SetupCatalogNames.standard
                    RunParamBuilderNames.Enhancer.sorterEvalStandard
                    false
                    parameterSpans
                    QueryCatalogNames.sorterEvalStandard
                    SetupEnhancerFunctionNames.sorterEval
                    [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            )

    type configType =
        | Rand_Test
        | Rand_Small
        | Rand_Medium

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_Medium, Specs.Rand_Medium);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
