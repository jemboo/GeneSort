namespace GeneSort.Dispatch.V1.SorterMutate

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterMutate


module MutateSpecsRs =

    let sorterEvalSelectionType =
            (ParamKeys.seedSorterPoolSelectionTypeKey,
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)


    module RandomStandard =

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test_Msrs ()  : run =
            let names = RunNaming.create()
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
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Mutation analysis for Msrs"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterMutate
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterMutateStandard
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandardFormat
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.mutantStandard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterMutateStandard
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
                    }
                }
            )

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let Test_Msuf4 ()  : run =
            let names = RunNaming.create()
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
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Mutation analysis for Msuf4"
                    AllowOverwrite = false
                    Execution = {
                        FunctionName = ExecuteFunctionNames.sorterMutate
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesRun.sorterMutateStandard
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandardFormat
                    }
                    Setup = {
                        FunctionName = SetupCatalogNames.mutantStandard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterMutateStandard
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
                    }
                }
            )

    type configType =
        | Test_Msrs
        | Rand_Small
        | Rand_Medium

    let Configs = Map.ofList
                    [
                        (configType.Test_Msrs, RandomStandard.Test_Msrs);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
