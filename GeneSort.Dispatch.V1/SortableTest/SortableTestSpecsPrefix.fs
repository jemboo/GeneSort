namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestSpecsPrefix =

    let private standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)



    module Specs =

        let Prefix_24s  (executorType: sortableTestExecutorType) : runHostSpec = {
            databaseName = SortableTestDbs.Prefix.dbName
            runName = sprintf @"Prefix-Test_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Bitv512 prefix sorter test sets"
            spans = [
                dataFomatBitv512
                sortableTestFilter_Prefix24s
            ]
            filter = (fun rp -> Some rp)
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }

        let Prefix_32  (executorType: sortableTestExecutorType) : runHostSpec = {
            databaseName = SortableTestDbs.Prefix.dbName
            runName = sprintf @"Prefix-Test_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Bitv512 prefix sorter test sets"
            spans = [
                dataFomatBitv512
                sortableTestFilter_Prefix32_4
            ]
            filter = (fun rp -> Some rp)
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


    type configType =
        | Prefix_24s
        | Prefix_32

    let Configs = Map.ofList 
                    [ 
                        (configType.Prefix_24s, Specs.Prefix_24s);
                        (configType.Prefix_32, Specs.Prefix_32);
                    ]

    let getRunHostSpec (config: configType) (executorType: sortableTestExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType