namespace GeneSort.Dispatch.V1.SorterMutate


open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Dispatch.V1.SortableTest



type sorterMutateExecutorType = 
    | GenStandard
    | GenMerge
    | FullReport
    | BinsReport


module SorterMutateExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | FullReport -> "FullReport"
        | BinsReport -> "BinsReport"



module CommonSorterMutate = 

    let projectName = "SorterMutate" |> UMX.tag<projectName>
    let queryName = "SorterMutate" |> UMX.tag<databaseName>

    let randomStandardDatabaseName = "RandomSimple" |> UMX.tag<databaseName>

    let randomMergeDatabaseName = "RandomMerge" |> UMX.tag<databaseName>


    let randomStandardDatabaseFolder = 
                    "c:\\Projects\\SorterMutate\\RandomSimple\\Data"
                     |> UMX.tag<pathToRootFolder>

    let randomMergeDatabaseFolder = 
                    "c:\\Projects\\SorterMutate\\RandomMerge\\Data"
                     |> UMX.tag<pathToRootFolder>

    let CollectSortableTests = true

    let ExcludeSelfCe = true |> UMX.tag<excludeSelfCe>

    let standardSortableDataFormat = sortableDataFormat.BitVector512
    let mergeSortableDataFormat = CommonSortableTest.projectSortableDataFormat
    let projectRngType = rngType.Lcg
    let projectRngFactory = projectRngType |> RngFactory.create


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



    //let getSimpleUniformSorterModelGen
    //        (rngType: rngType) 
    //        (sortingWidth: int<sortingWidth>)
    //        (simpleSorterModelType: simpleSorterModelType) 
    //                        : sorterModelGen =
    //        let stageLength = getStageLength simpleSorterModelType sortingWidth
    //        let rngFactory = rngType |> RngFactory.create
    //        SimpleSorterModelGen.makeUniform 
    //                                rngFactory 
    //                                sortingWidth 
    //                                stageLength 
    //                                simpleSorterModelType
    //                                ExcludeSelfCe
    //                                |> sorterModelGen.Simple



