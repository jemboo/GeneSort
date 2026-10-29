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

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.prefixLibIdKey,
                 [prefixLibId.create 24<channelCount> 3<stageLength> prefixLibVariant.PrefixA]
                 |> List.map PrefixLibId.toString)
            ]
        run.DirectRun (
            DirectRun.create {
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
            }
        )


    let description = "Small test configurations for prefix-based sortable inputs."

    let runType =
        GeneSort.Dispatch.V1.runType("SortableTests.PrefixTest", description, [|
            runExample("Test", Test)
        |])

module Prefix =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
    let executionFunctionName = ExecuteFunctionNames.sortableTests
    let executionQueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Channels24  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.prefixLibIdKey,
                 [
                     prefixLibId.create 24<channelCount> 4<stageLength> prefixLibVariant.PrefixA
                     prefixLibId.create 24<channelCount> 4<stageLength> prefixLibVariant.PrefixB
                     prefixLibId.create 24<channelCount> 3<stageLength> prefixLibVariant.PrefixA
                     prefixLibId.create 24<channelCount> 3<stageLength> prefixLibVariant.PrefixB
                 ]
                 |> List.map PrefixLibId.toString)
            ]
        run.DirectRun (
            DirectRun.create {
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
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Channels32  () : run =
        let names = RunNaming.create()
        let parameterSpans =
            [
                (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (ParamKeys.prefixLibIdKey,
                 [prefixLibId.create 32<channelCount> 4<stageLength> prefixLibVariant.PrefixA]
                 |> List.map PrefixLibId.toString)
            ]
        run.DirectRun (
            DirectRun.create {
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
            }
        )

    [<RequireQualifiedAccess>]
    type configType =
        | Channels24
        | Channels32

    let Configs = Map.ofList
                    [
                        (configType.Channels24, Channels24);
                        (configType.Channels32, Channels32);
                    ]

    let getRun (config: configType) () : run =
        let specFunc = Configs.[config]
        specFunc ()

    let description = "Generate sortable test inputs using sorting-network prefixes."

    let runType =
        GeneSort.Dispatch.V1.runType("SortableTests.Prefix", description, [|
            runExample("Channels24", Channels24)
            runExample("Channels32", Channels32)
        |])
