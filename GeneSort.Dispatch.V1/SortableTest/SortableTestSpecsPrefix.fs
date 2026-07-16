namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestSpecsPrefix =

    let private standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters host.Run.RunName)
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)



    module Specs =

        let Prefix_Test  (executorType: sortableTestExecutorType) : runHostSpec = {
            databaseName = SortableTestDbs.Prefix.dbName
            runName = sprintf @"Prefix-TestA_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
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
        | Prefix_Test

    let Configs = Map.ofList 
                    [ 
                        (configType.Prefix_Test, Specs.Prefix_Test);
                    ]

    let getRunHostSpec (config: configType) (executorType: sortableTestExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType