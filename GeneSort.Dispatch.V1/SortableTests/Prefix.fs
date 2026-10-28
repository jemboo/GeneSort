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



module PrefixTest =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
    let executionFunctionName = ExecuteFunctionNames.sortableTests
    let executionQueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
    let setupFunctionName = SetupCatalogNames.prefix
    let setupQueryCatalogName = QueryCatalogNamesSetup.sortableTestPrefix

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.prefixLibIdKey,
                 [prefixLibId.create 24<sortingWidth> 3<stageLength> prefixLibVariant.PrefixA]
                 |> List.map PrefixLibId.toString)
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Bitv512 prefix sorter test sets"
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

module Prefix =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
    let executionFunctionName = ExecuteFunctionNames.sortableTests
    let executionQueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
    let setupFunctionName = SetupCatalogNames.prefix
    let setupQueryCatalogName = QueryCatalogNamesSetup.sortableTestPrefix

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Width24s  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.prefixLibIdKey,
                 [
                     prefixLibId.create 24<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA
                     prefixLibId.create 24<sortingWidth> 4<stageLength> prefixLibVariant.PrefixB
                     prefixLibId.create 24<sortingWidth> 3<stageLength> prefixLibVariant.PrefixA
                     prefixLibId.create 24<sortingWidth> 3<stageLength> prefixLibVariant.PrefixB
                 ]
                 |> List.map PrefixLibId.toString)
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Bitv512 prefix sorter test sets"
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
    let Width32  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.prefixLibIdKey,
                 [prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA]
                 |> List.map PrefixLibId.toString)
            ]
        run.SimpleRun (
            SimpleRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Bitv512 prefix sorter test sets"
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
        | Width24s
        | Width32

    let Configs = Map.ofList
                    [
                        (configType.Width24s, Width24s);
                        (configType.Width32, Width32);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()
