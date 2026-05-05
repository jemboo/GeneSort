namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1


module SimpleRandom =

    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [4;5;6;7;8;9;10;11;12] |> List.map string)

    let testSortingWidths = 
            (runParameters.sortingWidthKey, [8;] |> List.map string)

    let getStageLength (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 4 -> 15
        | 5 -> 25
        | 6 -> 40 
        | 7 -> 50 
        | 8 -> 60
        | 9 -> 70
        | 10 -> 80
        | 11 -> 90
        | 12 -> 100
        | 14 -> 120
        | 16 -> 150
        | 18 -> 180
        | 20 -> 200
        | 22 -> 250
        | 24 -> 300
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag


    let private standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let sw = rp.GetSortingWidth().Value
        let qp = host.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters %host.Project.ProjectName)
       
        let sl = getStageLength %sw
        
        rp.WithProjectName(Some host.Project.ProjectName)
          .WithRunName(Some host.Project.RunName)
          .WithRunFinished(Some false)
          .WithStageLength(Some sl)
          .WithCollectSortableTests(Some true)
          .WithId (Some qp.Id)

    let private standardSorterModelTypeFilter (rp: runParameters) =
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let has2factor = (%sw % 2 = 0)
            return! if (smt = simpleSorterModelType.Msce) || has2factor then Some rp else None
        }


    module Specs =

        let Small_dev (executorType: Executor.executorType) : runHostSpec = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = sprintf @"Small_Dev_%s" (Executor.ExecutorType.toString executorType) |> UMX.tag
            ProjectDesc = "Standard binning for Msce/Mssi/Msrs"
            DataFolder = "c:\\Projects\\SorterEvalBins\\SimpleRandom\\Small\\Data"
            Spans = [
                testSortingWidths
                (runParameters.simpleSorterModelTypeKey, 
                    [simpleSorterModelType.Msce;] |> List.map SimpleSorterModelType.toString)
                (runParameters.sortableDataFormatKey, 
                    [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (runParameters.sorterCountKey, ["10000"])
                (runParameters.rngTypeKey, [rngType.Lcg;] |> List.map RngType.toString)
            ]
            GetStageLength = getStageLength 
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
            ExecutorType = executorType
        }

        let Small (executorType: Executor.executorType) : runHostSpec = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = sprintf @"Small_%s" (Executor.ExecutorType.toString executorType) |> UMX.tag
            ProjectDesc = "Standard binning for Msce/Mssi/Msrs"
            DataFolder = "c:\\Projects\\SorterEvalBins\\SimpleRandom\\Small\\Data"
            Spans = [
                smallSortingWidths
                (runParameters.simpleSorterModelTypeKey, 
                [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)
                (runParameters.sortableDataFormatKey, 
                [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (runParameters.sorterCountKey, ["10000"])
                (runParameters.rngTypeKey, [rngType.Lcg; rngType.Net; rngType.Smx] |> List.map RngType.toString)
            ]
            GetStageLength = getStageLength
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
            ExecutorType = executorType
        }

        let Standard_dev (executorType: Executor.executorType) : runHostSpec = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = sprintf @"Standard_Dev_%s" (Executor.ExecutorType.toString executorType) |> UMX.tag
            ProjectDesc = "Standard binning for Msce/Mssi/Msrs"
            DataFolder = "c:\\Projects\\SorterEvalBins\\SimpleRandom\\Standard\\Data"
            Spans = [
                (runParameters.sortingWidthKey, 
                ["12"])
                (runParameters.simpleSorterModelTypeKey, 
                [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)
                (runParameters.sortableDataFormatKey, 
                [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (runParameters.sorterCountKey, ["10000"])
                (runParameters.rngTypeKey, [rngType.Lcg; rngType.Net; rngType.Smx] |> List.map RngType.toString)
            ]
            GetStageLength = getStageLength
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
            ExecutorType = executorType
        }

        let Standard (executorType: Executor.executorType) : runHostSpec = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = sprintf @"Standard_%s" (Executor.ExecutorType.toString executorType) |> UMX.tag
            ProjectDesc = "Standard binning for Msce/Mssi/Msrs"
            DataFolder = "c:\\Projects\\SorterEvalBins\\SimpleRandom\\Standard\\Data"
            Spans = [
                (runParameters.sortingWidthKey, 
                ["12"])
                (runParameters.simpleSorterModelTypeKey, 
                [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)
                (runParameters.sortableDataFormatKey, 
                [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (runParameters.sorterCountKey, ["10000"])
                (runParameters.rngTypeKey, [rngType.Lcg; rngType.Net; rngType.Smx] |> List.map RngType.toString)
            ]
            GetStageLength = getStageLength
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
            ExecutorType = executorType
        }

    let Configs = Map.ofList 
                    [ 
                        ("Small_dev", Specs.Small_dev); 
                        ("Small", Specs.Small) 
                        ("Standard_dev", Specs.Standard_dev); 
                        ("Standard", Specs.Standard) 
                    ]

    let CreateHost (spec: runHostSpec) =
        let folder = spec.DataFolder |> UMX.tag
        let db = new GeneSortDbMp(folder) :> IGeneSortDb
        let proj = run.create spec.ProjectName spec.RunName spec.ProjectDesc 
                                [| outputDataType.RunParameters %spec.ProjectName; outputDataType.SorterEvalBins ""; |]
        runHost.Create db spec proj :> IRunHost
