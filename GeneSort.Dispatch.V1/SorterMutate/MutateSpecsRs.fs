namespace GeneSort.Dispatch.V1.SorterMutate

open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.SortingOps


module MutateSpecsRs = 

    let sorterEvalSelectionType = 
            (runParameters.seedSorterPoolSelectionTypeKey, 
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)
    

    module Specs =

        let Test_Msrs (executorType: sorterMutateExecutorType)  : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterMutateDbs.RandomStandard.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Rand-Test_Msrs%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag)
                    "Mutation analysis for Msrs"
                    [
                msrsModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_Scw
                (mutatorSpansMsrs 16<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate90
                sortingWidth16
                testChildCount
                mutationMod1
            ]
                    QueryCatalogNames.sorterMutateStandard
                    RunParamBuilderNames.Filter.standardSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterMutateStandardFormat
                    false
            )


        let Test_Msuf4 (executorType: sorterMutateExecutorType)  : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterMutateDbs.RandomStandard.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Rand-Test_Msuf4%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag)
                    "Mutation analysis for Msuf4"
                    [
                msuf4ModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_Scw
                (mutatorSpansMsrs 16<sortingWidth> mutatorVariant.V1 rngType.Lcg )
                modificationRate90
                sortingWidth16
                testChildCount
                mutationMod1
            ]
                    QueryCatalogNames.sorterMutateStandard
                    RunParamBuilderNames.Filter.standardSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterMutateStandardFormat
                    false
            )





    type configType =
        | Test_Msrs
        | Rand_Small
        | Rand_Medium

    let Configs = Map.ofList 
                    [ 
                        (configType.Test_Msrs, Specs.Test_Msrs); 
                    ]

    let getRun (config: configType) (executorType: sorterMutateExecutorType) : run =
        let specFunc = Configs.[config]
        specFunc executorType


