namespace GeneSort.Core

open FSharp.UMX
open System


[<Struct>]
type latticePathPermutation = 
    { 
        Lp: latticePoint
        // We keep the array as the "source of truth"
        PArray: int[] 
    }
    with
        static member createEmpty (latticeDimension: int<latticeDimension>) (pathLength: int<latticeDistance>) = 
            { Lp = latticePoint.create(Array.zeroCreate %latticeDimension)
              PArray = Array.create %pathLength -1 }

        static member create (lp: latticePoint) (pArray: int[]) = 
            { Lp = lp; PArray = pArray }

        member this.Length = this.PArray.Length

        /// Accessor for external consumption
        member this.Item
            with get(i: int) = 
                let v = this.PArray.[i]
                if v = -1 then None else Some v

        member this.IsComplete = 
            Array.IndexOf(this.PArray, -1) = -1



module LatticePathPermutation =

    /// Deep copy the array, but return a new Struct
    let copy (lptp: latticePathPermutation) =
        { Lp = lptp.Lp; PArray = Array.copy lptp.PArray }
        
    let updateWithLatticePoint 
            (edgeLength: int<latticeDistance>) 
            (newLevel: int)
            (newLp: latticePoint) 
            (lpPerm: latticePathPermutation) : latticePathPermutation =
        
        // 1. Calculate the index before we "move"
        let index = LatticePoint.getPermutationIndex edgeLength lpPerm.Lp newLp
        
        // 2. Create the new array for the branched path
        let newArray = Array.copy lpPerm.PArray
        newArray.[index] <- newLevel
        
        // 3. Return a brand new struct with the new point and the new array
        { Lp = newLp; PArray = newArray }

    let toPermutation (lptp: latticePathPermutation) : permutation =
        if not lptp.IsComplete then
            failwith "Cannot convert an incomplete lattice path to a permutation."
        permutation.create (Array.copy lptp.PArray)


//[<Struct>]
//type latticePathPermutation = 
//    private { 
//        mutable lp: latticePoint
//        // Using -1 as a sentinel for 'None' to avoid Option allocation overhead
//        pArray: int[] 
//    }
//    with
//        static member createEmpty (latticeDimension: int<latticeDimension>) (pathLength: int<latticeDistance>) = 
//            { lp = latticePoint.create(Array.zeroCreate %latticeDimension)
//              pArray = Array.create %pathLength -1 }

//        static member create (lp: latticePoint) (pArray: int[]) = 
//            { lp = lp; pArray = pArray }

//        member this.Length = this.pArray.Length
        
//        member this.LatticePoint 
//            with get() = this.lp
//            and set (value) = this.lp <- value

//        member this.PermutationArray = this.pArray

//        /// Accessor that handles the sentinel conversion to Option for external consumption
//        member this.Item
//            with get(i: int) = 
//                let v = this.pArray.[i]
//                if v = -1 then None else Some v
//            and set (i: int) (value: int option) =
//                if this.pArray.[i] <> -1 then 
//                    invalidArg (string i) "Position is already occupied."
//                match value with
//                | Some v -> this.pArray.[i] <- v
//                | None   -> this.pArray.[i] <- -1

//        member this.IsComplete = 
//            // Using a while loop or System.Array.IndexOf is faster than forall
//            Array.IndexOf(this.pArray, -1) = -1



//module LatticePathPermutation =

//    /// Creates a deep copy of the permutation to maintain immutability during path branching
//    let copy (lptp: latticePathPermutation) =
//        { lp = lptp.LatticePoint; pArray = Array.copy lptp.PermutationArray }
        
//    let updateWithLatticePoint 
//            (edgeLength: int<latticeDistance>) 
//            (newLevel: int)
//            (newLp: latticePoint) 
//            (lpPerm: latticePathPermutation) : latticePathPermutation =
        
//        // 1. Branch the path (copy current state)
//        let newLptp = copy lpPerm
        
//        // 2. Update the current coordinate position in the lattice
//        newLptp.LatticePoint <- newLp
        
//        // 3. Determine the index in the permutation array based on the movement
//        let index = LatticePoint.getPermutationIndex edgeLength lpPerm.LatticePoint newLp
        
//        // 4. Assign the level (no boxing to Option required here)
//        newLptp.PermutationArray.[index] <- newLevel
//        newLptp

//    let toPermutation (lptp: latticePathPermutation) : permutation =
//        if not lptp.IsComplete then
//            failwith "Cannot convert an incomplete lattice path to a permutation."
            
//        // Since we stored raw ints, we can pass the array directly to the permutation constructor
//        permutation.create (Array.copy lptp.PermutationArray)