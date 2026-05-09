namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open yab


module SimpleRandom =

    //let smallSortingWidths = 
    //        (runParameters.sortingWidthKey, [4;5;6;7;8;9;10;11;12] |> List.map string)
    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [12] |> List.map string)
    //let mediumSortingWidths = 
    //        (runParameters.sortingWidthKey, [14;16;18;20;22] |> List.map string)

    let mediumSortingWidths = 
            (runParameters.sortingWidthKey, [24] |> List.map string)

    let testSorterCount = (runParameters.sorterCountKey, ["100";] )
    let smallSorterCount = (runParameters.sorterCountKey, ["1000";] )
    let mediumSorterCount = (runParameters.sorterCountKey, ["10000";] )
    let largeSorterCount = (runParameters.sorterCountKey, ["100000";] )

    let allSimpleSorterModelTypes = 
            (runParameters.simpleSorterModelTypeKey, SimpleSorterModelType.all() |> List.map SimpleSorterModelType.toString)



    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let sw = rp.GetSortingWidth().Value
        let qp = host.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters %host.Run.ProjectName)
        let sl = getStandardStageLength %sw
        
        rp.WithProjectName(Some host.Run.ProjectName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithRngType(Some yab.projectRngType)
          .WithId (Some qp.Id)

    
    let private standardSorterModelTypeFilter (rp: runParameters) =
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let has2factor = (%sw % 2 = 0)
            let isPowerOf2 = (%sw &&& (%sw - 1) = 0)
            let isGt4 = (%sw > 4)
            let validMsce = (smt = simpleSorterModelType.Msce)
            let validMssi = (smt = simpleSorterModelType.Mssi) && has2factor
            let validMsrs = (smt = simpleSorterModelType.Msrs) && has2factor
            let validMsuf4 = (smt = simpleSorterModelType.Msuf4) && isPowerOf2 && isGt4
            return! if validMsce || validMssi || validMsrs || validMsuf4 then Some rp else None
        }


    module Specs =

        let Small_dev (executorType: executorType) : runHostSpec = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = sprintf @"Small_Dev_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            DataFolder = "c:\\Projects\\SorterEvalBins\\SimpleRandom\\Small\\Data"
            Spans = [
                smallSortingWidths
                allSimpleSorterModelTypes
                testSorterCount
            ]
            GetStageLength = getStandardStageLength 
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            ExecutorType = executorType
        }

        let Small (executorType: executorType) : runHostSpec = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = sprintf @"Small_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            DataFolder = "c:\\Projects\\SorterEvalBins\\SimpleRandom\\Small\\Data"
            Spans = [
                smallSortingWidths
                allSimpleSorterModelTypes
                largeSorterCount
            ]
            GetStageLength = getStandardStageLength
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            ExecutorType = executorType
        }

        let Medium_dev (executorType: executorType) : runHostSpec = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = sprintf @"Medium_Dev_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            DataFolder = "c:\\Projects\\SorterEvalBins\\SimpleRandom\\Medium_dev\\Data"
            Spans = [
                mediumSortingWidths
                allSimpleSorterModelTypes
                testSorterCount
            ]
            GetStageLength = getStandardStageLength
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            ExecutorType = executorType
        }

        let Medium (executorType: executorType) : runHostSpec = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = sprintf @"Medium_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            DataFolder = "c:\\Projects\\SorterEvalBins\\SimpleRandom\\Medium\\Data"
            Spans = [
                mediumSortingWidths
                allSimpleSorterModelTypes
                mediumSorterCount
            ]
            GetStageLength = getStandardStageLength
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            ExecutorType = executorType
        }

    let Configs = Map.ofList 
                    [ 
                        ("Small_dev", Specs.Small_dev); 
                        ("Small", Specs.Small) 
                        ("Medium_dev", Specs.Medium_dev); 
                        ("Medium", Specs.Medium) 
                    ]

    let CreateHost (spec: runHostSpec) =
        let folder = spec.DataFolder |> UMX.tag
        let db = new GeneSortDbMp(folder) :> IGeneSortDb
        let proj = run.create spec.ProjectName spec.RunName spec.RunDescription 
                                [| outputDataType.RunParameters %spec.ProjectName; outputDataType.SorterEvalBins ""; |]
        runHost.Create db spec proj :> IRunHost
