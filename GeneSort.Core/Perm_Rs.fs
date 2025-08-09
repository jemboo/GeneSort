namespace GeneSort.Core
open FSharp.UMX
open Combinatorics

// Perm_Rs type: a permutation that is its own inverse (p ∘ p = identity)
// Is reflection symmetric, and has an order that is divisible by 4.
type Perm_Rs = private Perm_Rs of Perm_Si with
    // Static method to create a Perm_Rs, validating self-inverse property
    static member create (arr: int array) : Perm_Rs =
        if arr.Length < 4 then
            failwith "Perm_Rs order must be at least 4"
        if arr.Length % 2 <> 0 then
            failwith "Perm_Rs order must be divisible by 2"
        let perm_Si = Perm_Si.create arr
        if not (Perm_Si.isReflectionSymmetric perm_Si) then
            failwith "Invalid Perm_Rs: permutation must be self-inverse"
        Perm_Rs perm_Si

    // Static method to create a Perm_Rs, validating self-inverse property
    static member createUnsafe (arr: int array) : Perm_Rs =
        let perm = Perm_Si.createUnsafe arr
        Perm_Rs perm

    // Property to access the underlying permutation
    member this.Permutation
        with get () =
            match this with
            | Perm_Rs perm -> perm.Permutation

    // Property to access the underlying Perm_Si
    member this.Perm_Si
        with get () =
        match this with
        | Perm_Rs perm_rs -> perm_rs

    member this.Id
        with get () =
            match this with
            | Perm_Rs perm -> perm.Id

    member this.Order
        with get () =
            match this with
            | Perm_Rs perm -> perm.Order
             
    member this.Array
        with get () =
            match this with
            | Perm_Rs perm -> perm.Array

    member this.equals (other: Perm_Rs) : bool =
            this.Permutation.equals(other.Permutation)
