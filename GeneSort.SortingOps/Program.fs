// For more information see https://aka.ms/fsharp-console-apps
open System
open FSharp.UMX
open GeneSort.Model.Sortable
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.SortingLib.Sorter

let ceUseSoruceTester() =

    let yow1 ="[0, 64, (0, 15)]; 
               [1, 64, (1, 14)]; 
               [2, 64, (2, 13)]; 
               [3, 64, (3, 12)]; 
               [4, 64, (4, 11)]; 
               [5, 64, (5, 10)]; [6, 64, (6, 9)]; [7, 128, (7, 8)]; [8, 64, (0, 3)]; [9, 64, (1, 2)]; [10, 64, (4, 7)]; [11, 64, (5, 6)]; [12, 64, (8, 11)]; [13, 64, (9, 10)]; [14, 64, (12, 15)]; [15, 64, (13, 14)]; [16, 32, (0, 1)]; [17, 72, (2, 12)]; [18, 72, (3, 13)]; [19, 64, (4, 5)]; [20, 96, (6, 7)]; [21, 96, (8, 9)]; [22, 64, (10, 11)]; [23, 32, (14, 15)]; [24, 8, (0, 4)]; [25, 56, (1, 6)]; [26, 96, (2, 5)]; [27, 96, (3, 8)]; [28, 96, (7, 12)]; [29, 56, (9, 14)]; [30, 96, (10, 13)]; [31, 8, (11, 15)]; [34, 60, (2, 3)]; [39, 60, (12, 13)]; [41, 42, (1, 4)]; [44, 104, (5, 10)]; [46, 128, (7, 8)]; [47, 42, (11, 14)]; [54, 92, (6, 9)]; [59, 111, (3, 4)]; [63, 111, (11, 12)]; [76, 116, (5, 6)]; [78, 116, (9, 10)]; [81, 12, (1, 2)]; [82, 114, (4, 7)]; [84, 114, (8, 11)]; [87, 12, (13, 14)]; [98, 49, (2, 3)]; [99, 114, (4, 5)]; [100, 125, (6, 7)]; [101, 125, (8, 9)]; [102, 114, (10, 11)]; [103, 49, (12, 13)]; [114, 70, (3, 4)]; [115, 122, (5, 6)]; [117, 122, (9, 10)]; [118, 70, (11, 12)]; [124, 122, (6, 8)]; [125, 122, (7, 9)]; [270, 128, (7, 8)]"


    let sorterId = Guid.NewGuid() |> UMX.tag<sorterId>
    let sortingWidth = 16 |> UMX.tag<sortingWidth>
    let sorter = CeUse.ceUseStringToSorter sorterId sortingWidth yow1
    let ceBlock = CeBlock.fromSorter sorter


    let sortableTestId = Guid.NewGuid() |> UMX.tag<sortableTestId>
    let sortableModel = msasF.create sortingWidth
    let sortableTest = sortableModel.MakeSortableBoolTest sortableTestId sortingWidth
                       |> sortableTest.Bools
    let collectNewSortableTests = true
    let ceBlockEval = CeBlockOps.evalWithSorterTest sortableTest ceBlock collectNewSortableTests
    None





printfn "Hello from F# yo"
