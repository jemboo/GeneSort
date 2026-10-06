namespace GeneSort.Dispatch.V1.SorterEval

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

        let Rand_Test (executorType: sorterEvalExecutorType)  : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Standard.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand-Test16_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    [
                rngTypeLcg
                sorterEvalTypeV2
                sortingWidth16
                allSimpleSorterModelTypes
                largeSorterCount
            ]
                    "sorter-eval.standard"
                    RunParamBuilderNames.Filter.standardSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalStandard
                    false
            )

        let Rand_Small (executorType: sorterEvalExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Standard.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand-Small_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    [
                rngTypeLcg
                sorterEvalTypeV2
                smallSortingWidths
                allSimpleSorterModelTypes
                extraLargeSorterCount
            ]
                    "sorter-eval.standard"
                    RunParamBuilderNames.Filter.standardSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalStandard
                    false
            )

        let Rand_Medium (executorType: sorterEvalExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Standard.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Rand-Medium_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
                    [
                rngTypeLcg
                sorterEvalTypeV2
                mediumSortingWidths
                allSimpleSorterModelTypes
                extraLargeSorterCount
            ]
                    "sorter-eval.standard"
                    RunParamBuilderNames.Filter.standardSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalStandard
                    false
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

    let getRun (config: configType) (executorType: sorterEvalExecutorType) : run =
        let specFunc = Configs.[config]
        specFunc executorType
