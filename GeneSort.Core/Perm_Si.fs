namespace GeneSort.Core
open GeneSort.Core.Permutation
open CollectionUtils
open FSharp.UMX
open Combinatorics

// Perm_Si: a permutation that is its own inverse (p ∘ p = identity)
type Perm_Si = private Perm_Si of permutation with

    // Static method to create a Perm_Rs, validating self-inverse property
    static member create (arr: int array) : Perm_Si =
        if arr.Length < 1 then
            failwith "array must contain items"
        let perm = permutation.create arr
        if not (Permutation.isSelfInverse perm) then
            failwith "Invalid: permutation must be self-inverse"
        Perm_Si perm

    // Static method to create a Perm_Rs, validating self-inverse property
    static member createUnsafe (arr: int array) : Perm_Si =
        let perm = permutation.createUnsafe arr
        Perm_Si perm

    // Property to access the underlying permutation
    member this.Permutation =
        match this with
        | Perm_Si perm -> perm

    member this.Id
        with get () =
            match this with
            | Perm_Si perm -> perm.Id

    member this.Order
        with get () =
            match this with
            | Perm_Si perm -> perm.Order

    member this.Array
        with get () =
            match this with
            | Perm_Si perm -> perm.Array

    member this.equals (other: Perm_Si) : bool =
            this.Permutation.equals(other.Permutation)


