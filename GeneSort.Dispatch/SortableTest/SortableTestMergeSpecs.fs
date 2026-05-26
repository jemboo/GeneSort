namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonSortableTest


module SortableTetsMergeSpecs =
    
    // SortingWidths
    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [16; 18; 24; 32; 36; 48;] |> List.map string)

    let mediumSortingWidths = 
            (runParameters.sortingWidthKey,  [64; 96; 128; 192; 256]  |> List.map string)


    // MergeDimensions
    let allMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4; 6; 8] |> List.map string)
    let lowMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4;] |> List.map string)
    let highMergeDimensions = 
            (runParameters.mergeDimensionKey, [6; 8] |> List.map string)


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
            RunName = sprintf @"Merge_test_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Int8 merge sorter test sets"
            Spans = [
                smallSortingWidths
                onlyDataFormat
                lowMergeDimensions
                bothMergeSuffixTypes
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }


        let Merge_small (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge_small_%s" (ExecutorType.toString executorType) |> UMX.tag
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

        
        let Merge_medium_Ld (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge_medium_Ld_%s" (ExecutorType.toString executorType) |> UMX.tag
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

        
        let Merge_medium_Hd (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge_medium_Hd_%s" (ExecutorType.toString executorType) |> UMX.tag
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
        | Merge_Medium_Ld
        | Merge_Medium_Hd

    let Configs = Map.ofList 
                    [ 
                        (configType.Merge_Test, Specs.Merge_test); 
                        (configType.Merge_Small, Specs.Merge_small);
                        (configType.Merge_Medium_Ld, Specs.Merge_medium_Ld);
                        (configType.Merge_Medium_Hd, Specs.Merge_medium_Hd);
                    ]

    let getConfig (config: configType) (executorType: executorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType