namespace GeneSort.Sorter.Tests

open Xunit
open GeneSort.Core
open FSharp.UMX
open System
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable


type SortableBoolArrayTests() =

    [<Fact>]
    let ``Create with valid inputs succeeds`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        Assert.Equal<bool>([| true; false; true |], arr.Values)
        Assert.Equal(3<sortingWidth>, arr.SortingWidth)

    [<Fact>]
    let ``Create with mismatched length throws`` () =
        let ex = Assert.Throws<ArgumentException>(fun () -> sortableBoolArray.Create([| true; false |], 3<sortingWidth>) |> ignore)
        Assert.Equal("Values length (2) must equal order (3). (Parameter 'values')", ex.Message)

    [<Fact>]
    let ``DistanceSquared with same array returns zero`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let other = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let result = arr.DistanceSquared(other)
        Assert.Equal(0, result)

    [<Fact>]
    let ``DistanceSquared with different array computes correctly`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let other = sortableBoolArray.Create([| false; false; true |], 3<sortingWidth>)
        let result = arr.DistanceSquared(other)
        Assert.Equal(1, result)

    [<Fact>]
    let ``IsSorted returns true for sorted array`` () =
        let arr = sortableBoolArray.Create([| false; false; true |], 3<sortingWidth>)
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns false for unsorted array`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        Assert.False(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns true for single element`` () =
        let arr = sortableBoolArray.Create([| true |], 1<sortingWidth>)
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns true for empty array`` () =
        let arr = sortableBoolArray.Create([||], 0<sortingWidth>)
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``SortByCes sorts unsorted array correctly`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let ces = [| Ce.create 0 1; Ce.create 1 2 |]
        let useCounter = [| 0; 0 |]
        let sorted = arr.SortByCes ces 0 2 useCounter
        Assert.Equal<bool>([| false; true; true |], sorted.Values)
        Assert.True(sorted.IsSorted)
        Assert.Equal(arr.SortingWidth, sorted.SortingWidth)
        Assert.Equal<int>([| 1; 0 |], useCounter)

    [<Fact>]
    let ``SortByCes with empty ces returns copy`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let useCounter = [||]
        let sorted = arr.SortByCes [||] 0 0 useCounter
        Assert.Equal<bool>(arr.Values, sorted.Values)
        Assert.Equal(arr.SortingWidth, sorted.SortingWidth)
        Assert.Equal<int>([||], useCounter)

    [<Fact>]
    let ``SortByCes with partial extent sorts correctly`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let ces = [| Ce.create 0 1; Ce.create 1 2 |]
        let useCounter = [| 0; 0 |]
        let sorted = arr.SortByCes ces 0 1 useCounter
        Assert.Equal<bool>([| false; true; true |], sorted.Values)
        Assert.Equal<int>([| 1; 0 |], useCounter)

    [<Fact>]
    let ``SortByCesWithHistory captures sorting steps correctly`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let ces = [| Ce.create 0 1; Ce.create 1 2 |]
        let useCounter = [| 0; 0 |]
        let history = arr.SortByCesWithHistory ces 0 2 useCounter
        Assert.Equal(3, history.Length)
        Assert.Equal<bool>([| true; false; true |], history.[0].Values)
        Assert.Equal<bool>([| false; true; true |], history.[1].Values)
        Assert.Equal<bool>([| false; true; true |], history.[2].Values)
        Assert.True(history.[2].IsSorted)
        Assert.True(history |> Array.forall (fun x -> x.SortingWidth = arr.SortingWidth))
        Assert.Equal<int>([| 1; 0 |], useCounter)

    [<Fact>]
    let ``SortByCesWithHistory with partial extent captures correctly`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let ces = [| Ce.create 0 1; Ce.create 1 2 |]
        let useCounter = [| 0; 0 |]
        let history = arr.SortByCesWithHistory ces 1 1 useCounter
        Assert.Equal(2, history.Length)
        Assert.Equal<bool>([| true; false; true |], history.[0].Values)
        Assert.Equal<bool>([| true; false; true |], history.[1].Values)
        Assert.Equal<int>([| 0; 0 |], useCounter)

    [<Fact>]
    let ``getAllSortableBoolArrays with sortingWidth 0 returns single empty array`` () =
        let arrays = SortableBoolArray.getAllSortableBoolArrays 0<sortingWidth>
        Assert.Equal(1, arrays.Length)
        Assert.Equal<bool>([||], arrays.[0].Values)
        Assert.Equal(0<sortingWidth>, arrays.[0].SortingWidth)

    [<Fact>]
    let ``getAllSortableBoolArrays with sortingWidth 1 returns two arrays`` () =
        let arrays = SortableBoolArray.getAllSortableBoolArrays 1<sortingWidth>
        Assert.Equal(2, arrays.Length)
        let expected = [|
            sortableBoolArray.Create([| false |], 1<sortingWidth>)
            sortableBoolArray.Create([| true |], 1<sortingWidth>)
        |]
        Assert.Equal<sortableBoolArray[]>(expected, arrays)

    [<Fact>]
    let ``getAllSortableBoolArrays with sortingWidth 2 returns four arrays`` () =
        let arrays = SortableBoolArray.getAllSortableBoolArrays 2<sortingWidth>
        Assert.Equal(4, arrays.Length)
        let expected = [|
            sortableBoolArray.Create([| false; false |], 2<sortingWidth>)
            sortableBoolArray.Create([| true; false |], 2<sortingWidth>)
            sortableBoolArray.Create([| false; true |], 2<sortingWidth>)
            sortableBoolArray.Create([| true; true |], 2<sortingWidth>)
        |]
        Assert.Equal<sortableBoolArray[]>(expected, arrays)

    [<Fact>]
    let ``getAllSortableBoolArrays with sortingWidth 3 returns eight arrays`` () =
        let arrays = SortableBoolArray.getAllSortableBoolArrays 3<sortingWidth>
        Assert.Equal(8, arrays.Length)

    [<Fact>]
    let ``Equality with same values returns true`` () =
        let arr1 = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let arr2 = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        Assert.True(arr1.Equals(arr2))
        Assert.True(arr1.Equals(arr2 :> obj))
        Assert.Equal(arr1.GetHashCode(), arr2.GetHashCode())

    [<Fact>]
    let ``Equality with different values returns false`` () =
        let arr1 = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let arr2 = sortableBoolArray.Create([| false; false; true |], 3<sortingWidth>)
        Assert.False(arr1.Equals(arr2))
        Assert.False(arr1.Equals(arr2 :> obj))

    [<Fact>]
    let ``Equality with different order returns false`` () =
        let arr1 = sortableBoolArray.Create([| true; false |], 2<sortingWidth>)
        let arr2 = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        Assert.False(arr1.Equals(arr2))
        Assert.False(arr1.Equals(arr2 :> obj))

    [<Fact>]
    let ``Equality with different type returns false`` () =
        let arr = sortableBoolArray.Create([| true; false |], 2<sortingWidth>)
        let obj = obj()
        Assert.False(arr.Equals(obj))

    [<Fact>]
    let ``getMergeSortTestCases with sortingWidth 2 returns correct test cases`` () =
        let arrays = SortableBoolArray.getMergeSortTestCases 2<sortingWidth>
        let expected = [|
            sortableBoolArray.Create([| false; false |], 2<sortingWidth>) // [false], [false]
            sortableBoolArray.Create([| false; true |], 2<sortingWidth>)  // [false], [true]
            sortableBoolArray.Create([| true; false |], 2<sortingWidth>)  // [true], [false]
            sortableBoolArray.Create([| true; true |], 2<sortingWidth>)   // [true], [true]
        |]
        Assert.Equal(4, arrays.Length)
        Assert.Equal<sortableBoolArray[]>(expected, arrays)
        Assert.True(arrays |> Array.forall (fun x -> 
            let half = int x.SortingWidth / 2
            x.IsSorted || (ArrayProperties.isSorted (x.Values.[0 .. half-1]) && ArrayProperties.isSorted (x.Values.[half ..]))))

    [<Fact>]
    let ``getMergeSortTestCases with sortingWidth 4 returns correct test cases`` () =
        let arrays = SortableBoolArray.getMergeSortTestCases 4<sortingWidth>
        Assert.Equal(9, arrays.Length) // (2+1) * (2+1) = 9
        let expected = [|
            sortableBoolArray.Create([| false; false; false; false |], 4<sortingWidth>) // [false; false], [false; false]
            sortableBoolArray.Create([| false; false; false; true |], 4<sortingWidth>)  // [false; false], [false; true]
            sortableBoolArray.Create([| false; false; true; true |], 4<sortingWidth>)   // [false; false], [true; true]
            sortableBoolArray.Create([| false; true; false; false |], 4<sortingWidth>)  // [false; true], [false; false]
            sortableBoolArray.Create([| false; true; false; true |], 4<sortingWidth>)   // [false; true], [false; true]
            sortableBoolArray.Create([| false; true; true; true |], 4<sortingWidth>)    // [false; true], [true; true]
            sortableBoolArray.Create([| true; true; false; false |], 4<sortingWidth>)   // [true; true], [false; false]
            sortableBoolArray.Create([| true; true; false; true |], 4<sortingWidth>)    // [true; true], [false; true]
            sortableBoolArray.Create([| true; true; true; true |], 4<sortingWidth>)     // [true; true], [true; true]
        |]
        Assert.Equal<sortableBoolArray[]>(expected, arrays)
        Assert.True(arrays |> Array.forall (fun x -> 
            let half = int x.SortingWidth / 2
            x.IsSorted || (ArrayProperties.isSorted (x.Values.[0 .. half-1]) && ArrayProperties.isSorted (x.Values.[half ..]))))