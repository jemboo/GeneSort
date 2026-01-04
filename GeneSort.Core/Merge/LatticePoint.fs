
namespace GeneSort.Core

open FSharp.UMX
open System

[<Measure>] type latticeDistance
[<Measure>] type latticeDimension

[<Struct; CustomEquality; CustomComparison>]
type latticePoint = 
    private { 
        coords: int[]
        hashCode: int 
    }
    with
        static member create(arr: int[]) = 
            let copy = Array.copy arr
            // Use Span-based hashing for speed if available, or manual loop
            let mutable h = 17
            for x in copy do h <- h * 31 + x
            { coords = copy; hashCode = h }

        member p.Coords = p.coords
        member p.Dimension = p.coords.Length
        member this.Item with get(i:int) = this.coords.[i]
        member this.Sum with get() = Array.sum this.coords

        override p.GetHashCode() = p.hashCode
        
        override p.Equals(obj) =
            match obj with
            | :? latticePoint as other ->
                p.hashCode = other.hashCode && 
                ReadOnlySpan(p.coords).SequenceEqual(ReadOnlySpan(other.coords))
            | _ -> false

        interface IComparable with
            member this.CompareTo(obj) =
                match obj with
                | :? latticePoint as other ->
                    let lenCompare = compare this.coords.Length other.coords.Length
                    if lenCompare <> 0 then lenCompare
                    else
                        ReadOnlySpan(this.coords).SequenceCompareTo(ReadOnlySpan(other.coords))
                | _ -> invalidArg "obj" "Cannot compare latticePoint"



module LatticePoint =

    /// Predicate: true iff latticePoint's coords are non-decreasing
    let isNonDecreasing0 (p:latticePoint) =
        let a = p.coords
        let n = a.Length
        let mutable i = 0
        let mutable ok = true
        while i < n - 1 && ok do
            if a.[i] > a.[i+1] then ok <- false
            i <- i + 1
        ok

    let isNonDecreasing (p: latticePoint) =
        let span = ReadOnlySpan(p.Coords)
        let mutable ok = true
        let mutable i = 0
        while i < span.Length - 1 && ok do
            if span.[i] > span.[i+1] then ok <- false
            i <- i + 1
        ok


    /// helper to compare latticePoints by coords (value equality)
    let equalCoords (a:int[]) (b:int[]) =
        if a.Length <> b.Length then false
        else
            let mutable ok = true
            let mutable i = 0
            while ok && i < a.Length do
                if a.[i] <> b.[i] then ok <- false
                i <- i + 1
            ok
    
    let latticePointHash (p:latticePoint) : int =
        let mutable h = 17
        for x in p.coords do
            h <- (h * 31) + x
        h


    /// Generate all lattice points where each coordinate is in 0..maxValue
    let latticeCube 
            (dim:int<latticeDimension>) 
            (edgeLength:int<latticeDistance>) : seq<latticePoint> =
        seq {
            let current = Array.zeroCreate %dim

            let rec loop pos =
                seq {
                    if pos = %dim then
                        yield latticePoint.create (Array.copy current)
                    else
                        for v in 0 .. %edgeLength do
                            current.[pos] <- v
                            yield! loop (pos + 1)
                }

            yield! loop 0
        }

    /// Generates all lattice points of length dim whose entries lie in 0 .. maxValue
    /// and are non-decreasing
    let latticeCubeVV 
                (dim:int<latticeDimension>) 
                (edgeLength:int<latticeDistance>) : seq<latticePoint> =
        latticeCube dim edgeLength |> Seq.filter isNonDecreasing


    /// Generates all lattice points of length dim whose entries:
    /// • sum to latticelevel
    /// • lie in 0 .. maxDistance
    let boundedLevelSet 
                (dim:int<latticeDimension>) 
                (latticelevel:int<latticeDistance>) 
                (edgeLength:int<latticeDistance>) : seq<latticePoint> =
        seq {
            let current = Array.zeroCreate %dim

            let rec loop pos sumRemaining =
                seq {
                    if pos = %dim then
                        if sumRemaining = 0 then
                            yield latticePoint.create (Array.copy current)
                    else
                        // The remaining slots count
                        let slotsLeft = %dim - pos

                        // Lowest legal value is 0 or whatever
                        let lowerBound =
                            max 0 (sumRemaining - %edgeLength * (slotsLeft - 1))

                        // Upper bound is limited by maxValue and remaining sum
                        let upperBound =
                            min %edgeLength sumRemaining

                        for v in lowerBound .. upperBound do
                            current.[pos] <- v
                            yield! loop (pos + 1) (sumRemaining - v)
                }

            yield! loop 0 %latticelevel
        }


    /// Generates all lattice points of length dim whose entries:
    /// • sum to levelSum
    /// • lie in 0 .. maxValue
    let boundedLevelSetVV 
                    (dim:int<latticeDimension>) 
                    (latticelevel:int<latticeDistance>) 
                    (maxDistance:int<latticeDistance>) : seq<latticePoint> =
        boundedLevelSet dim latticelevel maxDistance |> Seq.filter isNonDecreasing



    let getUnderCovers (subject:latticePoint) : latticePoint[] =
        let n = subject.Dimension
        let results = ResizeArray<latticePoint>()
        for i in 0 .. n-1 do
            if subject.[i] - 1 >= 0 then
                let arr = Array.copy subject.coords
                arr.[i] <- arr.[i] - 1
                results.Add (latticePoint.create arr)
        results.ToArray()


    let getOverCovers (subject:latticePoint) (maxDistance:int<latticeDistance>) : latticePoint[] =
        let n = subject.Dimension
        let results = ResizeArray<latticePoint>()
        for i in 0 .. n-1 do
            if subject.[i] < %maxDistance then
                let arr = Array.copy subject.coords
                arr.[i] <- arr.[i] + 1
                results.Add (latticePoint.create arr)
        results.ToArray()



    let getUnderCoversVV (subject:latticePoint) : latticePoint[] =
        getUnderCovers subject |> Array.filter isNonDecreasing


    let getOverCoversVV (subject:latticePoint) (maxDistance:int<latticeDistance>) : latticePoint[] =
        getOverCovers subject maxDistance |> Array.filter isNonDecreasing


    /// returns the (unique) index i where lowPoint.[i] + 1 = hiPoint.[i]
    let getOverIndex (lowPoint: latticePoint) (hiPoint: latticePoint) : int =
        let lowArr = lowPoint.Coords
        let hiArr = hiPoint.Coords
        let dim = lowArr.Length
        let mutable foundIndex = -1

        for i in 0 .. dim - 1 do
            let a = lowArr.[i]
            let b = hiArr.[i]
            if a + 1 = b then
                if foundIndex = -1 then foundIndex <- i
                else invalidArg "hiPoint" "Multiple increments"
            elif a <> b then
                invalidArg "hiPoint" "Mismatch"
            
        if foundIndex = -1 then failwith "No increment found"
        foundIndex


    // This is used by updateWithLatticePoint to convert a path in the mergeLattice to a permutation.
    // Place (level - 1) in the index returned, where level here is the level of the lowPoint
    let getPermutationIndex 
                (edgeLength: int<latticeDistance>) 
                (lowPoint:latticePoint) 
                (hiPoint:latticePoint) : int =
        let index = getOverIndex lowPoint hiPoint
        %edgeLength * index +  %edgeLength - lowPoint.coords.[index] - 1


