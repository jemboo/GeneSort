namespace GeneSort.Sorter.Tests

open Xunit
open GeneSort.Core
open FSharp.UMX
open System
open GeneSort.Sorter

type SortableBoolArrayTests() =

    [<Fact>]
    let ``Create with valid inputs succeeds`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        Assert.Equal<bool>([| true; false; true |], arr.Values)
        Assert.Equal(3<sortingWidth>, arr.Order)

    [<Fact>]
    let ``Create with mismatched length throws`` () =
        let ex = Assert.Throws<ArgumentException>(fun () -> sortableBoolArray.Create([| true; false |], 3<sortingWidth>) |> ignore)
        Assert.Equal("Values length (2) must equal order (3). (Parameter 'values')", ex.Message)

    [<Fact>]
    let ``DistanceSquared with same array returns zero`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let other = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let result = arr.DistanceSquared(other)
        Assert.Equal(0, result) // (1-1)^2 + (0-0)^2 + (1-1)^2 = 0

    [<Fact>]
    let ``DistanceSquared with different array computes correctly`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let other = sortableBoolArray.Create([| false; false; true |], 3<sortingWidth>)
        let result = arr.DistanceSquared(other)
        Assert.Equal(1, result) // (1-0)^2 + (0-0)^2 + (1-1)^2 = 1 + 0 + 0 = 1

    [<Fact>]
    let ``DistanceSquared with wrong order throws`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let other = sortableBoolArray.Create([| true; false |], 2<sortingWidth>)
        let ex = Assert.Throws<ArgumentException>(fun () -> arr.DistanceSquared(other) |> ignore)
        Assert.Equal("Other array order (2) must equal this order (3). (Parameter 'other')", ex.Message)

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
    let ``SortBy sorts unsorted array correctly`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth> )
        let ces = [| Ce.create 0 1; Ce.create 1 2 |]
        let sorted = arr.SortBy(ces)
        Assert.Equal<bool>([| false; true; true |], sorted.Values)
        Assert.True(sorted.IsSorted)
        Assert.Equal(arr.Order, sorted.Order)

    [<Fact>]
    let ``SortBy with empty ces returns copy`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let sorted = arr.SortBy([||])
        Assert.Equal<bool>(arr.Values, sorted.Values)
        Assert.Equal(arr.Order, sorted.Order)

    [<Fact>]
    let ``SortByWithHistory captures sorting steps correctly`` () =
        let arr = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let ces = [| Ce.create 0 1; Ce.create 1 2 |]
        let history = arr.SortByWithHistory(ces)
        Assert.Equal(3, history.Length) // ces.Length + 1
        Assert.Equal<bool>([| true; false; true |], history.[0].Values) // Original
        Assert.Equal<bool>([| false; true; true |], history.[1].Values) // After Ce(0,1)
        Assert.Equal<bool>([| false; true; true |], history.[2].Values) // After Ce(1,2)
        Assert.True(history.[2].IsSorted)
        Assert.True(history |> Array.forall (fun x -> x.Order = arr.Order))

    [<Fact>]
    let ``getAllSortableBoolArrays with sortingWidth 0 returns single empty array`` () =
        let arrays = SortableBoolArray.getAllSortableBoolArrays 0<sortingWidth>
        Assert.Equal(1, arrays.Length)
        Assert.Equal<bool>([||], arrays.[0].Values)
        Assert.Equal(0<sortingWidth>, arrays.[0].Order)


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

