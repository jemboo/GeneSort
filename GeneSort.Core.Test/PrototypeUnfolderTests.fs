namespace GeneSort.Core.Test

open System
open Xunit
open FsUnit.Xunit
open GeneSort.Core
open GeneSort.Core.TwoOrbitPairOps
open GeneSort.Core.PrototypeUnfolder


type PrototypeUnfolderTests() =
    let epsilon = 1e-10 // Tolerance for floating-point comparisons


    // unfoldTwoOrbitPairsIntoTwoOrbitPairs
    [<Fact>]
    let ``unfoldTwoOrbitPairsIntoTwoOrbitPairs with empty inputs returns empty list`` () =
        let types = []
        let pairs = []
        let result = unfoldTwoOrbitPairsIntoTwoOrbitPairs types pairs
        Assert.Empty(result)

    [<Fact>]
    let ``unfoldTwoOrbitPairsIntoTwoOrbitPairs from ortho seed generates correct pairs`` () =
        let pair0 = twoOrbitPairsForOrder4 TwoOrbitType.Ortho
        let types = [TwoOrbitType.Ortho; TwoOrbitType.Para]
        let result = unfoldTwoOrbitPairsIntoTwoOrbitPairs types [pair0]
        let expected = [
            unfoldTwoOrbitIntoTwoOrbitPair pair0.FirstOrbit 4 TwoOrbitType.Ortho
            unfoldTwoOrbitIntoTwoOrbitPair pair0.SecondOrbit.Value 4 TwoOrbitType.Para
        ]
        Assert.Equal(2, result.Length)
        Assert.Equal<int list>(expected.[0].FirstOrbit.Indices, result.[0].FirstOrbit.Indices)
        Assert.Equal<int list>(expected.[0].SecondOrbit.Value.Indices, result.[0].SecondOrbit.Value.Indices)
        Assert.Equal<int>(expected.[0].Order, result.[0].Order)
        Assert.Equal(expected.[0] |> TwoOrbitPairOps.getTwoOrbitType, result.[0] |> TwoOrbitPairOps.getTwoOrbitType)
        Assert.Equal<int list>(expected.[1].FirstOrbit.Indices, result.[1].FirstOrbit.Indices)
        Assert.Equal<int list>(expected.[1].SecondOrbit.Value.Indices, result.[1].SecondOrbit.Value.Indices)
        Assert.Equal(expected.[1].Order, result.[1].Order)
        Assert.Equal(expected.[1] |> TwoOrbitPairOps.getTwoOrbitType, result.[1] |> TwoOrbitPairOps.getTwoOrbitType)


    [<Fact>]
    let ``unfoldTwoOrbitPairsIntoTwoOrbitPairs two levels generates correct pairs`` () =
        let pairs0 = [twoOrbitPairsForOrder4 TwoOrbitType.SelfRefl]
        let types0 = [TwoOrbitType.Ortho; TwoOrbitType.Para]
        let pairs1 = unfoldTwoOrbitPairsIntoTwoOrbitPairs types0 pairs0
        let expected1 = [
            unfoldTwoOrbitIntoTwoOrbitPair pairs0[0].FirstOrbit 4 TwoOrbitType.Ortho
            unfoldTwoOrbitIntoTwoOrbitPair pairs0[0].SecondOrbit.Value 4 TwoOrbitType.Para
        ]
        Assert.Equal(2, pairs1.Length)
        Assert.Equal<int list>(expected1.[0].FirstOrbit.Indices, pairs1.[0].FirstOrbit.Indices)
        Assert.Equal<int list>(expected1.[0].SecondOrbit.Value.Indices, pairs1.[0].SecondOrbit.Value.Indices)
        Assert.Equal<int>(expected1.[0].Order, pairs1.[0].Order)
        Assert.Equal(expected1.[0] |> TwoOrbitPairOps.getTwoOrbitType, pairs1.[0] |> TwoOrbitPairOps.getTwoOrbitType)
        Assert.Equal<int list>(expected1.[1].FirstOrbit.Indices, pairs1.[1].FirstOrbit.Indices)
        Assert.Equal<int list>(expected1.[1].SecondOrbit.Value.Indices, pairs1.[1].SecondOrbit.Value.Indices)
        Assert.Equal(expected1.[1].Order, pairs1.[1].Order)
        Assert.Equal(expected1.[1] |> TwoOrbitPairOps.getTwoOrbitType, pairs1.[1] |> TwoOrbitPairOps.getTwoOrbitType)
        /// next round
        let types1 = [TwoOrbitType.Ortho; TwoOrbitType.SelfRefl; TwoOrbitType.Para; TwoOrbitType.Para]
        let pairs2 = unfoldTwoOrbitPairsIntoTwoOrbitPairs types1 pairs1
        Assert.Equal(4, pairs2.Length)


    [<Fact>]
    let ``unfoldTwoOrbitPairsIntoTwoOrbitPairs with mismatched lengths throws exception`` () =
        let orbit1 = TwoOrbit.create [0; 1]
        let orbit2 = TwoOrbit.create [2; 3]
        let pair = TwoOrbitPair.create 4 orbit1 (orbit2 |> Some)
        let types = [TwoOrbitType.Ortho; TwoOrbitType.Para; TwoOrbitType.SelfRefl] // Length 3
        let pairs = [pair] // Length 1, 2*1 != 3
        Assert.Throws<ArgumentException>(fun () -> unfoldTwoOrbitPairsIntoTwoOrbitPairs types pairs |> ignore)








