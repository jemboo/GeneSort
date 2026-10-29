namespace GeneSort.Sorter.Tests

open System
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter


type SortableBoolArrayTests() =

    [<Fact>]
    let ``Create with valid inputs succeeds`` () =
        let arr = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        Assert.Equal<bool>([| true; false; true |], arr.Values)
        Assert.Equal(3<channelCount>, arr.ChannelCount)

    [<Fact>]
    let ``Create with mismatched length throws`` () =
        let ex = Assert.Throws<ArgumentException>(fun () -> sortableBoolArray.create([| true; false |], 3<channelCount>) |> ignore)
        Assert.Equal("Values length (2) must equal order (3). (Parameter 'values')", ex.Message)

    [<Fact>]
    let ``DistanceSquared with same array returns zero`` () =
        let arr = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        let other = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        let result = arr.DistanceSquared(other)
        Assert.Equal(0, result)

    [<Fact>]
    let ``DistanceSquared with different array computes correctly`` () =
        let arr = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        let other = sortableBoolArray.create([| false; false; true |], 3<channelCount>)
        let result = arr.DistanceSquared(other)
        Assert.Equal(1, result)

    [<Fact>]
    let ``IsSorted returns true for sorted array`` () =
        let arr = sortableBoolArray.create([| false; false; true |], 3<channelCount>)
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns false for unsorted array`` () =
        let arr = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        Assert.False(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns true for single element`` () =
        let arr = sortableBoolArray.create([| true |], 1<channelCount>)
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``IsSorted returns true for empty array`` () =
        let arr = sortableBoolArray.create([||], 0<channelCount>)
        Assert.True(arr.IsSorted)

    [<Fact>]
    let ``SortByCes sorts unsorted array correctly`` () =
        let arr = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        let ces = [| ce.create 0 1; ce.create 1 2 |]
        let useCounter = [| 0; 0 |]
        let sorted = CeSorting.sortByWithUses ces useCounter arr.Values |> fun values -> sortableBoolArray.create(values, arr.ChannelCount)
        Assert.Equal<bool>([| false; true; true |], sorted.Values)
        Assert.True(sorted.IsSorted)
        Assert.Equal(arr.ChannelCount, sorted.ChannelCount)
        Assert.Equal<int>([| 1; 0 |], useCounter)

    [<Fact>]
    let ``SortByCes with empty ces returns copy`` () =
        let arr = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        let useCounter = [||]
        let sorted = CeSorting.sortByWithUses [||] useCounter arr.Values |> fun values -> sortableBoolArray.create(values, arr.ChannelCount)
        Assert.Equal<bool>(arr.Values, sorted.Values)
        Assert.Equal(arr.ChannelCount, sorted.ChannelCount)
        Assert.Equal<int>([||], useCounter)

    [<Fact>]
    let ``SortByCes with partial extent sorts correctly`` () =
        let arr = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        let ces = [| ce.create 0 1; ce.create 1 2 |]
        let useCounter = [| 0; 0 |]
        let sorted = CeSorting.sortByWithUses ces useCounter arr.Values |> fun values -> sortableBoolArray.create(values, arr.ChannelCount)
        Assert.Equal<bool>([| false; true; true |], sorted.Values)
        Assert.Equal<int>([| 1; 0 |], useCounter)

    [<Fact>]
    let ``SortByCesWithHistory captures sorting steps correctly`` () =
        let arr = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        let ces = [| ce.create 0 1; ce.create 1 2; ce.create 1 2 |]
        let useCounter = [| 0; 0; 0 |]
        let history = CeSorting.sortByWithHistoryAndUses ces useCounter arr.Values |> Array.map (fun values -> sortableBoolArray.create(values, arr.ChannelCount))
        Assert.Equal(4, history.Length)
        Assert.Equal<bool>([| true; false; true |], history.[0].Values)
        Assert.Equal<bool>([| false; true; true |], history.[1].Values)
        Assert.Equal<bool>([| false; true; true |], history.[2].Values)
        Assert.True(history.[2].IsSorted)
        Assert.True(history |> Array.forall (fun x -> x.ChannelCount = arr.ChannelCount))
        Assert.Equal<int>([| 1; 0; 0 |], useCounter)

    [<Fact>]
    let ``getAllSortableBoolArrays with channelCount 0 returns single empty array`` () =
        let arrays = SortableBoolArray.getAllSortableBoolArrays 0<channelCount> |> Seq.toArray
        Assert.Equal(1, arrays.Length)
        Assert.Equal<bool>([||], arrays.[0].Values)
        Assert.Equal(0<channelCount>, arrays.[0].ChannelCount)

    [<Fact>]
    let ``getAllSortableBoolArrays with channelCount 1 returns two arrays`` () =
        let arrays = SortableBoolArray.getAllSortableBoolArrays 1<channelCount> |> Seq.toArray
        Assert.Equal(2, arrays.Length)
        let expected = [|
            sortableBoolArray.create([| false |], 1<channelCount>)
            sortableBoolArray.create([| true |], 1<channelCount>)
        |]
        Assert.Equal<sortableBoolArray[]>(expected, arrays)

    [<Fact>]
    let ``getAllSortableBoolArrays with channelCount 2 returns four arrays`` () =
        let arrays = SortableBoolArray.getAllSortableBoolArrays 2<channelCount> |> Seq.toArray
        Assert.Equal(4, arrays.Length)
        let expected = [|
            sortableBoolArray.create([| false; false |], 2<channelCount>)
            sortableBoolArray.create([| true; false |], 2<channelCount>)
            sortableBoolArray.create([| false; true |], 2<channelCount>)
            sortableBoolArray.create([| true; true |], 2<channelCount>)
        |]
        Assert.Equal<sortableBoolArray[]>(expected, arrays)

    [<Fact>]
    let ``getAllSortableBoolArrays with channelCount 3 returns eight arrays`` () =
        let arrays = SortableBoolArray.getAllSortableBoolArrays 3<channelCount> |> Seq.toArray
        Assert.Equal(8, arrays.Length)

    [<Fact>]
    let ``Equality with same values returns true`` () =
        let arr1 = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        let arr2 = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        Assert.True(arr1.Equals(arr2))
        Assert.True(arr1.Equals(arr2 :> obj))
        Assert.Equal(arr1.GetHashCode(), arr2.GetHashCode())

    [<Fact>]
    let ``Equality with different values returns false`` () =
        let arr1 = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        let arr2 = sortableBoolArray.create([| false; false; true |], 3<channelCount>)
        Assert.False(arr1.Equals(arr2))
        Assert.False(arr1.Equals(arr2 :> obj))

    [<Fact>]
    let ``Equality with different order returns false`` () =
        let arr1 = sortableBoolArray.create([| true; false |], 2<channelCount>)
        let arr2 = sortableBoolArray.create([| true; false; true |], 3<channelCount>)
        Assert.False(arr1.Equals(arr2))
        Assert.False(arr1.Equals(arr2 :> obj))

    [<Fact>]
    let ``Equality with different type returns false`` () =
        let arr = sortableBoolArray.create([| true; false |], 2<channelCount>)
        let obj = obj()
        Assert.False(arr.Equals(obj))

    [<Fact>]
    let ``fromLatticePoint is correct`` () =
        let targetArray = sortableBoolArray.create([| false; false; true; true; true; true; false; false; false |], 9<channelCount>)
        let lp = latticePoint.create([| 1; 3; 0 |])
        let maxValue = 3<latticeDistance>
        let res = SortableBoolArray.fromLatticePoint lp maxValue
        Assert.True(res.Equals targetArray)


    [<Fact>]
    let ``fromLatticeCube is correct`` () =
        let maxValue = 3<latticeDistance>
        let dim = 3<latticeDimension>
        let res = SortableBoolArray.fromLatticeCubeFull dim maxValue |> Seq.toArray
        Assert.True(res.Length = 64)
        let res2 = SortableBoolArray.fromLatticeCubeVV_1 dim maxValue |> Seq.toArray
        Assert.True(res2.Length = 20)