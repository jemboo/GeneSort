module GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Standard_condense

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd



module Pool_32_condense =

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
            SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
            RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
            Source = evoContinuationSource.create {
                SourceRun = Standard.Pool_32.Test ()
                GenerationInterval = 2
                SourceMutationMods = [0 .. 3]
                SelectedSorterPoolCount = 2
            }
            ExecutionParameterSpans = executionParameterSpans
        }
        |> EvoCondense.createStandard
