namespace GeneSort.Core.Test
open System
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Core.Combinatorics
open GeneSort.Core.Permutation
open GeneSort.Core.Perm_Si
open GeneSort.Core.CoreGen
open FsUnit.Xunit

type PermSiTests() =

    let parseIntArray (s: string) : int[] =
        s.Split(',')
        |> Array.map (fun n -> int (n.Trim()))

    [<Fact>]
    let ``Perm_Rs counts are preserved by conjugation`` () =
        let order = 16
        let startIndex = 1
        let perm_RsCount = 5
        let orbitCount = order - perm_RsCount
        let permCount = 5
        let randSeed = 123UL |> UMX.tag<randomSeed>
        let rndPerms = randomPermutations order (new randomLcg(randSeed))
                        |> Seq.take permCount |> Seq.toList
        let perm_Rs = adjacentTwoCycles order startIndex perm_RsCount       
        let conjugates = rndPerms |> List.map(fun p -> Perm_Si.conjugate perm_Rs p)
        let perm_RsOrbitCounts = 
            conjugates |> List.map(fun c -> 
                            (c.Permutation |> Permutation.toOrbitSet).Orbits.Length) 
                       |> List.countBy(id)

        Assert.Equal<int>(orbitCount, perm_RsOrbitCounts.Head |> fst)
        Assert.Equal<int>(1, perm_RsOrbitCounts |> List.length)


    // Deterministic index shuffler for testing: reverses the array
    let testShuffler (n: int) : int =
        (2 * n - 1) % n

    [<Fact>]
    let ``Even order creates maximum transpositions correctly`` () =
        let perm_Rs = saturatedWithTwoCycles 4
        // Expected: [(0,1), (2,3)] -> [1; 0; 3; 2]
        let expected = Perm_Si.createUnsafe [|1; 0; 3; 2|]
        perm_Rs.Permutation.Array |> should equal expected.Permutation.Array
        (perm_Rs.Permutation|> Permutation.toOrbitSet).Orbits.Length |> should equal 2 // n/2 = 4/2 = 2 orbits

    [<Fact>]
    let ``Odd order creates maximum transpositions with one fixed point`` () =
        let perm_Rs = saturatedWithTwoCycles 5
        // Expected: [(0,1), (2,3)] -> [1; 0; 3; 2; 4]
        let expected = Perm_Si.createUnsafe [|1; 0; 3; 2; 4|]
        perm_Rs.Permutation.Array |> should equal expected.Permutation.Array
        (perm_Rs.Permutation |> Permutation.toOrbitSet).Orbits.Length |> should equal 3
        getFixedPoints perm_Rs.Permutation |> should equal [|4|] // One fixed point

    [<Fact>]
    let ``Order 2 creates one transposition`` () =
        let perm_Rs = saturatedWithTwoCycles 2
        // Expected: [(0,1)] -> [1; 0]
        let expected = Perm_Si.createUnsafe [|1; 0|]
        perm_Rs.Permutation.Array |> should equal expected.Permutation.Array
        (perm_Rs.Permutation |> Permutation.toOrbitSet).Orbits.Length |> should equal 1 // n/2 = 2/2 = 1 orbit

    [<Fact>]
    let ``Order 1 creates empty Perm_Rs with one fixed point`` () =
        let perm_Rs = saturatedWithTwoCycles 1
        // Expected: [] -> [0]
        let expected = Perm_Si.createUnsafe [|0|]
        perm_Rs.Permutation.Array |> should equal expected.Permutation.Array
        (perm_Rs.Permutation |> Permutation.toOrbitSet).Orbits.Length |> should equal 1 // n/2 = 1/2 = 0 orbits
        getFixedPoints perm_Rs.Permutation |> should equal [|0|]

    [<Fact>]
    let ``Negative order throws exception`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> saturatedWithTwoCycles -1 |> ignore)
        ex.Message |> should equal "Perm_Si order must not be negative"

    [<Fact>]
    let ``Perm_Rs Reflection`` () =
        let orig = Perm_Si.create [|2; 6; 0; 4; 3; 7; 1; 5|]
        let refl = Perm_Si.makeReflection orig
        let reflA = Perm_Si.makeReflection refl
        reflA.Permutation.Array |> should equal orig.Permutation.Array
        orig |> Perm_Si.isReflectionSymmetric |> should equal true
        let origB = Perm_Si.create [|1; 0; 6; 4; 3; 7; 2; 5|]
        origB |> Perm_Si.isReflectionSymmetric |> should equal false

    [<Fact>]
    let ``Perm_Rs Reflection is it's own inverse`` () =
        let origA = Perm_Si.createUnsafe [|1; 0; 3; 2; 4|]
        let reflA = Perm_Si.makeReflection origA
        let reflA2 = Perm_Si.makeReflection reflA
        reflA2.Permutation.Array |> should equal origA.Permutation.Array

        let origB = Perm_Si.createUnsafe [|7; 1; 5; 0; 3; 6; 2; 4|]
        let reflB = Perm_Si.makeReflection origB
        let reflB2 = Perm_Si.makeReflection reflB
        reflB2.Permutation.Array |> should equal origB.Permutation.Array

    // Helper function to parse a string of comma-separated integers (e.g., "11,2,15")
    let parseIntArray (s: string) : int[] =
        s.Split(',')
        |> Array.map (fun n -> int (n.Trim()))

    [<Theory>]
    [<InlineData("3,1,2,0,4", "3,1,2,0,4,5,9,7,8,6")>]
    [<InlineData("2,7,0,6,5,4,3,1", "2,7,0,6,5,4,3,1,14,12,11,10,9,15,8,13")>]
    let ``unfoldReflection`` (startingArray:string, reflectedArray:string) =
        let startingInts = parseIntArray startingArray
        let reflectedInts = parseIntArray reflectedArray
        let startingTwoCycle = Perm_Si.create startingInts
        let expectedTwoCycle = Perm_Si.create reflectedInts
        let reflectedTwoCycle = Perm_Si.unfoldReflection startingTwoCycle
        reflectedTwoCycle.Array |> should equal expectedTwoCycle.Array


    [<Fact>]
    let ``mutatePerm_Sis with None mode returns original permutation`` () =
        let perm = Perm_Si.create [|1; 0; 2; 3|] // (0 1)
        let mockIndexPicker = indexPicker [|0; 1|]
        let result = mutate mockIndexPicker MutationMode.NoAction perm
        Assert.True(perm.equals result)
        Assert.Equal<int array>(perm.Array, result.Array)

    [<Fact>]
    let ``mutatePerm_Sis with same orbit indices returns original permutation`` () =
        let perm = Perm_Si.create [|1; 0; 2; 3|] // (0 1)
        let mockIndexPicker = indexPicker [|0; 1|] // Picks indices in the same orbit
        let resultOrtho = mutate mockIndexPicker MutationMode.Ortho perm
        let resultPara = mutate mockIndexPicker MutationMode.Para perm
        Assert.True(perm.equals resultOrtho)
        Assert.True(perm.equals resultPara)
        Assert.Equal<int array>(perm.Array, resultOrtho.Array)
        Assert.Equal<int array>(perm.Array, resultPara.Array)

    [<Fact>]
    let ``mutatePerm_Sis with Ortho mode produces valid self-inverse permutation`` () =
        let perm = Perm_Si.create [|1; 0; 3; 2|] // (0 1)(2 3)
        let mockIndexPicker = indexPicker [|0; 2|] // Picks indices 0 and 2
        let result = mutate mockIndexPicker MutationMode.Ortho perm
        let expectedArray = [|2; 3; 0; 1|] // Expected: (0 2)(1 3)
        Assert.Equal<int array>(expectedArray, result.Array)
        Assert.True(Permutation.isSelfInverse result.Permutation)
        Assert.Equal(UMX.tag<Order> 4, result.Order)

    [<Fact>]
    let ``mutatePerm_Sis with Para mode produces valid self-inverse permutation`` () =
        let perm = Perm_Si.create [|1; 0; 3; 2|] // (0 1)(2 3)
        let mockIndexPicker = indexPicker [|0; 2|] // Picks indices 0 and 2
        let result = mutate mockIndexPicker MutationMode.Para perm
        let expectedArray = [|3; 2; 1; 0|] // Expected: (0 3)(1 2)
        Assert.Equal<int array>(expectedArray, result.Array)
        Assert.True(Permutation.isSelfInverse result.Permutation)
        Assert.Equal(UMX.tag<Order> 4, result.Order)

    [<Fact>]
    let ``mutatePerm_Sis with Ortho mode swaps correctly for non-adjacent indices`` () =
        let perm = Perm_Si.create [|1; 0; 3; 2; 5; 4|] // (0 1)(2 3)(4 5)
        let mockIndexPicker = indexPicker [|1; 4|] // Picks indices 1 and 4
        let result = mutate mockIndexPicker MutationMode.Ortho perm
        let expectedArray = [|4; 5; 3; 2; 0; 1|] // Expected: (0 1)(2 3)(4 5)
        Assert.Equal<int array>(expectedArray, result.Array)
        Assert.True(Permutation.isSelfInverse result.Permutation)

    [<Fact>]
    let ``mutatePerm_Sis with Para mode swaps correctly for non-adjacent indices`` () =
        let perm = Perm_Si.create [|1; 0; 3; 2; 5; 4|] // (0 1)(2 3)(4 5)
        let mockIndexPicker = indexPicker [|1; 4|] // Picks indices 1 and 4
        let result = mutate mockIndexPicker MutationMode.Para perm
        let expectedArray = [|5; 4; 3; 2; 1; 0|] // Expected: (0 5)(2 3)(1 4)
        Assert.Equal<int array>(expectedArray, result.Array)
        Assert.True(Permutation.isSelfInverse result.Permutation)

    [<Fact>]
    let ``mutatePerm_Sis preserves order of permutation`` () =
        let perm = Perm_Si.create [|1; 0; 2; 3; 4; 5|] // (0 1)
        let mockIndexPicker = indexPicker [|2; 4|]
        let resultOrtho = mutate mockIndexPicker MutationMode.Ortho perm
        let resultPara = mutate mockIndexPicker MutationMode.Para perm
        Assert.Equal(UMX.tag<Order> 6, resultOrtho.Order)
        Assert.Equal(UMX.tag<Order> 6, resultPara.Order)

    [<Fact>]
    let ``mutatePerm_Sis throws on invalid input permutation`` () =
        let invalidPerm = Permutation.createUnsafe [|1; 1; 2; 3|] // Not a valid permutation
        Assert.Throws<Exception>(fun () -> Perm_Si.create invalidPerm.Array |> ignore)