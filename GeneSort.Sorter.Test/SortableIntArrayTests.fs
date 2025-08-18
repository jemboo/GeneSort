namespace GeneSort.Sorter.Tests

open Xunit
open GeneSort.Core
open FSharp.UMX
open System
open GeneSort.Sorter

type SortableIntArrayTests() =

    [<Fact>]
    let ``Create with valid inputs succeeds`` () =
        let arr = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        Assert.Equal<int>([| 0; 2; 1 |], arr.Values)
        Assert.Equal(3<sortingWidth>, arr.SortingWidth)
        Assert.Equal(3UL, %arr.SymbolSetSize)

    [<Fact>]
    let ``DistanceSquared with same array returns zero`` () =
        let arr = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        let other = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        let result = arr.DistanceSquared(other)
        Assert.Equal(0, result)

    [<Fact>]
    let ``DistanceSquared with different array computes correctly`` () =
        let arr = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        let other = sortableIntArray.Create([| 1; 0; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>)) 
        let result = arr.DistanceSquared(other) 
        Assert.Equal(5, result) // (0-1)^2 + (2-0)^2 + (1-1)^2 = 1 + 4 + 0 = 5

    [<Fact>]
    let ``DistanceSquared with wrong sorting width throws`` () =
        let arr = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        let other = sortableIntArray.Create([| 0; 2 |], 2<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        let ex = Assert.Throws<ArgumentException>(fun () -> arr.DistanceSquared(other) |> ignore)
        Assert.Equal("Other array sorting width (2) must equal this sorting width (3). (Parameter 'other')", ex.Message)

    [<Fact>]
    let ``IsSorted returns true for sorted array`` () =
        let arr = sortableIntArray.Create([| 0; 1; 2 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns false for unsorted array`` () =
        let arr = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        Assert.False(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns true for single element`` () =
        let arr = sortableIntArray.Create([| 0 |], 1<sortingWidth>, (1UL |> UMX.tag<symbolSetSize>))
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns true for empty array`` () =
        let arr = sortableIntArray.Create([||], 0<sortingWidth>, (1UL |> UMX.tag<symbolSetSize>))
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``SortBy sorts unsorted array correctly`` () =
        let arr = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        let ces = [| Ce.create 1 2; Ce.create 0 1 |]
        let sorted = arr.SortBy(ces)
        Assert.Equal<int>([| 0; 1; 2 |], sorted.Values)
        Assert.True(sorted.IsSorted)
        Assert.Equal(arr.SortingWidth, sorted.SortingWidth)
        Assert.Equal(arr.SymbolSetSize, sorted.SymbolSetSize)

    [<Fact>]
    let ``SortBy with empty ces returns copy`` () =
        let arr = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        let sorted = arr.SortBy([||])
        Assert.Equal<int>(arr.Values, sorted.Values)
        Assert.Equal(arr.SortingWidth, sorted.SortingWidth)
        Assert.Equal(arr.SymbolSetSize, sorted.SymbolSetSize)

    [<Fact>]
    let ``SortByWithHistory captures sorting steps correctly`` () =
        let arr = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        let ces = [| Ce.create 1 2; Ce.create 0 1 |]
        let history = arr.SortByWithHistory(ces)
        Assert.Equal(3, history.Length) // ces.Length + 1
        Assert.Equal<int>([| 0; 2; 1 |], history.[0].Values) // Original
        Assert.Equal<int>([| 0; 1; 2 |], history.[1].Values) // After Ce(1,2)
        Assert.Equal<int>([| 0; 1; 2 |], history.[2].Values) // After Ce(0,1)
        Assert.True(history.[2].IsSorted)
        Assert.True(history |> Array.forall (fun x -> x.SortingWidth = arr.SortingWidth && x.SymbolSetSize = arr.SymbolSetSize))
