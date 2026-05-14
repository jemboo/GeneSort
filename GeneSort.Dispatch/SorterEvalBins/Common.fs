namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
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



module Common =

    let projectName = "SorterEvalBins" |> UMX.tag<projectName>

    let randomSimpleDatabaseName = "RandomSimple" |> UMX.tag<databaseName>

    let randomMergeDatabaseName = "RandomMerge" |> UMX.tag<databaseName>


    let randomSimpleDatabaseFolder = 
                    "c:\\Projects\\SorterEvalBins\\RandomSimple\\Data"
                     |> UMX.tag<pathToRootFolder>

    let randomMergeDatabaseFolder = 
                    "c:\\Projects\\SorterEvalBins\\RandomMerge\\Data"
                     |> UMX.tag<pathToRootFolder>

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
        | 18 -> match smt with | Msuf4 -> 600 | _ -> 150
        | 24 -> match smt with | Msuf4 -> 600 | _ -> 200
        | 32 -> match smt with | Msuf4 -> 600 | _ -> 300
        | 36 -> match smt with | Msuf4 -> 600 | _ -> 300
        | 48 -> match smt with | Msuf4 -> 1200 | _ -> 400
        | 64 -> match smt with | Msuf4 -> 2000 | _ -> 600
        | 96 -> match smt with | Msuf4 -> 2800 | _ -> 800
        | 128 -> match smt with | Msuf4 -> 4000 | _ -> 1200
        | 192 -> match smt with | Msuf4 -> 4800 | _ -> 2000
        | 256 -> match smt with | Msuf4 -> 6000 | _ -> 3000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag






