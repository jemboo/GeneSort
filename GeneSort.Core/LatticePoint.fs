namespace GeneSort.Core
open FSharp.UMX
open System


[<Measure>] type latticeDistance
[<Measure>] type latticeDimension

[<Struct; CustomEquality; NoComparison>]
type latticePoint = { coords: int[] }
    with
        static member create(arr:int[]) = { coords = Array.copy arr }
        member p.Sum = Array.sum p.coords
        member p.Dimension = p.coords.Length
        member this.Item
            with get(i:int) = this.coords.[i]
        override p.ToString() = sprintf "[|%s|]" (String.Join("; ", p.coords))
        override p.Equals(obj) =
            match obj with
            | :? latticePoint as other ->
                // structural equality on array contents
                p.Dimension = other.Dimension &&
                Array.forall2 (=) p.coords other.coords
            | _ -> false

        override p.GetHashCode() =
            // robust structural hash built from coords
            let mutable h = 17
            for x in p.coords do
                h <- h * 31 + x
            h


module LatticePoint =

    /// Predicate: true iff latticePoint's coords are non-decreasing
    let isNonDecreasing (p:latticePoint) =
        let a = p.coords
        let n = a.Length
        let mutable i = 0
        let mutable ok = true
        while i < n - 1 && ok do
            if a.[i] > a.[i+1] then ok <- false
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
    let latticeRect (dim:int<latticeDimension>) (maxValue:int<latticeDistance>) : seq<latticePoint> =
        seq {
            let current = Array.zeroCreate %dim

            let rec loop pos =
                seq {
                    if pos = %dim then
                        yield { coords = Array.copy current }
                    else
                        for v in 0 .. %maxValue do
                            current.[pos] <- v
                            yield! loop (pos + 1)
                }

            yield! loop 0
        }


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
                            yield { coords = Array.copy current }
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
                results.Add { coords = arr }
        results.ToArray()


    let getOverCovers (subject:latticePoint) (maxDistance:int<latticeDistance>) : latticePoint[] =
        let n = subject.Dimension
        let results = ResizeArray<latticePoint>()
        for i in 0 .. n-1 do
            if subject.[i] + 1 < %maxDistance then
                let arr = Array.copy subject.coords
                arr.[i] <- arr.[i] + 1
                results.Add { coords = arr }
        results.ToArray()



    let getUnderCoversVV (subject:latticePoint) : latticePoint[] =
        getUnderCovers subject |> Array.filter isNonDecreasing


    let getOverCoversVV (subject:latticePoint) (maxDistance:int<latticeDistance>) : latticePoint[] =
        getOverCovers subject maxDistance |> Array.filter isNonDecreasing


    /// returns the (unique) index i where lowPoint.[i] + 1 = hiPoint.[i]
    let getOverIndex (lowPoint:latticePoint) (hiPoint:latticePoint) : int =
    
        // First check same dimension
        let dim = lowPoint.Dimension
        if hiPoint.Dimension <> dim then
            invalidArg "hiPoint" "latticePoints must have the same dimension"

        // Scan coordinates
        let mutable foundIndex = -1

        for i in 0 .. dim - 1 do
            let a = lowPoint.[i]
            let b = hiPoint.[i]

            if a + 1 = b then
                // First candidate?
                if foundIndex = -1 then
                    foundIndex <- i
                else
                    // More than one coordinate differs by +1 → invalid
                    invalidArg "hiPoint" "More than one coordinate is increased by 1."
            elif a <> b then
                // Any other mismatch → invalid
                invalidArg "hiPoint" "Points differ in a coordinate not equal to +1."

        if foundIndex = -1 then
            invalidArg "hiPoint" "No coordinate was incremented by exactly 1."

        foundIndex


    // This is used by latticePointToPermtation to convert a path in the mergeLattice to a permutation.
    // Place (level - 1) in the index returned, where level here is the level of the lowPoint
    let getPermutationIndex (edgeLength: int<latticeDistance>) (lowPoint:latticePoint) (hiPoint:latticePoint) : int =
        let index = getOverIndex lowPoint hiPoint
        %edgeLength * index +  %edgeLength - lowPoint.coords.[index] - 1


