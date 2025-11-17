namespace GeneSort.Core.Test

open Xunit
open GeneSort.Core.Combinatorics
open FsUnit.Xunit
open FSharp.UMX
open GeneSort.Core

type CombinatoricsTests() =

    // Tests for cartesianProductLists
    [<Fact>]
    let ``Empty list of lists returns empty sequence`` () =
        let input = []
        let result = cartesianProductLists input |> Seq.toList
        result |> Seq.isEmpty |> should equal true

    [<Fact>]
    let ``Single empty list returns empty sequence`` () =
        let input = [[]]
        let result = cartesianProductLists input |> Seq.toList
        result |> Seq.isEmpty |> should equal true

    [<Fact>]
    let ``Single list returns sequence of single-element lists`` () =
        let input = [[1; 2; 3]]
        let result = cartesianProductLists input |> Seq.toList
        result |> should equal [[1]; [2]; [3]]

    [<Fact>]
    let ``Two lists return correct Cartesian product`` () =
        let input = [[1; 2]; [3; 4]]
        let result = cartesianProductLists input |> Seq.toList
        result |> should equal [[1; 3]; [1; 4]; [2; 3]; [2; 4]]

    [<Fact>]
    let ``Three lists return correct Cartesian product`` () =
        let input = [[1]; [2; 3]; [4; 5]]
        let result = cartesianProductLists input |> Seq.toList
        result |> should equal [[1; 2; 4]; [1; 2; 5]; [1; 3; 4]; [1; 3; 5]]

    [<Fact>]
    let ``Lists with single elements return single combination`` () =
        let input = [[1]; [2]; [3]]
        let result = cartesianProductLists input |> Seq.toList
        result |> should equal [[1; 2; 3]]

    [<Fact>]
    let ``One empty list among others returns empty sequence`` () =
        let input = [[1; 2]; []; [3; 4]]
        let result = cartesianProductLists input |> Seq.toList
        result |> Seq.isEmpty |> should equal true

    [<Fact>]
    let ``Lists with strings return correct Cartesian product`` () =
        let input = [["a"; "b"]; ["c"]]
        let result = cartesianProductLists input |> Seq.toList
        result |> should equal [["a"; "c"]; ["b"; "c"]]

    [<Fact>]
    let ``Large lists produce correct number of combinations`` () =
        let input = [[1; 2]; [3; 4]; [5; 6]]
        let result = cartesianProductLists input |> Seq.length
        result |> should equal 8 // 2 * 2 * 2 = 8 combinations

    [<Fact>]
    let ``Lists of lists produce correct combinations`` () =
        let input = [[[1; 2]; [3; 4];]; [[5; 6]; [7; 8]]]
        let result = cartesianProductLists input
        result |> Seq.length |> should equal 4


    // Tests for cartesianProductOfArrays
    [<Fact>]
    let ``Cartesian product of two non-empty arrays`` () =
        let array1 = [| [|1; 2|]; [|3; 4|] |]
        let array2 = [| [|5; 6|]; [|7; 8|] |]
        let expected = [| [|1; 2; 5; 6|]; [|1; 2; 7; 8|]; [|3; 4; 5; 6|]; [|3; 4; 7; 8|] |]
        let actual = cartesianProductOfArrays array1 array2
        Assert.Equal<'int [] []>(expected, actual)

    [<Fact>]
    let ``Cartesian product with first array empty`` () =
        let array1: int[][] = [||]
        let array2 = [| [|5; 6|]; [|7; 8|] |]
        let expected: int[][] = [||]
        let actual = cartesianProductOfArrays array1 array2
        Assert.Equal<'int [] []>(expected, actual)

    [<Fact>]
    let ``Cartesian product with second array empty`` () =
        let array1 = [| [|1; 2|]; [|3; 4|] |]
        let array2: int[][] = [||]
        let expected: int[][] = [||]
        let actual = cartesianProductOfArrays array1 array2
        Assert.Equal<'int [] []>(expected, actual)

    [<Fact>]
    let ``Cartesian product with both arrays empty`` () =
        let array1: int[][] = [||]
        let array2: int[][] = [||]
        let expected: int[][] = [||]
        let actual = cartesianProductOfArrays array1 array2
        Assert.Equal<'int [] []>(expected, actual)

    [<Fact>]
    let ``Cartesian product with single-element sub-arrays`` () =
        let array1 = [| [| "a" |]; [| "b" |] |]
        let array2 = [| [| "x" |]; [| "y" |] |]
        let expected = [| [| "a"; "x" |]; [| "a"; "y" |]; [| "b"; "x" |]; [| "b"; "y" |] |]
        let actual = cartesianProductOfArrays array1 array2
        Assert.Equal<'string [] []>(expected, actual)

    [<Fact>]
    let ``Cartesian product with empty sub-arrays`` () =
        let array1 = [| [||]; [||] |]  // Empty sub-arrays of int
        let array2 = [| [|1|]; [|2|] |]
        let expected = [| [|1|]; [|2|]; [|1|]; [|2|] |]
        let actual = cartesianProductOfArrays array1 array2
        Assert.Equal<'int [] []>(expected, actual)

    [<Fact>]
    let ``Cartesian product with one array having one element`` () =
        let array1 = [| [|1; 2; 3|] |]
        let array2 = [| [|4|]; [|5|]; [|6|] |]
        let expected = [| [|1; 2; 3; 4|]; [|1; 2; 3; 5|]; [|1; 2; 3; 6|] |]
        let actual = cartesianProductOfArrays array1 array2
        Assert.Equal<'int [] []>(expected, actual)


    // Tests for cartesianProductMaps and findMapIndex
    let input: list<string * list<string>> = [
        ("key1", [("desc1"); ("desc2")])
        ("key2", [("desc3"); ("desc4")])
    ]

    [<Fact>]
    let ``cartesianProductMaps with empty input returns empty sequence`` () =
        let result = cartesianProductMaps [] |> Seq.toList
        result |> should be Empty

    [<Fact>]
    let ``cartesianProductMaps with single key returns single-entry maps`` () =
        let input = [("key1", [("desc1"); ("desc2")])]
        let expected = [
            Map.ofList [("key1", ("desc1"))]
            Map.ofList [("key1", ("desc2"))]
        ]
        let result = cartesianProductMaps input |> Seq.toList
        result |> should equal expected

    [<Fact>]
    let ``cartesianProductMaps with multiple keys returns correct combinations`` () =
        let expected = [
            Map.ofList [("key1", ("desc1")); ("key2", ("desc3"))]
            Map.ofList [("key1", ("desc1")); ("key2", ("desc4"))]
            Map.ofList [("key1", ("desc2")); ("key2", ("desc3"))]
            Map.ofList [("key1", ("desc2")); ("key2", ("desc4"))]
        ]
        let result = cartesianProductMaps input |> Seq.toList
        result |> should haveLength 4
        result |> should equal expected

    [<Fact>]
    let ``cartesianProductMaps with empty value list returns empty sequence`` () =
        let input = [("key1", []); ("key2", [("desc3")])]
        let result = cartesianProductMaps input |> Seq.toList
        result |> should be Empty

    [<Fact>]
    let ``findMapIndex with matching map returns correct index`` () =
        let targetMap = Map.ofList [("key1", ("desc1")); ("key2", ("desc3"))]
        let index = findMapIndex targetMap input
        index |> should equal (Some 0)

    [<Fact>]
    let ``findMapIndex with another matching map returns correct index`` () =
        let targetMap = Map.ofList [("key1", ("desc2")); ("key2", ("desc4"))]
        let index = findMapIndex targetMap input
        index |> should equal (Some 3)

    [<Fact>]
    let ``findMapIndex with invalid key returns None`` () =
        let targetMap = Map.ofList [("key1", ("desc1")); ("key3", ("desc5"))]
        let index = findMapIndex targetMap input
        index |> should equal None

    [<Fact>]
    let ``findMapIndex with missing key returns None`` () =
        let targetMap = Map.ofList [("key1", ("desc1"))]
        let index = findMapIndex targetMap input
        index |> should equal None

    [<Fact>]
    let ``findMapIndex with invalid value returns None`` () =
        let targetMap = Map.ofList [("key1", ("desc1")); ("key2", ("desc5"))]
        let index = findMapIndex targetMap input
        index |> should equal None

    [<Fact>]
    let ``findMapIndex with empty input returns None`` () =
        let targetMap = Map.ofList [("key1", ("desc1"))]
        let index = findMapIndex targetMap []
        index |> should equal None