namespace GeneSort.Core.Test

open Xunit
open GeneSort.Core
open GeneSort.Core.PermutationSets

type PermutationSetsTests() =

    [<Fact>]
    let ``Powers of permutation are correct`` () =
        let p = permutation.create [|1; 2; 0; 3|] // (0 1 2)(3)
        let cycles = makeCyclicGroup p |> Seq.toList
        let expected = [
            permutation.create [|1; 2; 0; 3|]; // p
            permutation.create [|2; 0; 1; 3|]; // p^2
            permutation.create [|0; 1; 2; 3|]  // p^3 = identity
        ]
        Assert.Equal<int array list>(List.map (fun (p: permutation) -> p.Array) expected, List.map (fun (p: permutation) -> p.Array) cycles)

    [<Fact>]
    let ``Cycles of identity permutation returns only identity`` () =
        let id = Permutation.identity 4
        let cycles = makeCyclicGroup id |> Seq.toList
        let expected = [Permutation.identity 4]
        Assert.Equal<int array list>(List.map (fun (p: permutation) -> p.Array) expected, List.map (fun (p: permutation) -> p.Array) cycles)