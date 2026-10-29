namespace GeneSort.Model.Mp.Sorter.Tests

open System
open FSharp.UMX
open FsUnit.Xunit   
open Xunit
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.SortingOps

type CeOpsTests() =

    let createCe (low: int) (hi: int) =
       ce.create low hi

    //[<Fact>]
    //let ``sortBy sorts bool array correctly and updates usage`` () =
    //    // Arrange
    //    let values = [| true; false; true |]
    //    let channelCount = 3<channelCount>
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
    //    let channelCount = 3<channelCount>
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
        let channelCount = 3<channelCount>
        let boolArrays = [|
            sortableBoolArray.create([| true; false; true |], channelCount)
            sortableBoolArray.create([| false; true; false |], channelCount)
            sortableBoolArray.create([| true; false; true |], channelCount) // Duplicate
            sortableBoolArray.create([| false; false; true |], channelCount) // Already sorted
        |]
        let sortableTests = 
                sortableBinaryTests.create 
                        (Guid.NewGuid() |> UMX.tag<sortableTestsId>)
                        channelCount
                        boolArrays |> sortableTests.Bools
        let ceBlk = ceBlock.create (Guid.NewGuid() |> UMX.tag<ceBlockId>) channelCount [| createCe 0 1 |]
        
        // Act
        let collectNewSortableTests = false |> UMX.tag<collectNewSortableTests>
        let ceBlockEval = CeBlockOps.evalWithSorterTest sortableTests ceBlock.Empty ceBlk collectNewSortableTests
        
        // Assert
        let boolTests = match (ceBlockEval).SortableTests.Value with | sortableTests.Bools bt -> bt | _ -> failwith "Expected Bools"
        boolTests.SortableBinaryArrays.Length |> should equal 1 // Duplicate removed
        boolTests.SortableBinaryArrays |> Array.forall (fun sba -> sba.Values = [| false; true; false |]) |> should be True
        %ceBlockEval.CeLength |> should be (greaterThanOrEqualTo 1) // At least one swap occurred

    [<Fact>]
    let ``evalWithSorterTests handles int arrays and removes duplicates`` () =
        // Arrange
        let channelCount = 3<channelCount>
        let symbolSetSize = 2<symbolSetSize>
        let intArrays = [|
            sortableIntArray.create([| 1; 0; 1 |], channelCount, symbolSetSize)
            sortableIntArray.create([| 0; 1; 0 |], channelCount, symbolSetSize)
            sortableIntArray.create([| 1; 0; 1 |], channelCount, symbolSetSize) // Duplicate
            sortableIntArray.create([| 0; 0; 1 |], channelCount, symbolSetSize) // Already sorted
        |]
        let sortableTests = 
            sortableIntTests.create 
                (Guid.NewGuid() |> UMX.tag<sortableTestsId>) 
                channelCount
                intArrays |> sortableTests.Ints

        let ceBlk = ceBlock.create (Guid.NewGuid() |> UMX.tag<ceBlockId>) channelCount [| createCe 0 1 |]
        
        // Act
        let collectNewSortableTests = false |> UMX.tag<collectNewSortableTests>
        let ceBlockEval = CeBlockOps.evalWithSorterTest sortableTests ceBlock.Empty ceBlk collectNewSortableTests
        
        // Assert
        let intTests = match (ceBlockEval).SortableTests.Value with | sortableTests.Ints it -> it | _ -> failwith "Expected Ints"
        intTests.SortableIntArrays.Length |> should equal 1
        %ceBlockEval.CeLength |> should be (greaterThanOrEqualTo 1) // At least one swap occurred



    [<Fact>]
    let ``evalWithSorterTests preserves channel count and type`` () =
        // Arrange
        let channelCount = 2<channelCount>
        let symbolSetSize = 2<symbolSetSize>
        let intArrays = [| sortableIntArray.create([| 1; 0 |], channelCount, symbolSetSize) |]
        let sortableTests = (sortableIntTests.create 
                                (Guid.NewGuid() |> UMX.tag<sortableTestsId>)
                                channelCount
                                intArrays ) |> sortableTests.Ints

        let ceBlk = ceBlock.create (Guid.NewGuid() |> UMX.tag<ceBlockId>) channelCount [| createCe 0 1 |]

        // Act
        let collectNewSortableTests = false |> UMX.tag<collectNewSortableTests>
        let result = CeBlockOps.evalWithSorterTest sortableTests ceBlock.Empty ceBlk collectNewSortableTests
        
        // Assert
        let intTests = match result.SortableTests.Value with | sortableTests.Ints it -> it | _ -> failwith "Expected Ints"
        intTests.ChannelCount |> should equal channelCount
        intTests.SortableDataFormat |> should equal sortableDataFormat.IntArray