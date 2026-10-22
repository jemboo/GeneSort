module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_append

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd



module Pool_4_append =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Debug () : run =
        let names = RunNaming.create()
        let sourceParameterSpans =
            [
                ParamKeys.mutationModKey, [0 .. 2] |> List.map string
            ]
        let executionParameterSpans =
            [
                ParamKeys.mutationModKey, [0 .. 2] |> List.map string
                ParamKeys.sorterCountPerPoolSetKey, [32; 64] |> List.map string
                ParamKeys.generationIntervalLastKey, [3] |> List.map string
            ]
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
            RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
            Source = evoContinuationSource.create {
                Run = Soss.Pool_4.Debug ()
                GenerationInterval = 0
                ParameterSpans = sourceParameterSpans
            }
            ExecutionParameterSpans = executionParameterSpans
        }
        |> EvoAppend.createSoss

module Pool_32_append =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Gens15 () : run =
        let names = RunNaming.create()
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
            RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
            Source = evoContinuationSource.create {
                Run = Soss.Pool_32.Gens15 ()
                GenerationInterval = 14
                ParameterSpans = ([ParamKeys.mutationModKey, [0 .. 3] |> List.map string])
            }
            ExecutionParameterSpans =
                [
                    ParamKeys.mutationModKey, [0 .. 3] |> List.map string
                    ParamKeys.sorterCountPerPoolSetKey, [256; 512] |> List.map string
                    ParamKeys.generationIntervalLastKey, [20] |> List.map string
                ]
        }
        |> EvoAppend.createSoss

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Debug_Append () : run =
        let names = RunNaming.create()
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
            RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
            Source = evoContinuationSource.create {
                Run = Pool_4_append.Debug ()
                GenerationInterval = 2
                ParameterSpans = [ParamKeys.mutationModKey, ["0"]]
            }
            ExecutionParameterSpans =
                [
                    ParamKeys.mutationModKey, [0 .. 3] |> List.map string
                    ParamKeys.sorterCountPerPoolSetKey, ["64"]
                    ParamKeys.generationIntervalLastKey, ["3"]
                ]
        }
        |> EvoAppend.createSoss
