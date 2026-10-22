module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard_condense

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Dispatch.V1.SorterSgd



module Pool_32_condense =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        // Rank only these source MutationMods during preparation.
        let sourceParameterSpans =
            [
                ParamKeys.selectedSorterPoolCountKey, [2] |> List.map string
                ParamKeys.mutationModKey, [0 .. 3] |> List.map string
            ]
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
                Run = Standard.Pool_32.Test ()
                GenerationInterval = 2
                ParameterSpans = sourceParameterSpans
            }
            ExecutionParameterSpans = executionParameterSpans
        }
        |> EvoCondense.createStandard
