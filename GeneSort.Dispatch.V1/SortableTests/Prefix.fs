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
                    FunctionName = ExecuteFunctionNames.sortableTests
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
                }
                Setup = {
                    FunctionName = SetupCatalogNames.prefix
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sortableTestPrefix
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
                }
            }
        )

module Prefix =

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
                    FunctionName = ExecuteFunctionNames.sortableTests
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
                }
                Setup = {
                    FunctionName = SetupCatalogNames.prefix
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sortableTestPrefix
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
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
                    FunctionName = ExecuteFunctionNames.sortableTests
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix
                    EnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
                }
                Setup = {
                    FunctionName = SetupCatalogNames.prefix
                    ParameterSpans = parameterSpans
                    QueryCatalogName = QueryCatalogNamesSetup.sortableTestPrefix
                    EnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
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
