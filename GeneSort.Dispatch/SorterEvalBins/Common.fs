namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1


type evalExecutorType = 
    | Standard
    | Merge
    | FullReport
    | BinsReport

module EvalExecutorType =
    let toString = function
        | Standard -> "Standard"
        | Merge -> "Merge"
        | FullReport -> "FullReport"
        | BinsReport -> "BinsReport"



module Yab =

    let projectName = "SorterEvalBins" |> UMX.tag<projectName>

    let randomSimpleDatabaseName = "RandomSimple" |> UMX.tag<databaseName>

    let randomMergeDatabaseName = "RandomMerge" |> UMX.tag<databaseName>


    let randomSimpleDatabaseFolder = 
                    "c:\\Projects\\SortableTest\\RandomSimple\\Data"
                     |> UMX.tag<pathToRootFolder>

    let randomMergeDatabaseFolder = 
                    "c:\\Projects\\SortableTest\\RandomMerge\\Data"
                     |> UMX.tag<pathToRootFolder>

    let projectRngType = rngType.Lcg

    let CollectSortableTests = true

    let ExcludeSelfCe = true |> UMX.tag<excludeSelfCe>

    let standardSortableDataFormat = sortableDataFormat.BitVector512

    let mergeSortableDataFormat = sortableDataFormat.Int8Vector512

    let getStandardStageLength (smt: simpleSorterModelType) (sw: int<sortingWidth>) : int<stageLength> =
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


    let getMergeStageLength 
                    (smt: simpleSorterModelType) 
                    (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 16 -> 100
        | 32 -> match smt with | Msuf4 -> 600 | _ -> 300
        | 48 -> match smt with | Msuf4 -> 1200 | _ -> 400
        | 64 -> match smt with | Msuf4 -> 2000 | _ -> 600
        | 96 -> match smt with | Msuf4 -> 2800 | _ -> 800
        | 128 -> match smt with | Msuf4 -> 4000 | _ -> 1200
        | 196 -> match smt with | Msuf4 -> 4800 | _ -> 2000
        | 256 -> match smt with | Msuf4 -> 6000 | _ -> 3000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag



    let makeQueryParamsForSimple 
                (repl: int<replNumber> option) 
                (sortingWidth: int<sortingWidth> option)
                (mergeDimension: int<mergeDimension> option) 
                (mergeFillType: mergeSuffixType option)
                (sortableDataFormat: sortableDataFormat option) 
                (outputDataType: outputDataType) : queryParams =

        queryParams.create 
            projectName
            repl 
            outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
               (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
               (runParameters.mergeSuffixTypeKey, mergeFillType 
                    |> Option.map MergeSuffixType.toString |> UmxExt.stringOptionToString );
               (runParameters.sortableDataFormatKey, sortableDataFormat 
                    |> Option.map SortableDataFormat.toString |> UmxExt.stringOptionToString ); |]


    let makeQueryParamsFromRunParamsForSimple 
                (runParams: runParameters) 
                (outputDataType: outputDataType) : queryParams =
            makeQueryParamsForSimple 
                    (runParams.GetRepl()) 
                    (runParams.GetSortingWidth()) 
                    (runParams.GetMergeDimension())
                    (runParams.GetMergeSuffixType()) 
                    (runParams.GetSortableDataFormat()) 
                    outputDataType


    let makeQueryParamsForMerge
                (repl: int<replNumber> option) 
                (sortingWidth: int<sortingWidth> option)
                (smt: simpleSorterModelType option)
                (mergeDimension: int<mergeDimension> option) 
                (mergeSuffixType: mergeSuffixType option)
                (sortableDataFormat: sortableDataFormat option) 
                (outputDataType: outputDataType) : queryParams =

        queryParams.create 
            projectName
            repl 
            outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
               (runParameters.simpleSorterModelTypeKey, smt |> Option.map SimpleSorterModelType.toString |> UmxExt.stringOptionToString);
               (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
               (runParameters.mergeSuffixTypeKey, mergeSuffixType 
                    |> Option.map MergeSuffixType.toString |> UmxExt.stringOptionToString );
               (runParameters.sortableDataFormatKey, sortableDataFormat 
                    |> Option.map SortableDataFormat.toString |> UmxExt.stringOptionToString ); |]


    let makeQueryParamsFromRunParamsForMerge
                (runParams: runParameters) 
                (outputDataType: outputDataType) : queryParams =
            makeQueryParamsForMerge 
                    (runParams.GetRepl()) 
                    (runParams.GetSortingWidth()) 
                    (runParams.GetSimpleSorterModelType()) 
                    (runParams.GetMergeDimension())
                    (runParams.GetMergeSuffixType()) 
                    (runParams.GetSortableDataFormat()) 
                    outputDataType



    let randomSimpleDb = new GeneSortDbMp(randomSimpleDatabaseFolder, 
                                makeQueryParamsFromRunParamsForSimple)

    let randomMergeDb = new GeneSortDbMp(randomMergeDatabaseFolder, 
                                makeQueryParamsFromRunParamsForMerge)

    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ (randomSimpleDatabaseName, randomSimpleDb :> IGeneSortDb);
          (randomMergeDatabaseName, randomMergeDb :> IGeneSortDb) ]
        |> Map.ofList


    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)





