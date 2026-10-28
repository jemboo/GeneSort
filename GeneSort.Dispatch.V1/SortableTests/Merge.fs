namespace GeneSort.Dispatch.V1.SortableTests

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1


module Merge =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
    let executionFunctionName = ExecuteFunctionNames.sortableTests
    let executionQueryCatalogName = QueryCatalogNamesRun.sortableTestMerge
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
    let setupFunctionName = SetupCatalogNames.merge
    let setupQueryCatalogName = QueryCatalogNamesSetup.sortableTestMerge


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.mergeLibIdKey,
                 [
                     mergeLibId.create 32<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 32<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantB
                     mergeLibId.create 32<sortingWidth> 4<mergeDimension> sorterLibVariant.VariantA
                     mergeLibId.create 32<sortingWidth> 8<mergeDimension> sorterLibVariant.VariantA
                 ]
                 |> List.map MergeLibId.toString)
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Int8 merge sorter test sets"
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
        | Test
        | Small

    let Configs = Map.ofList
                    [
                        (configType.Test, Test);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
