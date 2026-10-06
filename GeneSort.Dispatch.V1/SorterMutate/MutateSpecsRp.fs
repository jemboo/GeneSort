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


module MutateSpecsRp = 

    let seedSorterPoolSelectionType = 
            (runParameters.seedSorterPoolSelectionTypeKey, 
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)

    module Specs =

        let Test_Msce (executorType: sorterMutateExecutorType)  : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterMutateDbs.RandomPrefix.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Test-Msce_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag)
                    "Mutation analysis for merge Msce"
                    [
                msceModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMsce 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
                    "sorter-mutate.prefix"
                    RunParamBuilderNames.Filter.prefixSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterMutateStandard
                    false
            )


        let Test_Mssi (executorType: sorterMutateExecutorType)  : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterMutateDbs.RandomPrefix.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Test-Mssi_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag)
                    "Mutation analysis for merge Mssi"
                    [
                mssiModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMssi 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
                    "sorter-mutate.prefix"
                    RunParamBuilderNames.Filter.prefixSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterMutateStandard
                    false
            )


        let Test_Msrs (executorType: sorterMutateExecutorType)  : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterMutateDbs.RandomPrefix.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Test-Msrs_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag)
                    "Mutation analysis for merge Msrs"
                    [
                msrsModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMsrs 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
                    "sorter-mutate.prefix"
                    RunParamBuilderNames.Filter.prefixSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterMutateStandard
                    false
            )

        let Test_Msuf4 (executorType: sorterMutateExecutorType)  : run =
            run.SimpleRun (
                SimpleRun.create
                    SorterMutateDbs.RandomPrefix.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Test-Msuf4_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag)
                    "Mutation analysis for merge Msuf4"
                    [
                msuf4ModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMsuf4 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
                    "sorter-mutate.prefix"
                    RunParamBuilderNames.Filter.prefixSorterModelCompatibility
                    RunParamBuilderNames.Enhancer.sorterMutateStandard
                    false
            )


    type configType =
        | Test_Msce
        | Test_Mssi
        | Test_Msrs
        | Test_Msuf4
        | Test_Msuf6

    let Configs = Map.ofList 
                    [ 
                        (configType.Test_Msce, Specs.Test_Msce); 
                        (configType.Test_Mssi, Specs.Test_Mssi); 
                        (configType.Test_Msrs, Specs.Test_Msrs); 
                        (configType.Test_Msuf4, Specs.Test_Msuf4);
                    ]

    let getRun (config: configType) (executorType: sorterMutateExecutorType) : run =
        let specFunc = Configs.[config]
        specFunc executorType


