namespace GeneSort.Sorter.Tests

open Xunit
open GeneSort.Core
open FSharp.UMX
open System
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable

type SortableIntArrayTests() =

    [<Fact>]
    let ``Create with valid inputs succeeds`` () =
        let arr = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        Assert.Equal<int>([| 0; 2; 1 |], arr.Values)
        Assert.Equal(3<sortingWidth>, arr.SortingWidth)
        Assert.Equal(3, %arr.SymbolSetSize)

    [<Fact>]
    let ``DistanceSquared with same array returns zero`` () =
        let arr = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let other = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let result = arr.DistanceSquared(other)
        Assert.Equal(0, result)

    [<Fact>]
    let ``DistanceSquared with different array computes correctly`` () =
        let arr = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let other = sortableIntArray.create([| 1; 0; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>)) 
        let result = arr.DistanceSquared(other) 
        Assert.Equal(5, result)

    [<Fact>]
    let ``DistanceSquared with wrong sorting width throws`` () =
        let arr = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let other = sortableIntArray.create([| 0; 2 |], 2<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let ex = Assert.Throws<ArgumentException>(fun () -> arr.DistanceSquared(other) |> ignore)
        Assert.Equal("Other array sorting width (2) must equal this sorting width (3). (Parameter 'other')", ex.Message)

    [<Fact>]
    let ``IsSorted returns true for sorted array`` () =
        let arr = sortableIntArray.create([| 0; 1; 2 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns false for unsorted array`` () =
        let arr = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        Assert.False(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns true for single element`` () =
        let arr = sortableIntArray.create([| 0 |], 1<sortingWidth>, (1 |> UMX.tag<symbolSetSize>))
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns true for empty array`` () =
        let arr = sortableIntArray.create([||], 0<sortingWidth>, (1 |> UMX.tag<symbolSetSize>))
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``SortByCes sorts unsorted array correctly`` () =
        let arr = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let ces = [| ce.create 1 2; ce.create 0 1 |]
        let useCounter = [| 0; 0 |]
        let sorted = arr.SortByCes ces useCounter
        Assert.Equal<int>([| 0; 1; 2 |], sorted.Values)
        Assert.True(sorted.IsSorted)
        Assert.Equal(arr.SortingWidth, sorted.SortingWidth)
        Assert.Equal(arr.SymbolSetSize, sorted.SymbolSetSize) 
        Assert.Equal<int>([| 1; 0 |], useCounter)

    [<Fact>]
    let ``SortByCes with empty ces returns copy`` () =
        let arr = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let useCounter = [||]
        let sorted = arr.SortByCes [||] useCounter
        Assert.Equal<int>(arr.Values, sorted.Values)
        Assert.Equal(arr.SortingWidth, sorted.SortingWidth)
        Assert.Equal(arr.SymbolSetSize, sorted.SymbolSetSize)
        Assert.Equal<int>([||], useCounter)

    [<Fact>]
    let ``SortByCesWithHistory captures sorting steps correctly`` () =
        let arr = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let ces = [| ce.create 1 2; ce.create 0 1 ; ce.create 0 1 |]
        let useCounter = [| 0; 0; 0 |]
        let history = arr.SortByCesWithHistory ces useCounter
        Assert.Equal(4, history.Length)
        Assert.Equal<int>([| 0; 2; 1 |], history.[0].Values)
        Assert.Equal<int>([| 0; 1; 2 |], history.[1].Values)
        Assert.Equal<int>([| 0; 1; 2 |], history.[2].Values)
        Assert.Equal<int>([| 0; 1; 2 |], history.[3].Values)
        Assert.True(history.[2].IsSorted)
        Assert.True(history |> Array.forall (fun x -> x.SortingWidth = arr.SortingWidth && x.SymbolSetSize = arr.SymbolSetSize))
        Assert.Equal<int>([| 1; 0; 0 |], useCounter)


    [<Fact>]
    let ``ToSortableBoolArrays with multiple values creates correct arrays`` () =
        let arr = sortableIntArray.create([| 0; 2; 1; 3 |], 4<sortingWidth>, (4 |> UMX.tag<symbolSetSize>))
        let boolArrays = arr.ToSortableBoolArrays()
        Assert.Equal(5, boolArrays.Length) 
        let expected = [|
            sortableBoolArray.Create([| true; true; true; true |], 4<sortingWidth>) // threshold = 0
            sortableBoolArray.Create([| false; true; true; true |], 4<sortingWidth>) // threshold = 1
            sortableBoolArray.Create([| false; true; false; true |], 4<sortingWidth>) // threshold = 2
            sortableBoolArray.Create([| false; false; false; true |], 4<sortingWidth>) // threshold = 3
            sortableBoolArray.Create([| false; false; false; false |], 4<sortingWidth>) // threshold = 4
        |]
        Assert.Equal<sortableBoolArray[]>(expected, boolArrays)

    [<Fact>]
    let ``ToSortableBoolArrays with SortingWidth 0 returns empty array`` () =
        let arr = sortableIntArray.create([||], 0<sortingWidth>, (1 |> UMX.tag<symbolSetSize>))
        let boolArrays = arr.ToSortableBoolArrays()
        Assert.Equal(0, boolArrays.Length)

    [<Fact>]
    let ``ToSortableBoolArrays with SortingWidth 1 returns empty array`` () =
        let arr = sortableIntArray.create([| 0 |], 1<sortingWidth>, (1 |> UMX.tag<symbolSetSize>))
        let boolArrays = arr.ToSortableBoolArrays()
        Assert.Equal(0, boolArrays.Length)


    [<Fact>]
    let ``Equality with same values returns true`` () =
        let arr1 = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let arr2 = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        Assert.True(arr1.Equals(arr2))
        Assert.True(arr1.Equals(arr2 :> obj))
        Assert.Equal(arr1.GetHashCode(), arr2.GetHashCode())

    [<Fact>]
    let ``Equality with different values returns false`` () =
        let arr1 = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let arr2 = sortableIntArray.create([| 1; 0; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        Assert.False(arr1.Equals(arr2))
        Assert.False(arr1.Equals(arr2 :> obj))

    [<Fact>]
    let ``Equality with different sortingWidth returns false`` () =
        let arr1 = sortableIntArray.create([| 0; 2 |], 2<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let arr2 = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        Assert.False(arr1.Equals(arr2))
        Assert.False(arr1.Equals(arr2 :> obj))

    [<Fact>]
    let ``Equality with different type returns false`` () =
        let arr = sortableIntArray.create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let obj = obj()
        Assert.False(arr.Equals (obj))


    [<Fact>]
    let ``compare bool and int merged sorterTests`` () =
        
        let sortableBoolArrays = SortableBoolArray.getMergeTestCases
                                    6<sortingWidth>
                                    3<mergeDimension>
                                    mergeFillType.NoFill


        let sortableIntArrays = SortableIntArray.getMergeTestCases
                                    6<sortingWidth>
                                    3<mergeDimension>
                                    mergeFillType.NoFill

        let boolConv = 
            sortableIntArrays 
            |> Array.map (fun sia -> 
                sia.ToSortableBoolArrays()
            ) |> Array.concat |> SortableBoolArray.removeDuplicates

        let mutable report = ""
        boolConv |> Array.iter (fun sba ->
            report <- report + sprintf "%A\n" sba.Values
        )

        Assert.Contains("M", "ex.Message")