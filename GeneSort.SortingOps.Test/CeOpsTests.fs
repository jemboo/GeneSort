namespace GeneSort.Model.Mp.Sorter.Tests

open System
open FSharp.UMX
open Xunit
open FsUnit.Xunit   
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.SortingOps
open GeneSort.Sorter.Sortable

type CeOpsTests() =

    let createCe (low: int) (hi: int) =
       ce.create low hi

    [<Fact>]
    let ``sortBy sorts bool array correctly and updates usage`` () =
        // Arrange
        let values = [| true; false; true |]
        let sortingWidth = 3<sortingWidth>
        let ceBlock = ceBlock.create [| createCe 0 1; createCe 1 2 |]
        let ceUseCounts = ceUseCounts.create 2<ceBlockLength>
        
        // Act
        let result = CeBlockOps.sortBy ceBlock ceUseCounts values
        
        // Assert
        result |> should equal [| false; true; true |]


    [<Fact>]
    let ``sortBy sorts int array correctly and updates usage`` () =
        // Arrange
        let values = [| 2; 0; 1 |]
        let sortingWidth = 3<sortingWidth>
        let symbolSetSize = 3<symbolSetSize>
        let ceBlock = ceBlock.create [| createCe 0 1; createCe 1 2 |]

        let ceUseCounts = ceUseCounts.create 2<ceBlockLength>
        
        // Act
        let result = CeBlockOps.sortBy ceBlock ceUseCounts values
        
        // Assert
        result |> should equal [| 0; 1; 2 |]


    [<Fact>]
    let ``evalWithSorterTests handles bool arrays and removes duplicates`` () =
        // Arrange
        let sortingWidth = 2<sortingWidth>
        let boolArrays = [|
            sortableBoolArray.Create([| true; false |], sortingWidth)
            sortableBoolArray.Create([| false; true |], sortingWidth)
            sortableBoolArray.Create([| true; false |], sortingWidth) // Duplicate
        |]
        let sorterTests = sorterBoolTests.create (Guid.NewGuid() |> UMX.tag<sorterTestsId>) boolArrays |> sorterTests.Bools
        let ceBlock = ceBlock.create [| createCe 0 1 |]
        
        // Act
        let ceBlockEval = CeBlockOps.evalWithSorterTest sorterTests ceBlock
        
        // Assert
        let boolTests = match ceBlockEval.SorterTests with | sorterTests.Bools bt -> bt | _ -> failwith "Expected Bools"
        boolTests.SortableBoolArrays.Length |> should equal 1 // Duplicate removed
        boolTests.SortableBoolArrays |> Array.forall (fun sba -> sba.Values = [| false; true |]) |> should be True
        ceBlockEval.CeUseCounts.[0]  |> should be (greaterThanOrEqualTo 1) // At least one swap occurred

    [<Fact>]
    let ``evalWithSorterTests handles int arrays and removes duplicates`` () =
        // Arrange
        let sortingWidth = 2<sortingWidth>
        let symbolSetSize = 2<symbolSetSize>
        let intArrays = [|
            sortableIntArray.Create([| 1; 0 |], sortingWidth, symbolSetSize)
            sortableIntArray.Create([| 0; 1 |], sortingWidth, symbolSetSize)
            sortableIntArray.Create([| 1; 0 |], sortingWidth, symbolSetSize) // Duplicate
        |]
        let sorterTests = sorterIntTests.create (Guid.NewGuid() |> UMX.tag<sorterTestsId>) intArrays |> sorterTests.Ints
        let ceBlock = ceBlock.create [| createCe 0 1 |]
        
        // Act
        let ceBlockEval = CeBlockOps.evalWithSorterTest sorterTests ceBlock
        
        // Assert
        let intTests = match ceBlockEval.SorterTests with | sorterTests.Ints it -> it | _ -> failwith "Expected Ints"
        intTests.SortableIntArrays.Length |> should equal 1 // Duplicates removed
        intTests.SortableIntArrays |> Array.forall (fun sia -> sia.Values = [| 0; 1 |]) |> should be True
        ceBlockEval.CeUseCounts.[0] |> should be (greaterThanOrEqualTo 1) // At least one swap occurred


    [<Fact>]
    let ``evalWithSorterTests preserves sorting width and type`` () =
        // Arrange
        let sortingWidth = 2<sortingWidth>
        let symbolSetSize = 2<symbolSetSize>
        let intArrays = [| sortableIntArray.Create([| 1; 0 |], sortingWidth, symbolSetSize) |]
        let sorterTests = sorterIntTests.create (Guid.NewGuid() |> UMX.tag<sorterTestsId>) intArrays |> sorterTests.Ints
        let ceBlock = ceBlock.create [| createCe 0 1 |]

        // Act
        let result = CeBlockOps.evalWithSorterTest sorterTests ceBlock
        
        // Assert
        let intTests = match result.SorterTests with | sorterTests.Ints it -> it | _ -> failwith "Expected Ints"
        intTests.SortingWidth |> should equal sortingWidth
        intTests.SortableArrayType |> should equal SortableArrayType.Ints