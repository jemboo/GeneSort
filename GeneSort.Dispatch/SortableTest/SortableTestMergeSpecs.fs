namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonSortableTest


module SortableTestMergeSpecs =
    
    // SortingWidths
    let testSortingWidths = 
            (runParameters.sortingWidthKey, [32; 64;] |> List.map string)

    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [16; 18; 24; 32; 36; 48;] |> List.map string)

    let singleSortingWidth = 
            (runParameters.sortingWidthKey, [32;] |> List.map string)

    let mediumSortingWidths = 
            (runParameters.sortingWidthKey,  [64; 96; 128; 192; 256]  |> List.map string)


    // MergeDimensions
    let allMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4; 6; 8] |> List.map string)
    let singleMergeDimension = 
            (runParameters.mergeDimensionKey, [8;] |> List.map string)
    let lowMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4;] |> List.map string)
    let highMergeDimensions = 
            (runParameters.mergeDimensionKey, [6; 8] |> List.map string)
    let testMergeDimensions = 
            (runParameters.mergeDimensionKey, [4; 8] |> List.map string)

    // DataFormats
    let onlyDataFormat = 
            (runParameters.sortableDataFormatKey, [projectSortableDataFormat] |> List.map SortableDataFormat.toString)


    // MergeSuffixTypes
    let bothMergeSuffixTypes = 
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix; mergeSuffixType.VV_1;] |> List.map MergeSuffixType.toString)
    let noSuffixSuffixType = 
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
    let vv1SuffixType = 
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.VV_1] |> List.map MergeSuffixType.toString)


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

        let Merge_test  (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge-test_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Int8 merge sorter test sets"
            Spans = [
                singleSortingWidth
                onlyDataFormat
                testMergeDimensions
                bothMergeSuffixTypes
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }


        let Merge_small (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge-small_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Int8 merge sorter test sets"
            Spans = [
                smallSortingWidths
                onlyDataFormat
                allMergeDimensions
                bothMergeSuffixTypes
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        
        let Merge_MediumLd (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge-MediumLd_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Int8 merge sorter test sets"
            Spans = [
                mediumSortingWidths
                onlyDataFormat
                lowMergeDimensions
                bothMergeSuffixTypes
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        
        let Merge_MediumHd (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge-MediumHd_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Int8 merge sorter test sets"
            Spans = [
                mediumSortingWidths
                onlyDataFormat
                highMergeDimensions
                vv1SuffixType
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

    let Configs = Map.ofList 
                    [ 
                        (configType.Merge_Test, Specs.Merge_test); 
                        (configType.Merge_Small, Specs.Merge_small);
                        (configType.Merge_MediumLd, Specs.Merge_MediumLd);
                        (configType.Merge_MediumHd, Specs.Merge_MediumHd);
                    ]

    let getConfig (config: configType) (executorType: executorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType