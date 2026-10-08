namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Sorting
open GeneSort.SortingOps

module SorterEvalSpecsTestPrefix =

    module Specs =

        let Prefix_24s (executorType: sorterEvalExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Prefix.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Prefix_24s_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf6"
                    [   
                rngTypeLcg
                prefixLib_Prefix24s
                allSimpleSorterModelTypes
                dataFomatBitv512
                sorterEvalTypeV2
                largeSorterCount
            ]
                    QueryCatalogNames.sorterEvalPrefix
                    RunParamBuilderNames.Filter.prefixSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalPrefix
                    false
            )


        let Prefix_32 (executorType: sorterEvalExecutorType) : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterEvalDbs.Prefix.dbName
                    SorterEvalDbs.projectName
                    (sprintf @"Prefix_32_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag)
                    "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf4"
                    [   
                rngTypeLcg
                prefixLib_Prefix32_4
                allSimpleSorterModelTypes
                dataFomatBitv512
                sorterEvalTypeV2
                largeSorterCount
            ]
                    QueryCatalogNames.sorterEvalPrefix
                    RunParamBuilderNames.Filter.prefixSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterEvalPrefix
                    false
            )


    type configType =
        | Prefix_24s
        | Prefix_32

    let Configs = Map.ofList 
                    [ 
                        (configType.Prefix_24s, Specs.Prefix_24s);
                        (configType.Prefix_32, Specs.Prefix_32);
                    ]

    let getRun (config: configType) (executorType: sorterEvalExecutorType) : run =
        let specFunc = Configs.[config]
        specFunc executorType
