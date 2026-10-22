module GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Soss_condense

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd


module Pool_128_condense =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
        let names = RunNaming.create()
        let sourceParameterSpans = [
            ParamKeys.selectedSorterPoolCountKey, [2] |> List.map string
            ParamKeys.mutationModKey, [0 .. 3] |> List.map string
        ]
        let executionParameterSpans = [
            ParamKeys.generationIntervalLastKey, [1] |> List.map string
        ]
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
            RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
            Source = evoContinuationSource.create {
                Run = Soss.Pool_128.Test ()
                GenerationInterval = 0
                ParameterSpans = sourceParameterSpans
            }
            ExecutionParameterSpans = executionParameterSpans
        }
        |> EvoCondense.createSoss
