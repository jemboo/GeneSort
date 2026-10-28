module GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.Standard_append

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd
open GeneSort.Dispatch.V1



let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend

[<MethodImpl(MethodImplOptions.NoInlining)>]
let Test () : run =
    let names = RunNaming.create()
    let executionParameterSpans =
        [
            ParamKeys.mutationModKey, [0 .. 2] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, [128] |> List.map string
            ParamKeys.paraRateKey,    [1.25;] |> List.map string
            ParamKeys.selfSymRateKey, [2.25;]  |> List.map string
            ParamKeys.generationIntervalLastKey, [1] |> List.map string
        ]
    evoContinuationConfig.create {
        ProjectName = names.ProjectName
        DatabaseName = names.DatabaseName
        RunName = names.RunName
        RunQueryCatalogName = executionQueryCatalogName
        Source = evoContinuationSource.create {
            SourceRun = Standard.Pool_32.Test ()
            GenerationInterval = 0
            SourceMutationMods = [0]
            SelectedSorterPoolCount = 1
        }
        ExecutionParameterSpans = executionParameterSpans
    }
    |> EvoAppend.createStandard


module Pool_128 =

    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let V1 () : run =
        let names = RunNaming.create()

        let executionParameterSpans =
            [
                ParamKeys.mutationModKey, [0; 1] |> List.map string
                ParamKeys.sorterCountPerPoolSetKey, [256;] |> List.map string
                ParamKeys.generationIntervalLastKey, [3] |> List.map string
            ]
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            RunQueryCatalogName = executionQueryCatalogName
            Source = evoContinuationSource.create {
                SourceRun = Test ()
                GenerationInterval = 1
                SourceMutationMods = [0 .. 2]
                SelectedSorterPoolCount = 3
            }
            ExecutionParameterSpans = executionParameterSpans
        }
        |> EvoAppend.createStandard

module Pool_512 =

    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let V1 () : run =
        let names = RunNaming.create()
        let executionParameterSpans =
            [
                ParamKeys.mutationModKey, [0; 1] |> List.map string
                ParamKeys.sorterCountPerPoolSetKey, [512;] |> List.map string
                ParamKeys.generationIntervalLastKey, [3] |> List.map string
            ]
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            RunQueryCatalogName = executionQueryCatalogName
            Source = evoContinuationSource.create {
                SourceRun = Test ()
                GenerationInterval = 1
                SourceMutationMods = [0 .. 2]
                SelectedSorterPoolCount = 3
            }
            ExecutionParameterSpans = executionParameterSpans
        }
        |> EvoAppend.createStandard
