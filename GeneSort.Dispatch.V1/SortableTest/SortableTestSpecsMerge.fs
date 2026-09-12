namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestSpecsMerge =

    let private standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)

    let private mergeDimensionDividesSortingWidth (rp: runParameters) =
        let sw = rp.GetMergeLibId().Value.SortingWidth
        let md = rp.GetMergeLibId().Value.MergeDimension
        if (%sw % %md = 0) then Some rp else None



    module Specs =

        let Merge_Test  (executorType: sortableTestExecutorType) : runHostSpec = {
            databaseName = SortableTestDbs.Merge.dbName
            runName = sprintf @"Merge-Test_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Int8 merge sorter test sets"
            spans = [
                mergeLib_Merge32s
                dataFormatInt8v512
            ]
            filter = mergeDimensionDividesSortingWidth
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


    type configType =
        | Merge_Test
        | Merge_Small

    let Configs = Map.ofList 
                    [ 
                        (configType.Merge_Test, Specs.Merge_Test); 
                    ]

    let getRunHostSpec (config: configType) (executorType: sortableTestExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType