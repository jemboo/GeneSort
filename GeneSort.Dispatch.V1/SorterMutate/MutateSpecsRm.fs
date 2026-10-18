namespace GeneSort.Dispatch.V1.SorterMutate


open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Project.V1.Parameters

module MutateSpecsRm =

    let seedSorterPoolSelectionType = 
            (runParameters.seedSorterPoolSelectionTypeKey, 
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)

    module Specs =

        let Test_Msce ()  : run =
            let parameterSpans =
                [
                    msceModelType
                    rngTypeLcg
                    dataFormatInt8v512
                    sorterEvalTypeV2
                    seedSorterPoolSelectionType
                    sorterEvalMeasure_CestM_noScw
                    mergeLib_Merge32s
                    (mutatorSpansMsce 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                    modificationRate03
                    testChildCount
                    mutationMod1
                ]
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterMutate
                    SorterMutateDbs.RandomMerge.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Test-Msce_%s" "GenMerge" |> UMX.tag)
                    "Mutation analysis for merge Msce"
                    parameterSpans
                    QueryCatalogNames.sorterMutateMerge
                    SetupCatalogNames.mutantMerge
                    RunParamBuilderNames.Enhancer.sorterMutateStandard
                    false
                    parameterSpans
                    QueryCatalogNames.sorterMutateMerge
                    SetupEnhancerFunctionNames.sorterMutate
                    [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            )


        let Test_Mssi ()  : run =
            let parameterSpans =
                [
                    mssiModelType
                    rngTypeLcg
                    dataFormatInt8v512
                    sorterEvalTypeV2
                    seedSorterPoolSelectionType
                    sorterEvalMeasure_CestM_noScw
                    mergeLib_Merge32s
                    (mutatorSpansMssi 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                    modificationRate03
                    testChildCount
                    mutationMod1
                ]
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterMutate
                    SorterMutateDbs.RandomMerge.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Test-Mssi_%s" "GenMerge" |> UMX.tag)
                    "Mutation analysis for merge Mssi"
                    parameterSpans
                    QueryCatalogNames.sorterMutateMerge
                    SetupCatalogNames.mutantMerge
                    RunParamBuilderNames.Enhancer.sorterMutateStandard
                    false
                    parameterSpans
                    QueryCatalogNames.sorterMutateMerge
                    SetupEnhancerFunctionNames.sorterMutate
                    [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            )


        let Test_Msrs ()  : run =
            let parameterSpans =
                [
                    msrsModelType
                    rngTypeLcg
                    dataFormatInt8v512
                    sorterEvalTypeV2
                    seedSorterPoolSelectionType
                    sorterEvalMeasure_CestM_noScw
                    mergeLib_Merge32s
                    (mutatorSpansMsrs 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                    modificationRate03
                    testChildCount
                    mutationMod1
                ]
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterMutate
                    SorterMutateDbs.RandomMerge.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Test-Msrs_%s" "GenMerge" |> UMX.tag)
                    "Mutation analysis for merge Msrs"
                    parameterSpans
                    QueryCatalogNames.sorterMutateMerge
                    SetupCatalogNames.mutantMerge
                    RunParamBuilderNames.Enhancer.sorterMutateStandard
                    false
                    parameterSpans
                    QueryCatalogNames.sorterMutateMerge
                    SetupEnhancerFunctionNames.sorterMutate
                    [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            )

        let Test_Msuf4 ()  : run =
            let parameterSpans =
                [
                    msuf4ModelType
                    rngTypeLcg
                    dataFormatInt8v512
                    sorterEvalTypeV2
                    seedSorterPoolSelectionType
                    sorterEvalMeasure_CestM_noScw
                    mergeLib_Merge32s
                    (mutatorSpansMsuf4 32<sortingWidth> mutatorVariant.V1 rngType.Lcg)
                    modificationRate03
                    testChildCount
                    mutationMod1
                ]
            run.SimpleRun (
                SimpleRun.create GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterMutate
                    SorterMutateDbs.RandomMerge.Uniform.dbName
                    SorterMutateDbs.projectName
                    (sprintf @"Test-Msuf4_%s" "GenMerge" |> UMX.tag)
                    "Mutation analysis for merge Msuf4"
                    parameterSpans
                    QueryCatalogNames.sorterMutateMerge
                    SetupCatalogNames.mutantMerge
                    RunParamBuilderNames.Enhancer.sorterMutateStandard
                    false
                    parameterSpans
                    QueryCatalogNames.sorterMutateMerge
                    SetupEnhancerFunctionNames.sorterMutate
                    [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
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

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()


