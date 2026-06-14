namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.SortingOps


type sorterEvalExecutorType = 
    | GenStandard
    | GenMerge
    | FullReport
    | StageStatsReport
    | CeBinReport


module SorterEvalExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | FullReport -> "FullReport"
        | StageStatsReport -> "StageStatsReport"
        | CeBinReport -> "CeBinReport"


module CommonSorterEval =

    let projectName = "SorterEval" |> UMX.tag<projectName>

    let CollectSortableTests = true

    let ExcludeSelfCe = true |> UMX.tag<excludeSelfCe>

    let standardSortableDataFormat = sortableDataFormat.BitVector512
    let dataFormatInt8v512 = CommonSortableTest.dataFormatInt8v512

    // SorterCounts
    let smallSorterCount = (runParameters.sorterCountKey, ["100";] )
    let mediumSorterCount = (runParameters.sorterCountKey, ["1000";] )
    let largeSorterCount = (runParameters.sorterCountKey, ["10000";] )
    let extraLargeSorterCount = (runParameters.sorterCountKey, ["100000";] )
    let sorterEvalTypeV1 = 
            (runParameters.sorterEvalTypeKey, 
            [ sorterEvalType.V2 ;] |> List.map SorterEvalType.toString)

    let sorterEvalTypeV2 = 
            (runParameters.sorterEvalTypeKey, 
            [ sorterEvalType.V2 ;] |> List.map SorterEvalType.toString)


    let getStageLength 
                (smt: simpleSorterModelType) 
                (sw: int<sortingWidth>) : int<stageLength> =
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
        | 16 -> match smt with | Msuf4 -> 300 | _ -> 150
        | 18 -> 180
        | 20 -> 200
        | 22 -> 250
        | 24 -> 300
        | 32 -> match smt with | Msuf4 -> 600 | _ -> 300
        | 36 -> 350
        | 48 -> 400
        | 64 -> match smt with | Msuf4 -> 2000 | _ -> 600
        | 96 -> 800
        | 128 -> match smt with | Msuf4 -> 4000 | _ -> 1200
        | 192 -> 2000
        | 256 -> match smt with | Msuf4 -> 6000 | _ -> 3000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag


    let getStageLengthShort
                (smt: simpleSorterModelType) 
                (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 4 -> 5
        | 5 -> 5
        | 6 -> 10 
        | 7 -> 10 
        | 8 -> 20
        | 9 -> 20
        | 10 -> 30
        | 11 -> 40
        | 12 -> 50
        | 14 -> 60
        | 16 -> match smt with | Msuf4 -> 100 | _ -> 60
        | 18 -> 80
        | 20 -> 100
        | 22 -> 125
        | 24 -> 150
        | 32 -> match smt with | Msuf4 -> 200 | _ -> 150
        | 36 -> 150
        | 48 -> 200
        | 64 -> match smt with | Msuf4 -> 1000 | _ -> 300
        | 96 -> 800
        | 128 -> match smt with | Msuf4 -> 1500 | _ -> 600
        | 192 -> 2000
        | 256 -> match smt with | Msuf4 -> 2000 | _ -> 1000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag


    let getSimpleUniformSorterModelGen
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>)
            (simpleSorterModelType: simpleSorterModelType) 
                            : sorterModelGen =
            let stageLength = getStageLength simpleSorterModelType sortingWidth
            let rngFactory = rngType |> RngFactory.create
            SimpleSorterModelGen.makeUniform 
                                    rngFactory 
                                    sortingWidth 
                                    stageLength 
                                    simpleSorterModelType
                                    ExcludeSelfCe
                                    |> sorterModelGen.Simple






