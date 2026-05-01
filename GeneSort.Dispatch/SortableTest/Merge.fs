namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1


module Merge =

    let private standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters %host.Project.ProjectName)
        rp.WithProjectName(Some host.Project.ProjectName)
          .WithRunName(Some host.Project.RunName)
          .WithRunFinished(Some false).WithId (Some qp.Id)

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

        let P1 = {
            ProjectName = "SortableTest" |> UMX.tag
            RunName = "Dev" |> UMX.tag
            ProjectDesc = "Int8 merge sorter test sets"
            DataFolder = "c:\\ProjectsV1\\SortableTest\\Merge\\Data"
            Spans = [
                (runParameters.sortingWidthKey, [32;] |> List.map string)
                (runParameters.sortableDataFormatKey, 
                [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
                (runParameters.mergeDimensionKey, [4;] |> List.map string)
                (runParameters.mergeSuffixTypeKey, 
                [mergeSuffixType.NoSuffix; mergeSuffixType.VV_1] |> List.map MergeSuffixType.toString)
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            RngFactory = rngFactory.LcgFactory
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
        }


        let P2 = {
            ProjectName = "SortableTest" |> UMX.tag
            RunName = "Prod" |> UMX.tag
            ProjectDesc = "Int8 merge sorter test sets"
            DataFolder = "c:\\ProjectsV1\\SortableTest\\Merge\\Data"
            Spans = [
                ( runParameters.sortingWidthKey, [16; 18; 24; 32; 36; 48; 64; 96; 128; 192; 256] 
                    |> List.map string)
                ( runParameters.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] 
                    |> List.map SortableDataFormat.toString)
                ( runParameters.mergeDimensionKey, [2; 3; 4; 6; 8] 
                    |> List.map string)
                ( runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix; mergeSuffixType.VV_1] 
                    |> List.map MergeSuffixType.toString)
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            RngFactory = rngFactory.LcgFactory
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
        }

    let Configs = Map.ofList [ ("P1", Specs.P1); ("P2", Specs.P2) ]

    let CreateHost (spec: runHostSpec) =
        let folder = spec.DataFolder |> UMX.tag
        let db = new GeneSortDbMp(folder) :> IGeneSortDb
        let proj = run.create spec.ProjectName spec.RunName spec.ProjectDesc 
                                [| outputDataType.RunParameters %spec.ProjectName; 
                                   outputDataType.SortableTestSet ""; |]
        runHost.Create db spec proj :> IRunHost
