namespace GeneSort.Core

open System
open GeneSort.Core.Orbit
open CollectionUtils

[<CustomEquality; NoComparison>]
type OrbitSet = private OrbitSet of orbits: Orbit list * order: int with
    // Static method to create an OrbitSet, validating input
    static member create (orbits: Orbit list) (order: int) : OrbitSet =
        if orbits.Length < 1 then
            failwith "orbits cannot be empty"
        if order < 0 then
            failwith "OrbitSet order must be non-negative"
        // Validate disjointness and coverage
        let allIndices = 
            orbits 
            |> List.collect (fun orbit -> orbit.Indices)
        let allUniqueIndices = 
            allIndices 
            |> Set.ofList
        if Set.count allUniqueIndices <> allIndices.Length then
            failwith "Orbits must be disjoint"
        if Set.count allUniqueIndices <> order || not (List.forall (fun i -> Set.contains i allUniqueIndices) [0 .. order-1]) then
            failwith "Orbits must cover indices 0 to order-1"
        let sortedOrbits = orbits |> List.sortBy (fun orbit -> orbit.Indices.[0])
        OrbitSet (sortedOrbits, order)

        // Property to access the orbits
        member this.Orbits =
            match this with
            | OrbitSet (orbits, _) -> orbits

        // Property to access the order
        member this.Order =
            match this with
            | OrbitSet (_, order) -> order

        // Check if two OrbitSets are equal
        member this.Equals (other: OrbitSet) =
            this.Orbits = other.Orbits && this.Order = other.Order

        // Override for object equality
        override this.Equals (obj: obj) =
            match obj with
            | :? OrbitSet as other -> this.Equals other
            | _ -> false

        // Override GetHashCode for consistency with Equals
        override this.GetHashCode () =
            let orbitHash = 
                this.Orbits
                |> List.map (fun orbit -> orbit.GetHashCode())
                |> List.fold (fun acc h -> (acc * 397) ^^^ h) 0
            (orbitHash * 397) ^^^ this.Order

        // Implement IEquatable<OrbitSet> for structural equality
        interface IEquatable<OrbitSet> with
            member this.Equals (other: OrbitSet) = this.Equals other



module OrbitSet =

    // Helper to print permutation in orbit notation, including fixed points
    let toOrbitNotation (orbitSet: OrbitSet) : string =
        let orbits = orbitSet.Orbits
        let orbitString = 
            orbits 
            |> List.map (fun orbit -> "(" + (orbit.Indices |> List.map string |> String.concat " ") + ")")
            |> String.concat ""
        if orbitString = "" then "()" else orbitString


    /// Finds all orbits (for all lengths including fixed points) that are either 
    /// their own reflection or have a reflection partner orbit. 
    let getReflectionPartnerOrbits (orbitSet: OrbitSet) =
        let reflected = orbitSet.Orbits
                        |> List.map(getReflection orbitSet.Order)
        let combo = orbitSet.Orbits |> List.append reflected
        let dupes = combo 
                    |> List.groupBy(id) 
                    |> List.filter(fun (k, g) -> g.Length = 2)
                    |> List.sortBy(fun (k, _) -> k.Indices.[0])
                    |> List.map fst
        dupes




