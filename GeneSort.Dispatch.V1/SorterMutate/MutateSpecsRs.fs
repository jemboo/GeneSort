namespace GeneSort.Dispatch.V1.SorterMutate

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
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
open GeneSort.Project.V1.Parameters


module MutateSpecsRs = 

    let sorterEvalSelectionType = 
            (runParameters.seedSorterPoolSelectionTypeKey, 
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)
    

    module Specs =

        let Test_Msrs ()  : run =
            let parameterSpans =
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
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = SorterMutateDbs.RandomStandard.Uniform.dbName
                    ProjectName = SorterMutateDbs.projectName
                    RunName = (sprintf @"Rand-Test_Msrs%s" "GenStandard" |> UMX.tag)
                    Description = "Mutation analysis for Msrs"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterMutate
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterMutateStandard
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandardFormat
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.mutantStandard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterMutateStandard
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
                        OutputTypes = [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                    }
                }
            )


        let Test_Msuf4 ()  : run =
            let parameterSpans =
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
            run.SimpleRun (
                SimpleRun.create {
                    DatabaseName = SorterMutateDbs.RandomStandard.Uniform.dbName
                    ProjectName = SorterMutateDbs.projectName
                    RunName = (sprintf @"Rand-Test_Msuf4%s" "GenStandard" |> UMX.tag)
                    Description = "Mutation analysis for Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterMutate
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterMutateStandard
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandardFormat
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.mutantStandard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNames.sorterMutateStandard
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
                        OutputTypes = [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
                    }
                }
            )





    type configType =
        | Test_Msrs
        | Rand_Small
        | Rand_Medium

    let Configs = Map.ofList 
                    [ 
                        (configType.Test_Msrs, Specs.Test_Msrs); 
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()


