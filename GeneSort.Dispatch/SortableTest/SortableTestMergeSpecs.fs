namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Project.V1
open GeneSort.Dispatch.V1


module SortableTetsMergeSpecs =
    
    // SortingWidths
    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [16; 18; 24; 32; 36; 48; 64] |> List.map string)

    let mediumSortingWidths = 
            (runParameters.sortingWidthKey,  [96; 128; 192; 256]  |> List.map string)


    // MergeDimensions
    let allMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4; 6; 8] |> List.map string)
    let lowMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4;] |> List.map string)
    let highMergeDimensions = 
            (runParameters.mergeDimensionKey, [6; 8] |> List.map string)


    // DataFormats
    let onlyDataFormat = 
            (runParameters.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)


    // MergeSuffixTypes
    let bothMergeSuffixTypes = 
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix; mergeSuffixType.VV_1;] |> List.map MergeSuffixType.toString)
    let noSuffixSuffixType = 
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
    let vv1SuffixType = 
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.VV_1] |> List.map MergeSuffixType.toString)


    let private standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters host.Run.RunName)
        rp.WithProjectName(Some host.Run.ProjectName)
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

        let Merge_Test_test  (executorType: executorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.mergeDatabaseName
            RunName = sprintf @"Merge_Test_test%s" (ExecutorType.toString executorType) |> UMX.tag
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
        }


        let Merge_Test_small (executorType: executorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.mergeDatabaseName
            RunName = sprintf @"Merge_Test_small%s" (ExecutorType.toString executorType) |> UMX.tag
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
        }

        
        let Merge_Test_medium_Ld (executorType: executorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.mergeDatabaseName
            RunName = sprintf @"Merge_Test_medium_Ld%s" (ExecutorType.toString executorType) |> UMX.tag
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
        }

        
        let Merge_Test_medium_Hd (executorType: executorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.mergeDatabaseName
            RunName = sprintf @"Merge_Test_medium_Hd%s" (ExecutorType.toString executorType) |> UMX.tag
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
        }


    type configType =
        | MergeTest_Test
        | MergeTest_Small
        | MergeTest_Medium_Ld
        | MergeTest_Medium_Hd

    let Configs = Map.ofList 
                    [ 
                        (configType.MergeTest_Test, Specs.Merge_Test_test); 
                        (configType.MergeTest_Small, Specs.Merge_Test_small);
                        (configType.MergeTest_Medium_Ld, Specs.Merge_Test_medium_Ld);
                        (configType.MergeTest_Medium_Hd, Specs.Merge_Test_medium_Hd);
                    ]

    let getConfig (config: configType) (executorType: executorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType