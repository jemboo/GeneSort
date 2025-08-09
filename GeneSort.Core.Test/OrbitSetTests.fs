namespace GeneSort.Core.Test

open Xunit
open FsUnit.Xunit
open System
open GeneSort.Core
open GeneSort.Core.OrbitSet

type OrbitSetTests() =

    [<Fact>]
    let ``Create valid orbit set with disjoint orbits covering all indices`` () =
        let orbit1 = Orbit.create [0; 1]
        let orbit2 = Orbit.create [2; 3]
        let (orbitSet) = OrbitSet.create [orbit1; orbit2] 4
        orbitSet.Orbits |> List.map (fun o -> o.Indices) |> should equal [[0; 1]; [2; 3]]
        orbitSet.Order |> should equal 4

    [<Fact>]
    let ``Create empty orbit set with order 0`` () =
        let ex = Assert.Throws<Exception>(fun () -> OrbitSet.create [] 0 |> ignore)
        ex.Message |> should equal "orbits cannot be empty"


    [<Fact>]
    let ``Fail to create orbit set with negative order`` () =
        let orbit = Orbit.create [0; 1]
        (fun () -> OrbitSet.create [orbit] -1 |> ignore) |> should throw typeof<System.Exception>

    [<Fact>]
    let ``Fail to create orbit set with non-disjoint orbits`` () =
        let orbit1 = Orbit.create [0; 1]
        let orbit2 = Orbit.create [1; 2]
        (fun () -> OrbitSet.create [orbit1; orbit2] 3 |> ignore) |> should throw typeof<System.Exception>

    [<Fact>]
    let ``Fail to create orbit set with incomplete coverage`` () =
        let orbit = Orbit.create [0; 1]
        (fun () -> OrbitSet.create [orbit] 3 |> ignore) |> should throw typeof<System.Exception>

    [<Fact>]
    let ``Fail to create orbit set with duplicate orbits`` () =
        let orbit = Orbit.create [0; 1]
        (fun () -> OrbitSet.create [orbit; orbit] 2 |> ignore) |> should throw typeof<System.Exception>

    [<Fact>]
    let ``Orbit sets with same orbits and order are equal`` () =
        let orbit1 = Orbit.create [0; 1]
        let orbit2 = Orbit.create [2; 3]
        let set1 = OrbitSet.create [orbit1; orbit2] 4
        let set2 = OrbitSet.create [orbit2; orbit1] 4 // Different input order, same normalized
        set1.Equals(set2) |> should be True
        set1.GetHashCode() |> should equal (set2.GetHashCode())

    [<Fact>]
    let ``Orbit sets with different orbits or order are not equal`` () =
        let orbit1 = Orbit.create [0; 1]
        let orbit2 = Orbit.create [2; 3]
        let set1 = OrbitSet.create [orbit1; orbit2] 4
        let set2 = OrbitSet.create [orbit1] 2
        set1.Equals(set2) |> should be False

    [<Fact>]
    let ``To orbit notation for non-empty orbit set`` () =
        let orbit1 = Orbit.create [0; 1]
        let orbit2 = Orbit.create [2; 3]
        let orbitSet = OrbitSet.create [orbit1; orbit2] 4
        toOrbitNotation orbitSet |> should equal "(0 1)(2 3)"

    [<Fact>]
    let ``Get reflection symmetric orbits`` () =
        let orbit1 = Orbit.create [0; 3] // Reflects to [3; 0] (same orbit)
        let orbit2 = Orbit.create [1; 2] // Reflects to [2; 1] (same orbit)
        let orbitSet = OrbitSet.create [orbit1; orbit2] 4
        let symmetric = getReflectionPartnerOrbits orbitSet
        symmetric |> List.map (fun o -> o.Indices) |> should equal [[0; 3]; [1; 2]]

    [<Fact>]
    let ``Get reflection symmetric orbits with no symmetric pairs`` () =
        let orbit1 = Orbit.create [0; 1; 2] // Reflects to [2; 1; 0] (different orbit)
        let orbitSet = OrbitSet.create [orbit1] 3
        let symmetric = getReflectionPartnerOrbits orbitSet
        symmetric.Length |> should equal 0
