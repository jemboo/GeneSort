module GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Simple.Standard_condense

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd
open GeneSort.Dispatch.V1



module Pool_32_condense =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc32.Cc32Params.channelCount

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Simple.SimpleParams.simpleSetupParams

    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        // Rank only these source MutationMods during preparation.
        let executionParameterSpans =
            [
                ParamKeys.generationIntervalLastKey, [3] |> List.map string
            ]
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            RunQueryCatalogName = executionQueryCatalogName
            Source = evoContinuationSource.create {
                SourceRun = Standard.Pool_32.Test ()
                GenerationInterval = 2
                SourceMutationMods = [0 .. 3]
                SelectedSorterPoolCount = 2
            }
            ExecutionParameterSpans = executionParameterSpans
        }
        |> EvoCondense.createStandard

    let description = "Condense saved pools for 32-input Msuf4a Standard evolution (Pool_32_condense)."


    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.cc32.p4a.Msuf4.Simple.Standard_condense.Pool_32_condense", description, channelCount, executionQueryCatalogName, [|
            runExample("Test", Test)
        |])
