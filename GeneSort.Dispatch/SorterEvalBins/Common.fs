namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core


type executorType = 
    | Standard
    | Merge
    | FullReport
    | BinsReport

module ExecutorType =
    let toString = function
        | Standard -> "Standard"
        | Merge -> "Merge"
        | FullReport -> "FullReport"
        | BinsReport -> "BinsReport"



module yab =

    let projectRngType = rngType.Lcg

    let CollectSortableTests = true

    let ExcludeSelfCe = true |> UMX.tag<excludeSelfCe>

    let standardSortableDataFormat = sortableDataFormat.BitVector512

    let mergeSortableDataFormat = sortableDataFormat.Int8Vector512

    let getStandardStageLength (sw: int<sortingWidth>) : int<stageLength> =
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

