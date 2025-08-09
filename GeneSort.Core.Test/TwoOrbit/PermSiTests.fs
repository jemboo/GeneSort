namespace GeneSort.Core.Test.TwoOrbit
open System
open Xunit
open FsUnit.Xunit
open GeneSort.Core


type Perm_SiTests() =
    // Tests for Perm_Si.getTwoOrbits
    [<Fact>]
    let ``Perm_Si.getTwoOrbits with valid order 4 permutation returns correct TwoOrbits`` () =
        let perm_Rs = Perm_Si.create [|1; 0; 3; 2|]
        let result = Perm_Si.getTwoOrbits perm_Rs
        let expected = [|
            TwoOrbit.create [0; 1]
            TwoOrbit.create [2; 3]
        |]
        Assert.Equal<TwoOrbit array>(expected, result)

    [<Fact>]
    let ``Perm_Si.getTwoOrbits with different valid order 4 permutation returns correct TwoOrbits`` () =
        let perm_Rs = Perm_Si.create  [|2; 3; 0; 1|]
        let result = Perm_Si.getTwoOrbits perm_Rs
        let expected = [|
            TwoOrbit.create [0; 2]
            TwoOrbit.create [1; 3]
        |]
        Assert.Equal<TwoOrbit array>(expected, result)

    [<Fact>]
    let ``Perm_Si.getTwoOrbits with odd order throws exception`` () =
        let perm_Rs = Perm_Si.create [|1; 0; 2|]
        let ex = Assert.Throws<Exception>(fun () -> Perm_Si.getTwoOrbits perm_Rs |> ignore)
        Assert.Equal("Perm_Si order must be non-negative and even", ex.Message)


    [<Fact>]
    let ``Perm_Si.makeReflection`` () =
        let perm_Rs = Perm_Si.create [|1; 0; 2; 3; 5; 4|]
        let reflPerm = perm_Rs |> Perm_Si.makeReflection
        Assert.Equal(reflPerm |> Perm_Si.isReflectionSymmetric, true)