namespace GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple
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
open GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.Common

module SameFamilyChain =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.Cc32Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = SimpleParams.simpleSetupParams

    let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
    let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend

    let description = "Chains of append and condense runs within the same workflow family."

    module StandardAppend =

        let executionQueryCatalogName = standardAppendExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestStandardAppendCondense () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Continuation.StandardAppend.TestStandardAppend ()
                    GenerationInterval = 1
                    SourceMutationMods = [0 .. 2]
                    SelectedSorterPoolCount = 2
                }
                ExecutionParameterSpans = [
                    ParamKeys.generationIntervalLastKey, ["2"]
                ]
            }
            |> EvoCondense.createStandard

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestStandardCondenseAppend () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Continuation.StandardAppend.TestStandardCondense ()
                    GenerationInterval = 1
                    SourceMutationMods = [0 .. 3]
                    SelectedSorterPoolCount = 2
                }
                ExecutionParameterSpans = [
                    ParamKeys.generationIntervalLastKey, ["2"]
                ]
            }
            |> EvoAppend.createStandard

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SameFamilyChain.StandardAppend", description, channelCount, executionQueryCatalogName, [|
                runExample("TestStandardAppendCondense", TestStandardAppendCondense)
                runExample("TestStandardCondenseAppend", TestStandardCondenseAppend)
            |])

    module SossAppend =

        let executionQueryCatalogName = sossAppendExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestSossAppendCondense () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Continuation.SossAppend.TestSossAppend ()
                    GenerationInterval = 1
                    SourceMutationMods = [0 .. 2]
                    SelectedSorterPoolCount = 2
                }
                ExecutionParameterSpans = [
                    ParamKeys.generationIntervalLastKey, ["2"]
                ]
            }
            |> EvoCondense.createSoss

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestSossCondenseAppend () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Continuation.SossAppend.TestSossCondense ()
                    GenerationInterval = 1
                    SourceMutationMods = [0 .. 3]
                    SelectedSorterPoolCount = 8
                }
                ExecutionParameterSpans = [
                    ParamKeys.generationIntervalLastKey, ["2"]
                ]
            }
            |> EvoAppend.createSoss

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.SameFamilyChain.SossAppend", description, channelCount, executionQueryCatalogName, [|
                runExample("TestSossAppendCondense", TestSossAppendCondense)
                runExample("TestSossCondenseAppend", TestSossCondenseAppend)
            |])
