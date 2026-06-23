namespace GeneSort.Core.Test.TwoOrbit
open System
open Xunit
open GeneSort.Core
open FSharp.UMX
open GeneSort.Core.TwoOrbitPairOps
open GeneSort.Core.PrototypeUnfolder

type TwoOrbitPairTests() =

    [<Fact>]
    let ``Create TwoOrbitPair with valid Ortho pair succeeds`` () =
        let orbit1 = twoOrbit.create [0; 1]
        let orbit2 = twoOrbit.create [2; 3]
        let order = 4
        let pair = twoOrbitPair.create order orbit1 (orbit2 |> Some)
        Assert.Equal(orbit1, pair.FirstOrbit)
        Assert.Equal(orbit2, pair.SecondOrbit.Value)
        Assert.Equal(order, pair.Order)
        Assert.Equal(twoOrbitPairType.Ortho, pair |> TwoOrbitPairOps.getTwoOrbitPairType)
        Assert.Equal((orbit1, orbit2), (pair.FirstOrbit, pair.SecondOrbit.Value))

    [<Fact>]
    let ``Create TwoOrbitPair with valid Para pair succeeds`` () =
        let orbit1 = twoOrbit.create [2; 4]
        let orbit2 = twoOrbit.create [3; 5]
        let order = 8
        let pair = twoOrbitPair.create order orbit1 (orbit2 |> Some)
        Assert.Equal(orbit1, pair.FirstOrbit)
        Assert.Equal(orbit2, pair.SecondOrbit.Value)
        Assert.Equal(order, pair.Order)
        Assert.Equal(twoOrbitPairType.Para, pair |> TwoOrbitPairOps.getTwoOrbitPairType)
        Assert.Equal((orbit1, orbit2), (pair.FirstOrbit, pair.SecondOrbit.Value))

    [<Fact>]
    let ``Create TwoOrbitPair with ReflectionSymmetric pair succeeds`` () =
        let orbit1 = twoOrbit.create [1; 2]
        let orbit2 = twoOrbit.create [0; 3]
        let order = 4
        let pair = twoOrbitPair.create order orbit1 (orbit2 |> Some)
        Assert.Equal(orbit2, pair.FirstOrbit)
        Assert.Equal(orbit1, pair.SecondOrbit.Value)
        Assert.Equal(order, pair.Order)
        Assert.Equal(twoOrbitPairType.SelfRefl, pair |> TwoOrbitPairOps.getTwoOrbitPairType)
        Assert.Equal((orbit2, orbit1), (pair.FirstOrbit, pair.SecondOrbit.Value))

    [<Fact>]
    let ``Create TwoOrbitPair with non-disjoint indices fails`` () =
        let orbit1 = twoOrbit.create [0; 1]
        let orbit2 = twoOrbit.create [1; 2]
        let order = 4
        Assert.Throws<Exception>(fun () -> twoOrbitPair.create order orbit1 (orbit2 |> Some) |> ignore)

    [<Fact>]
    let ``Create TwoOrbitPair with order less than 4 fails`` () =
        let orbit1 = twoOrbit.create [0; 1]
        let orbit2 = twoOrbit.create [2; 3]
        let order = 3
        Assert.Throws<Exception>(fun () -> twoOrbitPair.create order orbit1 (orbit2 |> Some) |> ignore)

    [<Fact>]
    let ``Create TwoOrbitPair with invalid orbit pair fails`` () =
        let orbit1 = twoOrbit.create [0; 1]
        let orbit2 = twoOrbit.create [2; 4]
        let order = 4
        Assert.Throws<Exception>(fun () -> twoOrbitPair.create order orbit1 (orbit2 |> Some) |> ignore)

    [<Fact>]
    let ``TwoOrbitPair equality with same orbits and order`` () =
        let orbit1 = twoOrbit.create [0; 1]
        let orbit2 = twoOrbit.create [2; 3]
        let order = 4
        let pair1 = twoOrbitPair.create order orbit1 (orbit2 |> Some)
        let pair2 = twoOrbitPair.create order orbit2 (orbit1 |> Some)
        Assert.True(pair1.Equals(pair2))
        Assert.Equal(pair1.GetHashCode(), pair2.GetHashCode())

    [<Fact>]
    let ``TwoOrbitPair inequality with different orbits`` () =
        let orbit1 = twoOrbit.create [0; 1]
        let orbit2 = twoOrbit.create [2; 3]
        let orbit3 = twoOrbit.create [0; 3]
        let orbit4 = twoOrbit.create [1; 2]
        let order = 4
        let pair1 = twoOrbitPair.create order orbit1 (orbit2 |> Some)
        let pair2 = twoOrbitPair.create order orbit3 (orbit4 |> Some)
        Assert.False(pair1.Equals(pair2))

    [<Fact>]
    let ``TwoOrbitPair inequality with different orders and orbits`` () =
        let orbit1 = twoOrbit.create [0; 1]
        let orbit2 = twoOrbit.create [2; 3]
        let orbit2s = twoOrbit.create [4; 5]
        let pair1 = twoOrbitPair.create 4 orbit1 (orbit2 |> Some)
        let pair2 = twoOrbitPair.create 6 orbit1 (orbit2s |> Some)
        Assert.False(pair1.Equals(pair2))

    //unfoldTwoOrbitIntoTwoOrbitPair
    [<Fact>]
    let ``unfoldTwoOrbitIntoTwoOrbitPair with Ortho type creates correct TwoOrbitPair`` () =
        let toOb = twoOrbit.create [0; 1]
        let order = 4
        let pair = unfoldTwoOrbitIntoTwoOrbitPair toOb order twoOrbitPairType.Ortho
        let expectedFirst = twoOrbit.create [0; 1]
        let expectedSecond = twoOrbit.create [6; 7]
        Assert.Equal(expectedFirst, pair.FirstOrbit)
        Assert.Equal(expectedSecond, pair.SecondOrbit.Value)
        Assert.Equal(order * 2, pair.Order) // order * 2 = 8
        Assert.Equal(twoOrbitPairType.Ortho, pair |> TwoOrbitPairOps.getTwoOrbitPairType)
        Assert.Equal((expectedFirst, expectedSecond), (pair.FirstOrbit, pair.SecondOrbit.Value))

    [<Fact>]
    let ``unfoldTwoOrbitIntoTwoOrbitPair with Para type creates correct TwoOrbitPair`` () =
        let toOb = twoOrbit.create [0; 1]
        let order = 4
        let pair = unfoldTwoOrbitIntoTwoOrbitPair toOb order twoOrbitPairType.Para
        let expectedFirst = twoOrbit.create [0; 6]
        let expectedSecond = twoOrbit.create [1; 7]
        Assert.Equal(expectedFirst, pair.FirstOrbit)
        Assert.Equal(expectedSecond, pair.SecondOrbit.Value)
        Assert.Equal(order * 2, pair.Order) // order * 2 = 8
        Assert.Equal(twoOrbitPairType.Para, pair |> TwoOrbitPairOps.getTwoOrbitPairType)
        Assert.Equal((expectedFirst, expectedSecond), (pair.FirstOrbit, pair.SecondOrbit.Value))

    [<Fact>]
    let ``unfoldTwoOrbitIntoTwoOrbitPair with ReflectionSymmetric type creates correct TwoOrbitPair`` () =
        let toOb = twoOrbit.create [1; 2]
        let order = 4
        let pair = unfoldTwoOrbitIntoTwoOrbitPair toOb order twoOrbitPairType.SelfRefl
        let expectedFirst = twoOrbit.create [1; 6]
        let expectedSecond = twoOrbit.create [2; 5]
        Assert.Equal(expectedFirst, pair.FirstOrbit)
        Assert.Equal(expectedSecond, pair.SecondOrbit.Value)
        Assert.Equal(order * 2, pair.Order) // order * 2 = 8
        Assert.Equal(twoOrbitPairType.SelfRefl, pair |> TwoOrbitPairOps.getTwoOrbitPairType)
        Assert.Equal((expectedFirst, expectedSecond), (pair.FirstOrbit, pair.SecondOrbit.Value))

    [<Fact>]
    let ``unfoldTwoOrbitIntoTwoOrbitPair with order less than 2 fails`` () =
        let twoOrbit = twoOrbit.create [0; 1]
        let order = 1
        Assert.Throws<Exception>(fun () -> unfoldTwoOrbitIntoTwoOrbitPair twoOrbit order twoOrbitPairType.Ortho |> ignore)


    [<Fact>]
    let ``makeTwoCycleFromRsOrbitPairTypes with no seed`` () =
        let types = [
                twoOrbitPairType.SelfRefl; twoOrbitPairType.Ortho; 
                twoOrbitPairType.Para; twoOrbitPairType.SelfRefl; 
                twoOrbitPairType.Para; twoOrbitPairType.Ortho; twoOrbitPairType.Para]
        let perm_Rs = makePerm_SiFromTwoOrbitPairsAndTypes None types
        Assert.Equal((perm_Rs.Order |> UMX.untag), 16)


    [<Fact>]
    let ``makeTwoCycleFromRsOrbitPairTypes with a seed`` () =
        let types = [twoOrbitPairType.Para; twoOrbitPairType.SelfRefl; 
                     twoOrbitPairType.Para; twoOrbitPairType.Ortho; 
                     twoOrbitPairType.Para; twoOrbitPairType.SelfRefl;]
        let orbit1 = twoOrbit.create [0; 1]
        let orbit2 = twoOrbit.create [2; 3]
        let pair = twoOrbitPair.create 4 orbit1 (orbit2 |> Some)
        let perm_Rs = makePerm_SiFromTwoOrbitPairsAndTypes (Some [pair]) types
        Assert.Equal((perm_Rs.Order |> UMX.untag), 16)


    [<Fact>]
    let ``makeAllRsTwoCyclesOfOrder`` () =
        let types = makeAllPerm_SisOfOrder 16 |> Seq.toList
        let setOf = types |> Set.ofList
        Assert.Equal(16, 16)


    [<Fact>]
    let ``makeRandomRsTwoCycles classic`` () =
        let randSeed = 123UL |> UMX.tag<randomSeed>
        let order = 32
        let randy = new randomLcg(randSeed)
        let res = makeRandomPerm_Sis randy order |> Seq.take 200000
        let seti = 
            res 
            |> Seq.groupBy(id)
            |> Seq.map (fun (len, arr) -> len, arr |> Seq.length)
            |> Seq.toArray
            |> Array.sortByDescending(snd)
        Assert.Equal(16, 16)
