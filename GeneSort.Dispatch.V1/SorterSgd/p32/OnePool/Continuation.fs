namespace GeneSort.Dispatch.V1.SorterSgd.p32.OnePool
open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.p32.OnePool.Common

module Continuation =

    let channelCount = 32<GeneSort.Sorting.channelCount>
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters =
        [
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
            ParamKeys.prefixLibIdKey, [GeneSort.SortingLib.Sorter.prefixLibId.create channelCount 4<GeneSort.Sorting.stageLength> GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixA |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]

    let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
    let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend

    let description = "Single-pool tests that append or condense a saved evolutionary state."

    module StandardAppend =

        let executionQueryCatalogName = standardAppendExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestStandardAppend () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Standard.TestStandard ()
                    GenerationInterval = 0
                    SourceMutationMods = [0]
                    SelectedSorterPoolCount = 1
                }
                ExecutionParameterSpans = [
                    ParamKeys.mutationModKey, [0 .. 2] |> List.map string
                    ParamKeys.generationIntervalLastKey, ["1"]
                ]
            }
            |> EvoAppend.createStandard

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestStandardCondense () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Standard.TestStandard ()
                    GenerationInterval = 0
                    SourceMutationMods = [0 .. 3]
                    SelectedSorterPoolCount = 2
                }
                ExecutionParameterSpans = [
                    ParamKeys.generationIntervalLastKey, ["1"]
                ]
            }
            |> EvoCondense.createStandard

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.p32.OnePool.Continuation.StandardAppend", description, channelCount, executionQueryCatalogName, [|
                runExample("TestStandardAppend", TestStandardAppend)
                runExample("TestStandardCondense", TestStandardCondense)
            |])

    module SossAppend =

        let executionQueryCatalogName = sossAppendExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestSossAppend () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Soss.TestSoss ()
                    GenerationInterval = 0
                    SourceMutationMods = [0]
                    SelectedSorterPoolCount = 1
                }
                ExecutionParameterSpans = [
                    ParamKeys.mutationModKey, [0 .. 2] |> List.map string
                    ParamKeys.generationIntervalLastKey, ["1"]
                ]
            }
            |> EvoAppend.createSoss

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestSossCondense () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Soss.TestSoss ()
                    GenerationInterval = 0
                    SourceMutationMods = [0 .. 3]
                    SelectedSorterPoolCount = 2
                }
                ExecutionParameterSpans = [
                    ParamKeys.mutationModKey, [0 .. 3] |> List.map string
                    ParamKeys.generationIntervalLastKey, ["1"]
                ]
            }
            |> EvoCondense.createSoss

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.p32.OnePool.Continuation.SossAppend", description, channelCount, executionQueryCatalogName, [|
                runExample("TestSossAppend", TestSossAppend)
                runExample("TestSossCondense", TestSossCondense)
            |])
