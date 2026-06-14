namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonParams


module SortableTestMergeSpecs =

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

        let Merge_Test  (executorType: executorType) : runHostSpec = {
            DatabaseName = SortableMergeTestDb.dbName
            RunName = sprintf @"Merge-Test_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Int8 merge sorter test sets"
            Spans = [
                testMergeSortingWidth
                dataFormatInt8v512
                lowMergeDimensions
                noSuffixSuffixType
            ]
            Filter = mergeDimensionDividesSortingWidth
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }


        let Merge_Small (executorType: executorType) : runHostSpec = {
            DatabaseName = SortableMergeTestDb.dbName
            RunName = sprintf @"Merge-Small_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge_Small sorter test sets"
            Spans = [
                smallMergeSortingWidths
                dataFormatInt8v512
                allMergeDimensions
                noSuffixSuffixType
            ]
            Filter = mergeDimensionDividesSortingWidth
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        
        let Merge_MediumLd (executorType: executorType) : runHostSpec = {
            DatabaseName = SortableMergeTestDb.dbName
            RunName = sprintf @"Merge-MediumLd_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge_MediumLd sorter test sets"
            Spans = [
                mediumMergeSortingWidths
                dataFormatInt8v512
                lowMergeDimensions
                noSuffixSuffixType
            ]
            Filter = mergeDimensionDividesSortingWidth
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        
        let Merge_MediumHd (executorType: executorType) : runHostSpec = {
            DatabaseName = SortableMergeTestDb.dbName
            RunName = sprintf @"Merge-MediumHd_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge_MediumHd sorter test sets"
            Spans = [
                mediumMergeSortingWidths
                dataFormatInt8v512
                highMergeDimensions
                noSuffixSuffixType
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }

        let Merge_LargeLd (executorType: executorType) : runHostSpec = {
            DatabaseName = SortableMergeTestDb.dbName
            RunName = sprintf @"Merge-LargeLd_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge_LargeLd sorter test sets"
            Spans = [
                largeMergeSortingWidths
                dataFormatInt8v512
                mergeDimension2
                noSuffixSuffixType
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
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

    let getRunHostSpec (config: configType) (executorType: executorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType