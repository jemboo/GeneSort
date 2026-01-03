namespace GeneSort.Core

open System.Security.Cryptography
open System
open CollectionUtils
open Combinatorics
open FSharp.UMX

[<Measure>] type Order

// --- Struct DU for Zero-Allocation Wrapping ---
[<Struct>]
type permutation = private Perm of arr: int array * id: Guid with
    
    static member private ComputeGuid (arr: int array) : Guid =
        use sha256 = SHA256.Create()
        let bytes = Array.zeroCreate (arr.Length * 4)
        Buffer.BlockCopy(arr, 0, bytes, 0, arr.Length * 4)
        let hash = sha256.ComputeHash(bytes)
        Guid(hash.[0..15])

    static member create (arr: int array) : permutation =
        let n = Array.length arr
        if n = 0 then failwith "array cannot be empty"
        let sorted = Array.sort arr
        let isValid = Array.forall2 (fun i v -> i = v) [|0 .. n-1|] sorted
        if not isValid then 
            failwith "Invalid permutation: must contain each integer from 0 to n-1 exactly once"
        Perm (arr, permutation.ComputeGuid arr)

    static member createUnsafe (arr: int array) : permutation =
        Perm (arr, permutation.ComputeGuid arr)

    member this.Array = match this with Perm (arr, _) -> arr
    member this.Id = match this with Perm (_, id) -> id
    member this.Order = UMX.tag<Order> this.Array.Length

    member this.equals (other: permutation) : bool =
            this.Array.Length = other.Array.Length &&
            arrayEquals this.Array other.Array


module Permutation =
    
    // --- Identity Optimization ---
    
    // Pre-cache IDs for common orders to make identity check O(1)
    let private identityIdCache = 
        use sha = SHA256.Create() // Create once for the entire initialization
        [1..128] 
        |> List.map (fun n -> 
            let arr = Array.init n id
            let bytes = Array.zeroCreate (arr.Length * 4)
            Buffer.BlockCopy(arr, 0, bytes, 0, arr.Length * 4)
            let hash = sha.ComputeHash(bytes)
            n, Guid(hash.[0..15]))
        |> Map.ofList

    let getIdentityId (order: int) =
        match identityIdCache.TryFind order with
        | Some id -> id
        | None -> 
            let arr = Array.init order id
            // Manual computation for rare large orders
            let bytes = Array.zeroCreate (arr.Length * 4)
            Buffer.BlockCopy(arr, 0, bytes, 0, arr.Length * 4)
            use sha = SHA256.Create()
            Guid(sha.ComputeHash(bytes).[0..15])

    let identity (order: int) : permutation =
        if order <= 0 then failwith "Permutation order must be positive"
        Perm (Array.init order id, getIdentityId order)

    let isIdentity (perm: permutation) : bool =
        perm.Id = getIdentityId (int perm.Order)

    // --- Core Operations ---

    let compose (p1: permutation) (p2: permutation) : permutation =
        let arr1 = p1.Array
        let arr2 = p2.Array
        if arr1.Length <> arr2.Length then failwith "Orders must match"
        permutation.createUnsafe (Array.init arr1.Length (fun i -> arr2.[arr1.[i]]))

    let inverse (perm: permutation) : permutation =
        let arr = perm.Array
        let n = arr.Length
        let inv = Array.zeroCreate n
        for i in 0 .. n-1 do
            inv.[arr.[i]] <- i
        permutation.createUnsafe inv

    let conjugate (p: permutation) (q: permutation) : permutation =
        compose (compose (inverse q) p) q

    // --- Sequence & Analysis ---

    let powerSequence (perm: permutation) : permutation seq =
        seq {
            let mutable cur = perm
            while true do
                yield cur
                cur <- compose perm cur
        }
        
    let isSelfInverse (perm: permutation) : bool =
        let n = int perm.Order
        (compose perm perm).Id = getIdentityId n

    let getFixedPoints (perm: permutation) : int array =
        perm.Array 
        |> Array.indexed 
        |> Array.filter (fun (i, x) -> i = x) 
        |> Array.map fst

    let toArrayNotation (perm: permutation) : string =
        sprintf "[%s]" (perm.Array |> Array.map string |> String.concat "; ")

    // --- Visualization & Orbits ---

    let toOrbitSet (perm: permutation) : OrbitSet =
        let arr = perm.Array
        let n = arr.Length
        let visited = Array.create n false
        let rec build i acc =
            if visited.[i] then acc
            else
                visited.[i] <- true
                build arr.[i] (i :: acc)
        
        let orbits = [
            for i in 0 .. n-1 do
                if not visited.[i] then
                    yield Orbit.create (build i [] |> List.rev)
        ]
        OrbitSet.create orbits n


    /// Converts an OrbitSet to a Permutation.
    /// The orbits must cover indices from 0 to order-1.
    let fromOrbitSet (orbitSet:OrbitSet) : permutation =
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
        permutation.create permArray


    let toBoolArrays (perm: permutation) : bool[][] =
        let n = int perm.Order
        if n <= 1 then [||]
        else
            let vals = perm.Array
            Array.init (n + 1) (fun threshold ->
                vals |> Array.map (fun v -> v >= threshold))


    // returns a random Permutation by shuffling the identity Permutation
    let randomPermutation (indexShuffler: int -> int) (order: int) : permutation =
        let initialPerm = identity order
        let shuffled = fisherYatesShuffle indexShuffler initialPerm.Array
                       |> Seq.toArray
        permutation.createUnsafe shuffled

    // Returns a "rotated" identity permutation of given by k positions (positive k for left, negative for right)
    let rotated (k: int) (order:int) : permutation =
        let arr = (identity order).Array
        if order <= 0 then
            failwith "Permutation order must be positive"
        let k = k % order // Normalize k to avoid unnecessary cycles
        let k = if k < 0 then k + order else k // Handle negative k
        permutation.createUnsafe (Array.init order (fun i -> arr.[(i + k + order) % order]))
