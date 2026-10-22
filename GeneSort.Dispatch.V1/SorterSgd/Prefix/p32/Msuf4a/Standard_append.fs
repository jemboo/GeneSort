module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard_append

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd


[<MethodImpl(MethodImplOptions.NoInlining)>]
let Test () : run =
    let names = RunNaming.create()
    let sourceParameterSpans =
        [
            ParamKeys.mutationModKey, [0] |> List.map string
        ]
    let executionParameterSpans =
        [
            ParamKeys.mutationModKey, [0 .. 2] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, [128; 256] |> List.map string
            ParamKeys.paraRateKey,    [1.25;] |> List.map string
            ParamKeys.selfSymRateKey, [2.25;]  |> List.map string
            ParamKeys.generationIntervalLastKey, [1] |> List.map string
        ]
    evoContinuationConfig.create {
        ProjectName = names.ProjectName
        DatabaseName = names.DatabaseName
        RunName = names.RunName
        SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
        RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
        Source = evoContinuationSource.create {
            Run = Standard.Pool_32.Test ()
            GenerationInterval = 0
            ParameterSpans = sourceParameterSpans
        }
        ExecutionParameterSpans = executionParameterSpans
    }
    |> EvoAppend.createStandard


module Pool_128 =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let V1 () : run =
        let names = RunNaming.create()

        let sourceParameterSpans =
            [
                ParamKeys.mutationModKey, [0 .. 2] |> List.map string
            ]
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
            SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
            RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
            Source = evoContinuationSource.create {
                Run = Test ()
                GenerationInterval = 1
                ParameterSpans = sourceParameterSpans
            }
            ExecutionParameterSpans = executionParameterSpans
        }
        |> EvoAppend.createStandard

module Pool_512 =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let V1 () : run =
        let names = RunNaming.create()
        let sourceParameterSpans =
            [
                ParamKeys.mutationModKey, [0 .. 2] |> List.map string
            ]
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
            SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
            RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
            Source = evoContinuationSource.create {
                Run = Test ()
                GenerationInterval = 1
                ParameterSpans = sourceParameterSpans
            }
            ExecutionParameterSpans = executionParameterSpans
        }
        |> EvoAppend.createStandard
