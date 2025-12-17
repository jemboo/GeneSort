namespace GeneSort.Core

open System.Security.Cryptography
open System
open CollectionUtils
open Combinatorics
open FSharp.UMX

[<Measure>] type Order

// Permutation type as a single-case discriminated union with Guid Id
type Permutation = private Permutation of arr: int array * id: Guid ref with
    // Compute Guid from int array
    static member private ComputeGuid (arr: int array) : Guid =
        use sha256 = SHA256.Create()
        // Convert int array to byte array (4 bytes per int, little-endian)
        let bytes = Array.zeroCreate (arr.Length * 4)
        Buffer.BlockCopy(arr, 0, bytes, 0, arr.Length * 4)
        // Compute SHA256 hash and take first 16 bytes for Guid
        let hash = sha256.ComputeHash(bytes)
        Guid(hash.[0..15])

    // Static method to create a permutation, validating input
    static member create (arr: int array) : Permutation =
        let n = Array.length arr
        if n = 0 then
            failwith "array cannot be empty"
        let sorted = Array.sort arr
        let isValid = Array.forall2 (fun i v -> i = v) [|0 .. n-1|] sorted
        if not isValid then
            failwith "Invalid permutation: must contain each integer from 0 to n-1 exactly once"
        Permutation (arr, ref Guid.Empty)

    static member createUnsafe (arr: int array) : Permutation =
        Permutation (arr, ref Guid.Empty)

    // Property to access the underlying array
    member this.Array =
        match this with
        | Permutation (arr, _) -> arr

    // Property to access or compute the Id
    member this.Id
        with get () =
            match this with
            | Permutation (arr, idRef) ->
                if !idRef = Guid.Empty then
                    idRef := Permutation.ComputeGuid arr
                !idRef

    member this.Order =
        UMX.tag<Order> this.Array.Length

    member this.equals (other: Permutation) : bool =
            this.Array.Length = other.Array.Length &&
            arrayEquals this.Array other.Array


module Permutation =
    // Create identity permutation of order n
    let identity (order: int) : Permutation =
        if order <= 0 then
            failwith "Permutation order must be positive"
        Permutation.create [|0 .. order-1|]

    // Check if a permutation is the identity permutation
    let isIdentity (permutation: Permutation) : bool =
        let arr = permutation.Array
        let mutable isIdentity = true
        let mutable i = 0
        while i < arr.Length && isIdentity do
            if arr.[i] <> i then
                isIdentity <- false
            i <- i + 1
        isIdentity


    // returns a random Permutation by shuffling the identity Permutation
    let randomPermutation (indexShuffler: int -> int) (order: int) : Permutation =
        let initialPerm = identity order
        let shuffled = fisherYatesShuffle indexShuffler initialPerm.Array
                       |> Seq.toArray
        Permutation.createUnsafe shuffled

    // Returns a "rotated" identity permutation of given by k positions (positive k for left, negative for right)
    let rotated (k: int) (order:int) : Permutation =
        let arr = (identity order).Array
        if order <= 0 then
            failwith "Permutation order must be positive"
        let k = k % order // Normalize k to avoid unnecessary cycles
        let k = if k < 0 then k + order else k // Handle negative k
        Permutation.createUnsafe (Array.init order (fun i -> arr.[(i + k + order) % order]))

    // Compose two permutations: applies p1 then p2 (p2 ∘ p1)
    let compose (p1: Permutation) (p2: Permutation) : Permutation =
        let arr1 = p1.Array
        let arr2 = p2.Array
        if Array.length arr1 <> Array.length arr2 then
            failwith "Permutations must have the same order"
        Permutation.createUnsafe (Array.init (Array.length arr1) (fun i -> arr2.[arr1.[i]]))

    let powerSequence (perm: Permutation) : Permutation seq =
        let mutable curPerm = perm
        seq
            {
                while true do
                    yield curPerm
                    curPerm <- compose perm curPerm
            }
        
    let isSelfInverse (perm: Permutation) : bool =
        let arr = perm.Array
        let n = Array.length arr
        let composed = compose perm perm
        composed.Array = (identity n).Array

    // Inverse of a permutation
    let inverse (perm: Permutation) : Permutation =
        let arr = perm.Array
        let n = Array.length arr
        let inv = Array.zeroCreate n
        for i in 0 .. n-1 do
            inv.[arr.[i]] <- i
        Permutation.createUnsafe inv

    // Conjugate a permutation: q^-1 ∘ p ∘ q
    let conjugate (p: Permutation) (q: Permutation) : Permutation =
        let arr1 = p.Array
        let arr2 = q.Array
        if Array.length arr1 <> Array.length arr2 then
            failwith "Permutations must have the same order"
        compose (compose (inverse q) p) q


    // Returns an array of indices where the permutation fixes the element (perm[i] = i)
    let getFixedPoints (perm: Permutation) : int array =
        perm.Array
        |> Array.indexed
        |> Array.filter (fun (i, x) -> i = x)
        |> Array.map fst

    // Helper to print permutation in array notation
    let toArrayNotation (perm: Permutation) : string =
        sprintf "[%s]" (perm.Array |> Array.map string |> String.concat "; ")


    /// Converts an OrbitSet to a Permutation.
    /// The orbits must cover indices from 0 to order-1.
    let fromOrbitSet (orbitSet:OrbitSet) : Permutation =
        let indices = 
            orbitSet.Orbits 
            |> List.collect (fun orbit -> orbit.Indices)
            |> List.distinct
            |> List.sort
            |> Array.ofList
        let order = indices.Length
        if (not (arrayEquals indices (Array.init order id))) then
            failwith "Orbits must cover indices 0 to order-1"
        let permArray = Array.zeroCreate order
        for orbit in orbitSet.Orbits do
            let indices = orbit.Indices
            if indices.Length > 0 then
                for i in 0 .. indices.Length - 2 do
                    permArray.[indices.[i]] <- indices.[i + 1]
                permArray.[indices.[indices.Length - 1]] <- indices.[0]
        Permutation.create permArray

        
    // Find orbits of the elements of a permutation, including fixed points
    let toOrbitSet (perm: Permutation) : OrbitSet =
        let arr = perm.Array
        let n = Array.length arr
        let visited = Array.create n false
        let rec buildOrbit current orbit =
            if visited.[current] then orbit
            else
                visited.[current] <- true
                if orbit |> List.head = current then
                    buildOrbit arr.[current] orbit
                else
                    buildOrbit arr.[current] (current :: orbit)
        let rec findOrbits i acc =
            if i >= n then acc
            elif visited.[i] then findOrbits (i + 1) acc
            else
                let orbitIndices = buildOrbit i [i] |> List.rev
                let orbit = Orbit.create orbitIndices
                findOrbits (i + 1) (orbit :: acc)
        OrbitSet.create (List.rev (findOrbits 0 [])) n


    let toBoolArrays (perm: Permutation) : bool[][] =
        let n = %perm.Order
        if perm.Order <= 1<Order> then
            [||]
        else
            let thresholds = [| 0 .. %perm.Order |]
            let vals = perm.Array
            thresholds 
            |> Array.map (
                fun threshold ->
                    vals |> Array.map (fun v -> v >= threshold ) )