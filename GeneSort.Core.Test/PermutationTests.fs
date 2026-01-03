namespace GeneSort.Core.Test
open System
open Xunit
open FsUnit.Xunit
open GeneSort.Core.Permutation
open GeneSort.Core
open GeneSort.Core.OrbitSet

type PermutationTests() =

    [<Fact>]
    let ``Identity permutation is valid`` () =
        let id = identity 4
        Assert.Equal<int>([|0; 1; 2; 3|], id.Array)
        Assert.True(Array.forall2 (fun i v -> i = v) [|0 .. 3|] (Array.sort id.Array))

    [<Fact>]
    let ``Rotated identity by 1 is correct`` () =
        let r = rotated 1 4
        Assert.Equal<int>([|1; 2; 3; 0|], r.Array) // Expected: (0 1 2 3)

    [<Fact>]
    let ``Rotated identity by -1 is correct`` () =
        let r = rotated -1 4
        Assert.Equal<int>([|3; 0; 1; 2|], r.Array) // Expected: (0 3 2 1)

    [<Fact>]
    let ``Rotated identity by 0 is identity`` () =
        let r = rotated 0 4
        Assert.Equal<int>([|0; 1; 2; 3|], r.Array) // Expected: (0)(1)(2)(3)

    [<Fact>]
    let ``Rotated identity by n is identity`` () =
        let r = rotated 4 4
        Assert.Equal<int>([|0; 1; 2; 3|], r.Array) // Expected: (0)(1)(2)(3)

    [<Fact>]
    let ``Rotated with invalid order throws exception`` () =
        Assert.Throws<System.Exception>(fun () -> rotated 1 0 |> ignore)

    [<Fact>]
    let ``Orbit notation of rotated identity is correct`` () =
        let r = rotated 1 4
        Assert.Equal("(0 1 2 3)", toOrbitNotation (r |> Permutation.toOrbitSet) )

    [<Fact>]
    let ``Identity permutation has all indices as fixed points`` () =
        let perm = permutation.createUnsafe [|0; 1; 2; 3|]
        let fixedPoints = getFixedPoints perm
        fixedPoints |> should equal [|0; 1; 2; 3|]

    [<Fact>]
    let ``Permutation with no fixed points returns empty array`` () =
        let perm = permutation.createUnsafe [|1; 0; 3; 2|]
        let fixedPoints = getFixedPoints perm
        fixedPoints |> should equal [||]

    [<Fact>]
    let ``Permutation with some fixed points returns correct indices`` () =
        let perm = permutation.createUnsafe [|0; 2; 2; 3|]
        let fixedPoints = getFixedPoints perm
        fixedPoints |> should equal [|0; 2; 3|]

    [<Fact>]
    let ``Empty permutation returns empty fixed points`` () =
        let perm = permutation.createUnsafe [||]
        let fixedPoints = getFixedPoints perm
        fixedPoints |> should equal [||]

    [<Fact>]
    let ``Single element permutation has that element as fixed point`` () =
        let perm = permutation.createUnsafe [|0|]
        let fixedPoints = getFixedPoints perm
        fixedPoints |> should equal [|0|]


    [<Fact>]
    let ``Composition of two permutations is correct`` () =
        let p = permutation.create [|1; 2; 0; 3|] // (0 1 2)(3)
        let q = permutation.create [|1; 0; 3; 2|] // (0 1)(2 3)
        let result = compose p q
        Assert.Equal<int>([|0; 3; 1; 2|], result.Array) // Expected: (0)(1 3 2)

    [<Fact>]
    let ``Orbit notation of composition is correct`` () =
        let p = permutation.create [|1; 2; 0; 3|] // (0 1 2)(3)
        let q = permutation.create [|1; 0; 3; 2|] // (0 1)(2 3)
        let result = compose p q
        Assert.Equal("(0)(1 3 2)", toOrbitNotation (result |> Permutation.toOrbitSet) )

    [<Fact>]
    let ``Composition with mismatched orders throws exception`` () =
        let p = permutation.create [|1; 2; 0|]
        let q = permutation.create [|1; 0; 3; 2|]
        Assert.Throws<System.Exception>(fun () -> compose p q |> ignore)

    [<Fact>]
    let ``Inverse of permutation is correct`` () =
        let p = permutation.create [|1; 2; 0; 3|] // (0 1 2)(3)
        let inv = inverse p
        Assert.Equal<int>([|2; 0; 1; 3|], inv.Array) // Expected: (0 2 1)(3)
        Assert.Equal<int>((identity 4).Array, (compose p inv).Array)
        Assert.Equal<int>((identity 4).Array, (compose inv p).Array)

    [<Fact>]
    let ``Conjugate of permutation is correct`` () =
        let p = permutation.create [|1; 2; 0; 3|] // (0 1 2)(3)
        let q = permutation.create [|1; 0; 3; 2|] // (0 1)(2 3)
        let result = Permutation.conjugate p q
        Assert.Equal<int>([|3; 0; 2; 1|], result.Array) // Expected: (0 3 1)(2)

    [<Fact>]
    let ``Conjugate with mismatched orders throws exception`` () =
        let p = permutation.create [|1; 2; 0|]
        let q = permutation.create [|1; 0; 3; 2|]
        Assert.Throws<System.Exception>(fun () -> Permutation.conjugate p q |> ignore)
    
    [<Fact>]
    let ``Orbits of permutation are correct`` () =
        let p = permutation.create [|1; 2; 0; 3|] // (0 1 2)(3)
        let expected = [[0; 1; 2]; [3]]
        let actual = (p |> Permutation.toOrbitSet).Orbits |> List.map (fun orbit -> (orbit.Indices))
        actual |> should equal expected

    [<Fact>]
    let ``Orbits of identity permutation are singletons`` () =
        let id = identity 3
        let expected = [[0]; [1]; [2]]
        let actual = (id |> Permutation.toOrbitSet).Orbits |> List.map (fun orbit -> orbit.Indices)
        actual |> should equal expected

    [<Fact>]
    let ``Permutation with fixed points returns the correct orbits`` () =
        // Permutation [1; 0; 2; 4; 3] has cycles (0,1), (2), (3,4)
        let perm = permutation.createUnsafe [|1; 0; 2; 4; 3|]
        let orbits = (perm |> Permutation.toOrbitSet).Orbits |> List.map (fun orbit -> orbit.Indices)
        orbits |> should equal [ [0; 1]; [2]; [3; 4] ]

    [<Fact>]
    let ``Identity permutation returns fixed points`` () =
        // Permutation [0; 1; 2; 3] has only fixed points
        let perm = permutation.createUnsafe [|0; 1; 2; 3|]
        let orbits = (perm |> Permutation.toOrbitSet).Orbits |> List.map (fun orbit -> orbit.Indices)
        orbits |> should equal [ [0]; [1]; [2]; [3] ]

    [<Fact>]
    let ``Permutation with only non-trivial cycle returns that cycle`` () =
        // Permutation [1; 2; 0] has cycle (0,1,2)
        let perm = permutation.createUnsafe [|1; 2; 0|]
        let orbits = (perm|> Permutation.toOrbitSet).Orbits |> List.map (fun orbit -> orbit.Indices)
        orbits |> should equal [ [0; 1; 2] ]

    [<Fact>]
    let ``Permutation with multiple cycles returns all`` () =
        // Permutation [2; 0; 1; 4; 3] has cycles (0,2,1), (3,4)
        let perm = permutation.createUnsafe [|2; 0; 1; 4; 3|]
        let orbits = (perm |> Permutation.toOrbitSet).Orbits |> List.map (fun orbit -> orbit.Indices)
        orbits |> should equal [ [0; 2; 1]; [3; 4] ]

    [<Fact>]
    let ``Orbit notation of identity is correct`` () =
        let id = identity 4
        Assert.Equal("(0)(1)(2)(3)", toOrbitNotation (id |> Permutation.toOrbitSet))

    [<Fact>]
    let ``Orbit notation of permutation is correct`` () =
        let p = permutation.create [|1; 2; 0; 3|] // (0 1 2)(3)
        Assert.Equal("(0 1 2)(3)", toOrbitNotation (p |> Permutation.toOrbitSet))

    [<Fact>]
    let ``toPermutation converts multiple cycles with fixed points correctly`` () =
        let orbit1 = Orbit.create [0; 1]
        let orbit2 = Orbit.create [2]
        let orbit3 = Orbit.create [3; 4]
        let orbitSet = OrbitSet.create [orbit1; orbit2; orbit3] 5
        let perm = fromOrbitSet orbitSet
        perm.Array |> should equal [|1; 0; 2; 4; 3|] // (0 1)(2)(3 4)

    [<Fact>]
    let ``toPermutation converts identity OrbitSet correctly`` () =
        let orbits = [Orbit.create [0]; Orbit.create [1]; Orbit.create [2]]
        let orbitSet = OrbitSet.create orbits 3
        let perm = fromOrbitSet orbitSet
        perm.Array |> should equal [|0; 1; 2|] // (0)(1)(2)

    [<Fact>]
    let ``toPermutation converts full cycle OrbitSet correctly`` () =
        let orbit = Orbit.create [0; 1; 2; 3]
        let orbitSet = OrbitSet.create [orbit] 4
        let perm = fromOrbitSet orbitSet
        perm.Array |> should equal [|1; 2; 3; 0|] // (0 1 2 3)

    [<Fact>]
    let ``OrbitSet.create throws for incomplete coverage`` () =
        let orbit = Orbit.create [0; 1]
        let ex = Assert.Throws<Exception>(fun () -> OrbitSet.create [orbit] 3 |> ignore)
        ex.Message |> should equal "Orbits must cover indices 0 to order-1"

    [<Fact>]
    let ``toPermutation handles empty OrbitSet for order 0`` () =
        let ex = Assert.Throws<Exception>(fun () -> OrbitSet.create [] 3 |> ignore)
        ex.Message |> should equal "orbits cannot be empty"

    [<Fact>]
    let ``toPermutation round-trip with getOrbits preserves permutation`` () =
        let originalPerm = permutation.create [|2; 0; 1; 4; 3|] // (0 2 1)(3 4)
        let newPerm = fromOrbitSet (originalPerm |> Permutation.toOrbitSet)
        newPerm.Array |> should equal originalPerm.Array
