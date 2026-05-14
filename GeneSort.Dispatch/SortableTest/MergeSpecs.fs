namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1


module SortableMergeSpecs =


    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [16; 18; 24; 32;] |> List.map string)

    let allSortingWidths = 
            (runParameters.sortingWidthKey,  [16; 18; 24; 32; 36; 48; 64; 96; 128; 192; 256]  |> List.map string)

    let smallMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4; 6; 8] |> List.map string)


    let allMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4; 6; 8] |> List.map string)

    let allDataFormats = 
            (runParameters.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)

    let allMergeFillTypes = 
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix; mergeSuffixType.VV_1;] |> List.map MergeSuffixType.toString)


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

        let Merge_dev  (executorType: executorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.mergeDatabaseName
            RunName = sprintf @"Merge_Dev%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Int8 merge sorter test sets"
            Spans = [
                smallSortingWidths
                allDataFormats
                smallMergeDimensions
                allMergeFillTypes
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
        }


        let Merge_small (executorType: executorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.mergeDatabaseName
            RunName = sprintf @"Merge_Prod%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Int8 merge sorter test sets"
            Spans = [
                allSortingWidths
                allDataFormats
                allMergeDimensions
                allMergeFillTypes
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
        }

    let Configs = Map.ofList [ ("Merge_dev", Specs.Merge_dev); ("Merge_small", Specs.Merge_small) ]
