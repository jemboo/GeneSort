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
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1


module Standard =

    let channelCount = 16<GeneSort.Sorting.channelCount>

    let sorterEvalModule = sprintf "SorterEval.Standard.Channels%d" (int channelCount)

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandardFormat
    let executionFunctionName = ExecuteFunctionNames.sorterMutate
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterMutateStandard
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
    let setupFunctionName = SetupCatalogNames.mutantStandard
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterMutateStandard

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Msrs ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sorterEvalModuleKey, [sorterEvalModule])
                (ParamKeys.excludeSelfCeKey, ["true"])
                (ParamKeys.collectSortableTestsKey, ["false"])
                (ParamKeys.simpleSorterModelTypeKey, [simpleSorterModelType.Msrs] |> List.map SimpleSorterModelType.toString)
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V1] |> List.map SorterEvalType.toString)
                (ParamKeys.seedSorterPoolSelectionTypeKey, [sorterSelectionType.ValueSpan 5<sorterCount>] |> List.map SorterSelectionType.toString)
                (ParamKeys.sorterEvalMeasureKey,
                 [
                     ceStMeasure.create {
                         StageWeight = 1.1<stageWeight>
                         FilterUnsorted = true |> UMX.tag<filterUnsorted>
                         FilterReflectionSymmetric = false |> UMX.tag<filterReflectionSymmetric>
                         StageCrossingWeight = 0.3 |> UMX.tag<stageCrossingWeight>
                     } |> sorterEvalMeasure.CeSt
                 ]
                 |> List.map SorterEvalMeasure.toCompactString)
                (ParamKeys.mutatorParamsKey,
                 [MutatorParams.msrsParamsRL 16<channelCount> rngType.Lcg mutatorVariant.V1]
                 |> List.map MutatorParams.toString)
                (ParamKeys.modificationRateKey, [0.90] |> List.map string)
                (ParamKeys.channelCountKey, [16] |> List.map string)
                (ParamKeys.sorterChildCountKey, [1000] |> List.map string)
                (ParamKeys.mutationModKey, [1] |> List.map string)
            ]
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Mutation analysis for Msrs"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Msuf4 ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sorterEvalModuleKey, [sorterEvalModule])
                (ParamKeys.excludeSelfCeKey, ["true"])
                (ParamKeys.collectSortableTestsKey, ["false"])
                (ParamKeys.simpleSorterModelTypeKey, [simpleSorterModelType.Msuf4] |> List.map SimpleSorterModelType.toString)
                (ParamKeys.rngTypeKey, [rngType.Lcg] |> List.map RngType.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.sorterEvalTypeKey, [sorterEvalType.V1] |> List.map SorterEvalType.toString)
                (ParamKeys.seedSorterPoolSelectionTypeKey, [sorterSelectionType.ValueSpan 5<sorterCount>] |> List.map SorterSelectionType.toString)
                (ParamKeys.sorterEvalMeasureKey,
                 [
                     ceStMeasure.create {
                         StageWeight = 1.1<stageWeight>
                         FilterUnsorted = true |> UMX.tag<filterUnsorted>
                         FilterReflectionSymmetric = false |> UMX.tag<filterReflectionSymmetric>
                         StageCrossingWeight = 0.3 |> UMX.tag<stageCrossingWeight>
                     } |> sorterEvalMeasure.CeSt
                 ]
                 |> List.map SorterEvalMeasure.toCompactString)
                (ParamKeys.mutatorParamsKey,
                 [MutatorParams.msrsParamsRL 16<channelCount> rngType.Lcg mutatorVariant.V1]
                 |> List.map MutatorParams.toString)
                (ParamKeys.modificationRateKey, [0.90] |> List.map string)
                (ParamKeys.channelCountKey, [16] |> List.map string)
                (ParamKeys.sorterChildCountKey, [1000] |> List.map string)
                (ParamKeys.mutationModKey, [1] |> List.map string)
            ]
        run.PreparedRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Mutation analysis for Msuf4"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            }
        )

    [<RequireQualifiedAccess>]
    type configType =
        | Msrs
        | Small
        | Medium

    let Configs = Map.ofList
                    [
                        (configType.Msrs, Msrs);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()

    let description = "Compare standard sorting-network mutation configurations."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterMutate.Standard", description, channelCount, executionQueryCatalogName, [|
            runExample("Msrs", Msrs)
            runExample("Msuf4", Msuf4)
        |])
