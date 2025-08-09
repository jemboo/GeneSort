namespace GeneSort.Core.Test

open Xunit
open GeneSort.Core.CollectionUtils
open FsUnit.Xunit

type CollectionUtilsTests() =

    [<Fact>]
    let ``steppedOffsetPairs start=0 order=12, offset=3`` () =
        let result = steppedOffsetPairs 0 12 3 |> Seq.toList
        let expected = [(0,3); (1,4); (2,5); (6,9); (7,10); (8,11)]
        result |> should equal expected

    [<Fact>]
    let ``steppedOffsetPairs start=1 order=12, offset=3`` () =
        let result = steppedOffsetPairs 1 12 3 |> Seq.toList
        let expected = [(1,4); (2,5); (3,6); (7,10); (8,11)]
        result |> should equal expected

    [<Fact>]
    let ``steppedOffsetPairs start=0 order=18, offset=3`` () =
        let result = steppedOffsetPairs 0 18 3 |> Seq.toList
        let expected = [(0,3); (1,4); (2,5); (6,9); (7,10); (8,11); (12,15); (13,16); (14,17)]
        result |> should equal expected

    [<Fact>]
    let ``steppedOffsetPairs start=0 order=24, offset=3`` () =
        let result = steppedOffsetPairs 0 24 3 |> Seq.toList
        let expected = [(0, 3); (1, 4); (2, 5); (6, 9); (7, 10); (8, 11); (12, 15); (13, 16); (14, 17); (18, 21); (19, 22); (20, 23)]
        result |> should equal expected

    [<Fact>]
    let ``steppedOffsetPairs start=0 order=10, offset=3`` () =
        let result = steppedOffsetPairs 0 10 3 |> Seq.toList
        let expected = [(0, 3); (1, 4); (2, 5); (6, 9);]
        result |> should equal expected

    [<Fact>]
    let ``steppedOffsetPairs start=0 order=7, offset=2`` () =
        let result = steppedOffsetPairs 0 7 2 |> Seq.toList
        let expected = [(0,2); (1,3); (4,6)]
        result |> should equal expected

    [<Fact>]
    let ``steppedOffsetPairs start=1 order=7, offset=2`` () =
        let result = steppedOffsetPairs 1 7 2 |> Seq.toList
        let expected = [(1,3); (2,4);]
        result |> should equal expected

    [<Fact>]
    let ``steppedOffsetPairs start=1 order=7, offset=1`` () =
        let result = steppedOffsetPairs 1 7 1 |> Seq.toList
        let expected = [(1,2); (3,4); (5,6)]
        result |> should equal expected

    [<Fact>]
    let ``steppedOffsetPairs start=0 order=0, offset=3 should return empty sequence`` () =
        let result = steppedOffsetPairs 0 0 3 |> Seq.toList
        result |> should be Empty

    [<Fact>]
    let ``steppedOffsetPairs start=0 order=5, offset=5 should return empty sequence`` () =
        let result = steppedOffsetPairs 0 5 5 |> Seq.toList
        result |> should be Empty

    [<Fact>]
    let ``steppedOffsetPairs start=0 order=10, offset=0 should return empty sequence`` () =
        let result = steppedOffsetPairs 0 10 0 |> Seq.toList
        result |> should be Empty


    //pairWithNext
    [<Fact>]
    let ``pairWithNext with two integers returns a pair`` () =
        let input = seq [1; 2;]
        let result = pairWithNext input |> Seq.toList
        let expected = [(1, Some 2)]
        Assert.Equal<(int * int option) list>(expected, result)

    [<Fact>]
    let ``pairWithNext with three integers returns correct pairs`` () =
        let input = seq [1; 2; 3]
        let result = pairWithNext input |> Seq.toList
        let expected = [(1, Some 2); (3, None)]
        Assert.Equal<(int * int option) list>(expected, result)

    [<Fact>]
    let ``pairWithNext with single integer returns single pair with None`` () =
        let input = seq [42]
        let result = pairWithNext input |> Seq.toList
        let expected = [(42, None)]
        Assert.Equal<(int * int option) list>(expected, result)

    [<Fact>]
    let ``pairWithNext with empty sequence returns empty sequence`` () =
        let input = Seq.empty<int>
        let result = pairWithNext input |> Seq.toList
        Assert.Empty(result)

    [<Fact>]
    let ``pairWithNext with strings returns correct pairs`` () =
        let input = seq ["a"; "b"; "c"; "d"]
        let result = pairWithNext input |> Seq.toList
        let expected = [("a", Some "b"); ("c", Some "d");]
        Assert.Equal<(string * string option) list>(expected, result)

    [<Fact>]
    let ``pairWithNext with large sequence returns correct pairs`` () =
        let input = seq { 1 .. 11 }
        let result = pairWithNext input |> Seq.toList
        let expected = [ for i in 1 .. 5 -> (2*i - 1, Some (2*i)) ] @ [(11, None)]
        Assert.Equal<(int * int option) list>(expected, result)


    //chunkByPowersOfTwo
    [<Fact>]
    let ``chunkByPowersOfTwo with empty sequence returns empty sequence`` () =
        let input = Seq.empty<int>
        let result = chunkByPowersOfTwo 1 input
        Assert.Empty(result)

    [<Fact>]
    let ``chunkByPowersOfTwo with single element returns one chunk`` () =
        let input = seq [1]
        let result = chunkByPowersOfTwo 1 input |> Seq.toList
        Assert.Equal(1, result.Length)
        Assert.Equal<int list>([1], result.[0] |> Seq.toList)

    [<Fact>]
    let ``chunkByPowersOfTwo with exact power of 2 length (7 = 1+2+4) returns correct chunks`` () =
        let input = seq [1; 2; 3; 4; 5; 6; 7]
        let result = chunkByPowersOfTwo 1 input |> Seq.map Seq.toList |> Seq.toList
        let expected = [[1]; [2; 3]; [4; 5; 6; 7]]
        Assert.Equal(expected.Length, result.Length)
        Assert.Equal<int list>(expected.[0], result.[0])
        Assert.Equal<int list>(expected.[1], result.[1])
        Assert.Equal<int list>(expected.[2], result.[2])

    [<Fact>]
    let ``chunkByPowersOfTwo with non-power-of-2 length (9) returns correct chunks`` () =
        let input = seq [1; 2; 3; 4; 5; 6; 7; 8; 9]
        let result = chunkByPowersOfTwo 1 input |> Seq.map Seq.toList |> Seq.toList
        let expected = [[1]; [2; 3]; [4; 5; 6; 7]; [8; 9]]
        Assert.Equal(expected.Length, result.Length)
        Assert.Equal<int list>(expected.[0], result.[0])
        Assert.Equal<int list>(expected.[1], result.[1])
        Assert.Equal<int list>(expected.[2], result.[2])
        Assert.Equal<int list>(expected.[3], result.[3])


    [<Fact>]
    let ``chunkByPowersOfTwo with large sequence returns correct chunks`` () =
        let input = seq { 1 .. 20 }
        let result = chunkByPowersOfTwo 1 input |> Seq.map Seq.toList |> Seq.toList
        let expected = [[1]; [2; 3]; [4; 5; 6; 7]; [8; 9; 10; 11; 12; 13; 14; 15]; [16; 17; 18; 19; 20]]
        Assert.Equal(expected.Length, result.Length)
        Assert.Equal<int list>(expected.[0], result.[0])
        Assert.Equal<int list>(expected.[1], result.[1])
        Assert.Equal<int list>(expected.[2], result.[2])
        Assert.Equal<int list>(expected.[3], result.[3])
        Assert.Equal<int list>(expected.[4], result.[4])


    [<Fact>]
    let ``chunkByPowersOfTwo with non-zero startingPower large sequence returns correct chunks`` () =
        let input = seq { 1 .. 20 }
        let startingMultiple = 4
        let result = chunkByPowersOfTwo startingMultiple input |> Seq.map Seq.toList |> Seq.toList
        let expected = [[1; 2; 3; 4]; [5; 6; 7; 8; 9; 10; 11; 12]; [13; 14; 15; 16; 17; 18; 19; 20]]
        Assert.Equal(expected.Length, result.Length)
        Assert.Equal<int list>(expected.[0], result.[0])
        Assert.Equal<int list>(expected.[1], result.[1])
        Assert.Equal<int list>(expected.[2], result.[2])


    [<Fact>]
    let ``chunkByPowersOfTwo preserves element order within chunks`` () =
        let input = seq ['a'; 'b'; 'c'; 'd'; 'e']
        let result = chunkByPowersOfTwo 1 input |> Seq.map Seq.toList |> Seq.toList
        let expected = [['a']; ['b'; 'c']; ['d'; 'e']]
        Assert.Equal(expected.Length, result.Length)
        Assert.Equal<char list>(expected.[0], result.[0])
        Assert.Equal<char list>(expected.[1], result.[1])
        Assert.Equal<char list>(expected.[2], result.[2])