namespace GeneSort.Model.Mp.Sorter.Tests

open System
open FSharp.UMX
open Xunit
open FsUnit.Xunit   
open GeneSort.Component
open GeneSort.Component.Sorter
open GeneSort.SortingOps
open GeneSort.Component.Sortable

type CeOpsTests() =

    let createCe (low: int) (hi: int) =
       ce.create low hi

    //[<Fact>]
    //let ``sortBy sorts bool array correctly and updates usage`` () =
    //    // Arrange
    //    let values = [| true; false; true |]
    //    let sortingWidth = 3<sortingWidth>
    //    let ceBlock = ceBlock.create [| createCe 0 1; createCe 1 2 |]
    //    let ceUseCounts = ceUseCounts.create 2<ceBlockLength>
        
    //    // Act
    //    let result = CeBlockOps.sortBy ceBlock ceUseCounts values
        
    //    // Assert
    //    result |> should equal [| false; true; true |]


    //[<Fact>]
    //let ``sortBy sorts int array correctly and updates usage`` () =
    //    // Arrange
    //    let values = [| 2; 0; 1 |]
    //    let sortingWidth = 3<sortingWidth>
    //    let symbolSetSize = 3<symbolSetSize>
    //    let ceBlock = ceBlock.create [| createCe 0 1; createCe 1 2 |]

    //    let ceUseCounts = ceUseCounts.create 2<ceBlockLength>
        
    //    // Act
    //    let result = CeBlockOps.sortBy ceBlock ceUseCounts values
        
    //    // Assert
    //    result |> should equal [| 0; 1; 2 |]


    [<Fact>]
    let ``evalWithSorterTests handles bool arrays and removes duplicates`` () =
        // Arrange
        let sortingWidth = 3<sortingWidth>
        let boolArrays = [|
            sortableBoolArray.create([| true; false; true |], sortingWidth)
            sortableBoolArray.create([| false; true; false |], sortingWidth)
            sortableBoolArray.create([| true; false; true |], sortingWidth) // Duplicate
            sortableBoolArray.create([| false; false; true |], sortingWidth) // Already sorted
        |]
        let sortableTests = 
                sortableBinaryTest.create 
                        (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                        sortingWidth
                        boolArrays |> sortableTest.Bools
        let ceBlock = ceBlock.create (Guid.NewGuid() |> UMX.tag<ceBlockId>) sortingWidth [| createCe 0 1 |]
        
        // Act
        let ceBlockEval = CeBlockOps.evalWithSorterTest sortableTests ceBlock
        
        // Assert
        let boolTests = match ceBlockEval.SortableTest.Value with | sortableTest.Bools bt -> bt | _ -> failwith "Expected Bools"
        boolTests.SortableBinaryArrays.Length |> should equal 1 // Duplicate removed
        boolTests.SortableBinaryArrays |> Array.forall (fun sba -> sba.Values = [| false; true; false |]) |> should be True
        %ceBlockEval.CeUseCounts.UsedCeCount |> should be (greaterThanOrEqualTo 1) // At least one swap occurred

    [<Fact>]
    let ``evalWithSorterTests handles int arrays and removes duplicates`` () =
        // Arrange
        let sortingWidth = 3<sortingWidth>
        let symbolSetSize = 2<symbolSetSize>
        let intArrays = [|
            sortableIntArray.create([| 1; 0; 1 |], sortingWidth, symbolSetSize)
            sortableIntArray.create([| 0; 1; 0 |], sortingWidth, symbolSetSize)
            sortableIntArray.create([| 1; 0; 1 |], sortingWidth, symbolSetSize) // Duplicate
            sortableIntArray.create([| 0; 0; 1 |], sortingWidth, symbolSetSize) // Already sorted
        |]
        let sortableTest = 
            sortableIntTest.create 
                (Guid.NewGuid() |> UMX.tag<sorterTestId>) 
                sortingWidth
                intArrays |> sortableTest.Ints

        let ceBlock = ceBlock.create (Guid.NewGuid() |> UMX.tag<ceBlockId>) sortingWidth [| createCe 0 1 |]
        
        // Act
        let ceBlockEval = CeBlockOps.evalWithSorterTest sortableTest ceBlock 
        
        // Assert
        let intTests = match ceBlockEval.SortableTest.Value with | sortableTest.Ints it -> it | _ -> failwith "Expected Ints"
        intTests.SortableIntArrays.Length |> should equal 1
        %ceBlockEval.CeUseCounts.UsedCeCount |> should be (greaterThanOrEqualTo 1) // At least one swap occurred



    [<Fact>]
    let ``evalWithSorterTests preserves sorting width and type`` () =
        // Arrange
        let sortingWidth = 2<sortingWidth>
        let symbolSetSize = 2<symbolSetSize>
        let intArrays = [| sortableIntArray.create([| 1; 0 |], sortingWidth, symbolSetSize) |]
        let sortableTest = (sortableIntTest.create 
                                (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                                sortingWidth
                                intArrays ) |> sortableTest.Ints

        let ceBlock = ceBlock.create (Guid.NewGuid() |> UMX.tag<ceBlockId>) sortingWidth [| createCe 0 1 |] 

        // Act
        let result = CeBlockOps.evalWithSorterTest sortableTest ceBlock
        
        // Assert
        let intTests = match result.SortableTest.Value with | sortableTest.Ints it -> it | _ -> failwith "Expected Ints"
        intTests.SortingWidth |> should equal sortingWidth
        intTests.SortableDataFormat |> should equal sortableDataFormat.IntArray