module Perm_Si =

    type MutationMode =
        | Ortho
        | Para
        | NoAction

    /// The returned TwoOrbits are ordered ascending by their first index.
    /// <param name="perm_Si">The Perm_Si to convert.</param>
    /// <exception cref="System.ArgumentException">Thrown when the Perm_Si order is odd, or the array is invalid.</exception>
    let getTwoOrbits (perm_Si: Perm_Si) : TwoOrbit array =
        let order = perm_Si.Order |> UMX.untag
        if order < 0 || order % 2 <> 0 then
            failwith "Perm_Si order must be non-negative and even"
        if perm_Si.Array.Length <> order then
            failwith "Perm_Si array length must match order"
        let visited = Array.create order false
        let rec findOrbits i acc =
            if i >= order then acc
            elif visited.[i] then findOrbits (i + 1) acc
            else
                visited.[i] <- true
                let j = perm_Si.Array.[i]
                if j < 0 || j >= order then
                    failwith "Perm_Si array contains invalid indices"
                visited.[j] <- true
                let twoOrbit = TwoOrbit.create [i; j]
                findOrbits (i + 1) (twoOrbit :: acc)
        findOrbits 0 [] |> List.rev |> Array.ofList


    // Create a Perm_Si from a list of disjoint transpositions
    let fromTranspositions 
                (n: int) 
                (transpositions: List<int * int>) : Perm_Si =
        if n < 0 then
            failwith "Perm_Si order must not be negative"
        let arr = Array.init n id
        let mutable seen = Set.empty
        for (i, j) in transpositions do
            if i < 0 || i >= n || j < 0 || j >= n || i = j then
                failwith "Invalid transposition: must be distinct indices in [0, n-1]"
            if Set.contains i seen || Set.contains j seen then
                failwith "Transpositions must be disjoint"
            seen <- seen |> Set.add i |> Set.add j
            arr.[i] <- j
            arr.[j] <- i
        Perm_Si.create arr


    // Creates a Perm_Si of order n with a maximum number of orbits
    let saturatedWithTwoCycles (order: int) : Perm_Si =
        let cycleCount = order / 2
        let transpositions =
            [ for i in 0 .. (cycleCount - 1) -> (2 * i, 2 * i + 1) ]
        fromTranspositions order transpositions 


    // Creates a Perm_Si of order n with a maximum number of orbits
    let unSaturatedWithTwoCycles (order:int) (cycleCount:int) : Perm_Si =
        if(cycleCount > order / 2) then
            failwithf "cycleCount must be at most %d" (order/2)
        let cycleCount = order / 2
        let transpositions =
            [ for i in 0 .. (cycleCount - 1) -> (2 * i, 2 * i + 1) ]
        fromTranspositions order transpositions


    // makeReflection returns a reflection of the given Perm_Si
    let makeReflection (perm_Rs: Perm_Si) : Perm_Si =
        let origArray = perm_Rs.Permutation.Array
        let arrayLength = origArray.Length
        let _refl pos = 
            arrayLength - pos - 1
        Perm_Si.createUnsafe
            (Array.init arrayLength (fun dex -> origArray.[_refl dex] |> _refl))


    // isReflectionSymmetric checks if a Perm_Si is reflection symmetric
    let isReflectionSymmetric (perm_Rs: Perm_Si) :bool =
        let arr = perm_Rs.Permutation.Array
        let reflArray = (makeReflection perm_Rs).Permutation.Array
        arrayEquals arr reflArray


    // Creates a Perm_Si of given order packed with non-overlapping transpositions (i, i+k)
    let steppedOffsetTwoCycle 
            (order: int) (start:int) 
            (offset: int) (count: int) : Perm_Si =
        let transpositions =
            steppedOffsetPairs start order offset |> Seq.truncate count |> Seq.toList
        fromTranspositions order transpositions


    // Creates a Perm_Si of given even with count sequential 
    // transpositions (i, i+1) starting at i = startIndex
    let adjacentTwoCycles (order: int) (startIndex:int) (count: int): Perm_Si =
        if (order - startIndex) < 2 * count then
            failwith (sprintf "%d cycles is too many for Permutation of order %d and sda %d" 
                        count order startIndex)
        let transpositions =
            steppedOffsetPairs startIndex order 1 |> Seq.truncate count |> Seq.toList
        fromTranspositions order transpositions


    // Conjugate a Perm_Si (t) with a permutation (p) : p^-1 ∘ t ∘ p
    // This always makes a Perm_Si
    let conjugate (t: Perm_Si) (p: permutation) : Perm_Si =
        let arrT = t.Permutation.Array
        let arrP = p.Array
        if Array.length arrT <> Array.length arrP then
            failwith "Permutations must have the same order"
        let res = compose (compose (inverse p) t.Permutation) p
        Perm_Si.createUnsafe res.Array


    // given a Perm_Si of Order n, this returns a Perm_Si of order 2n
    // that is reflection symmetric
    let unfoldReflection (perm_Rs:Perm_Si) : Perm_Si =
        let sa = perm_Rs.Array
        let o = %perm_Rs.Order
        let newArray = Array.create (o * 2) 0
        for i in 0 .. (o - 1) do
            newArray.[i] <- sa.[i]
            newArray.[o + i] <- 2 * o - sa.[o - i - 1] - 1
        Perm_Si.createUnsafe newArray


    // Generates a random Perm_Rs by conjugating a saturated Perm_Si with a random permutation
    let makeRandoms (indexShuffler: int -> int) (order: int) : Perm_Si seq =
        seq {
            while true do
                let initialPerm = Permutation.randomPermutation indexShuffler order
                yield conjugate (saturatedWithTwoCycles order) initialPerm
        }

    // Mutates a Perm_Si in a minimal way based on siMutationMode
    let mutate 
            (indexPicker: int -> int) 
            (siMutationMode:MutationMode) 
            (permSi: Perm_Si) : Perm_Si =

        // for None mode, return the permutation as is
        if (siMutationMode = MutationMode.NoAction) then
            permSi
        else
            let first, second = pickAndOrderTwoDistinct indexPicker %permSi.Order
            // if the two items are in the same orbit, do nothing
            if (permSi.Array.[first] = second) then
                permSi
            else
                let fpLow, fpHi = orderItems first permSi.Array.[first]
                let spLow, spHi = orderItems second permSi.Array.[second]
                let newArray = Array.copy permSi.Array
                match siMutationMode with
                | MutationMode.Ortho -> 
                   newArray.[fpLow] <- spLow
                   newArray.[spLow] <- fpLow
                   newArray.[spHi] <- fpHi
                   newArray.[fpHi] <- spHi
                   Perm_Si.createUnsafe newArray
                | MutationMode.Para ->
                   newArray.[fpLow] <- spHi
                   newArray.[spHi] <- fpLow
                   newArray.[fpHi] <- spLow
                   newArray.[spLow] <- fpHi
                   Perm_Si.createUnsafe newArray
                | MutationMode.NoAction -> permSi


    /// Computes the TwoOrbitType breakdown for a given Perm_Rs
    /// <param name="order">The order (must be non-negative and even).</param>
    /// <param name="perm_Rs">The Perm_Rs to analyze.</param>
    /// <returns>A TwoOrbitBreakdown containing the count of each TwoOrbitType.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the Perm_Rs order is negative, odd, or the array is invalid.</exception>
    let getTwoOrbitTypeCounts (order: int) (perm_Si: Perm_Si) :(int*int*int) =
        // Helper function to extract components from a triple
        let fst3 (a, _, _) = a
        let snd3 (_, b, _) = b
        let trd3 (_, _, c) = c
        let twoOrbits = getTwoOrbits perm_Si
        let counts = 
            twoOrbits 
            |> Array.fold (fun (ortho, para, self) orbit ->
                match TwoOrbit.getTwoOrbitType order orbit with
                | TwoOrbitType.Ortho -> (ortho + 1, para, self)
                | TwoOrbitType.Para -> (ortho, para + 1, self)
                | TwoOrbitType.SelfRefl -> (ortho, para, self + 1)
            ) (0, 0, 0)
        (fst3 counts, snd3 counts, trd3 counts)


    /// Creates a Perm_Rs from a list of TwoOrbit pairs by mutating the identity Perm
    let fromTwoOrbitPair (twoOrbitPairs : TwoOrbitPair array) : Perm_Si =
        if twoOrbitPairs.Length < 1 then
            failwith "RsOrbitPair list must have an element"

        let arr = Array.init twoOrbitPairs.[0].Order id
        for orbitPair in twoOrbitPairs do
            let a, b = orbitPair.First.IndicesTuple
            arr.[a] <- b
            arr.[b] <- a
            if (orbitPair.Second.IsSome) then
                let c, d = orbitPair.Second.Value.IndicesTuple
                arr.[c] <- d
                arr.[d] <- c

        Perm_Si.create arr


