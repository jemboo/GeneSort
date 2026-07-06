namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestSpecsMerge =

    let private standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters host.Run.RunName)
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)

    let private mergeDimensionDividesSortingWidth (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%sw % %md = 0) then Some rp else None

    let private limitForMergeFillType (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let ft = rp.GetMergeSuffixType().Value
        if (ft.IsNoSuffix && %sw > 64) then None else Some rp

    let private limitForMergeDimension (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%md > 6 && %sw > 144) then None else Some rp

    let standardParamMapFilter (rp: runParameters) = 
        Some rp
        |> Option.bind mergeDimensionDividesSortingWidth
        |> Option.bind limitForMergeFillType
        |> Option.bind limitForMergeDimension



    module Specs =

        let Merge_Test  (executorType: sortableTestExecutorType) : runHostSpec = {
            databaseName = SortableMergeTestDb.dbName
            runName = sprintf @"Merge-Test_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Int8 merge sorter test sets"
            spans = [
                testMergeSortingWidths
                dataFormatInt8v512
                testMergeDimensions
                noSuffixSuffixType
            ]
            filter = mergeDimensionDividesSortingWidth
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let Merge_Small (executorType: sortableTestExecutorType) : runHostSpec = {
            databaseName = SortableMergeTestDb.dbName
            runName = sprintf @"Merge-Small_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Merge_Small sorter test sets"
            spans = [
                smallMergeSortingWidths
                dataFormatInt8v512
                allMergeDimensions
                noSuffixSuffixType
            ]
            filter = mergeDimensionDividesSortingWidth
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        
        let Merge_MediumLd (executorType: sortableTestExecutorType) : runHostSpec = {
            databaseName = SortableMergeTestDb.dbName
            runName = sprintf @"Merge-MediumLd_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Merge_MediumLd sorter test sets"
            spans = [
                mediumMergeSortingWidths
                dataFormatInt8v512
                lowMergeDimensions
                noSuffixSuffixType
            ]
            filter = mergeDimensionDividesSortingWidth
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        
        let Merge_MediumHd (executorType: sortableTestExecutorType) : runHostSpec = {
            databaseName = SortableMergeTestDb.dbName
            runName = sprintf @"Merge-MediumHd_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Merge_MediumHd sorter test sets"
            spans = [
                sortingWidth96
                dataFormatInt8v512
                mergeDimension6
                noSuffixSuffixType
            ]
            filter = mergeDimensionDividesSortingWidth
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let Merge_LargeLd (executorType: sortableTestExecutorType) : runHostSpec = {
            databaseName = SortableMergeTestDb.dbName
            runName = sprintf @"Merge-LargeLd_%s" (SortableTestExecutorType.toString executorType) |> UMX.tag
            runDescription = "Merge_LargeLd sorter test sets"
            spans = [
                largeMergeSortingWidths
                dataFormatInt8v512
                mergeDimension2
                noSuffixSuffixType
            ]
            filter = mergeDimensionDividesSortingWidth
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


    type configType =
        | Merge_Test
        | Merge_Small
        | Merge_MediumLd
        | Merge_MediumHd
        | Merge_LargeLd

    let Configs = Map.ofList 
                    [ 
                        (configType.Merge_Test, Specs.Merge_Test); 
                        (configType.Merge_Small, Specs.Merge_Small);
                        (configType.Merge_MediumLd, Specs.Merge_MediumLd);
                        (configType.Merge_MediumHd, Specs.Merge_MediumHd);
                        (configType.Merge_LargeLd, Specs.Merge_LargeLd);
                    ]

    let getRunHostSpec (config: configType) (executorType: sortableTestExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType