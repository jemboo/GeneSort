namespace GeneSort.Core.Test

open Xunit
open FsUnit.Xunit
open System.Collections.Generic
open GeneSort.Core
open GeneSort.Core.Orbit

// Custom equality comparer for Orbit to use in HashSet
type OrbitEqualityComparer() =
    interface IEqualityComparer<Orbit> with
        member _.Equals(x: Orbit, y: Orbit) = x.Equals(y)
        member _.GetHashCode(orbit: Orbit) = orbit.GetHashCode()

type OrbitTests() =

    [<Fact>]
    let ``Create valid orbit with unique non-negative indices`` () =
        let indices = [2; 0; 1]
        let orbit = Orbit.create indices
        orbit.Indices |> should equal [0; 1; 2] // Normalized with smallest index first

    [<Fact>]
    let ``Create empty orbit`` () =
        let orbit = Orbit.create []
        orbit.Indices.Length |> should equal 0

    [<Fact>]
    let ``Fail to create orbit with negative indices`` () =
        let indices = [1; -2; 3]
        (fun () -> Orbit.create indices |> ignore) |> should throw typeof<System.Exception>

    [<Fact>]
    let ``Fail to create orbit with duplicate indices`` () =
        let indices = [1; 2; 1]
        (fun () -> Orbit.create indices |> ignore) |> should throw typeof<System.Exception>

    [<Fact>]
    let ``Orbits with same normalized indices are equal`` () =
        let orbit1 = Orbit.create [2; 0; 1]
        let orbit2 = Orbit.create [0; 1; 2]
        orbit1.Equals(orbit2) |> should be True
        orbit1.GetHashCode() |> should equal (orbit2.GetHashCode())

    [<Fact>]
    let ``Orbits with different indices are not equal`` () =
        let orbit1 = Orbit.create [0; 1]
        let orbit2 = Orbit.create [0; 2]
        orbit1.Equals(orbit2) |> should be False

    [<Fact>]
    let ``Get reflection of orbit`` () =
        let orbit = Orbit.create [0; 1; 2]
        let reflected = getReflection 3 orbit
        reflected.Indices |> should equal [0; 2; 1] // Reflection about (3-1)/2 = 1

    [<Fact>]
    let ``Get reflection of orbit with order 4`` () =
        let orbit = Orbit.create [0; 2]
        let reflected = getReflection 4 orbit
        reflected.Indices |> should equal [1; 3] // Reflection: 0 -> 3, 2 -> 1


    [<Fact>]
    let ``Orbits in HashSet collection handle equality correctly`` () =
        let orbit1 = Orbit.create [2; 5; 0]
        let orbit2 = Orbit.create [5; 0; 2]
        let orbit3 = Orbit.create [1; 3; 4]
        let orbitSet = HashSet<Orbit>(OrbitEqualityComparer())
        orbitSet.Add orbit1 |> should be True
        orbitSet.Add orbit2 |> should be False // orbit2 is equal to orbit1, so not added
        orbitSet.Add orbit3 |> should be True  // orbit3 is distinct, so added
        orbitSet.Count |> should equal 2
        orbitSet.Contains orbit1 |> should be True
        orbitSet.Contains orbit2 |> should be True
        orbitSet.Contains orbit3 |> should be True

    [<Fact>]
    let ``GetHashCode is consistent across equal orbits`` () =
        let orbit1 = Orbit.create [2; 5; 0]
        let orbit2 = Orbit.create [5; 0; 2]
        let orbit3 = Orbit.create [0; 2; 5]
        orbit1.GetHashCode() |> should equal (orbit2.GetHashCode())
        orbit1.GetHashCode() |> should equal (orbit3.GetHashCode())