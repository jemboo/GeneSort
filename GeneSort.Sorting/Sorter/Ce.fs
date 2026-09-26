
namespace GeneSort.Sorting.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

[<Measure>] type sequenceHash
[<Measure>] type reflectiveCount

[<Struct>]
type ce = private { low: int; hi: int } with

    static member create (lhs: int) (rhs: int) : ce =
        if lhs < 0 || rhs < 0 then
            failwith "Indices must be non-negative"
        else if lhs < rhs then
            { low = lhs; hi = rhs }
        else
            { low = rhs; hi = lhs }

    /// Gets the first TwoOrbit.
    member this.Low with get () = this.low

    /// Gets the second TwoOrbit (if present).
    member this.Hi with get () = this.hi

    member this.Length with get() = this.Hi - this.Low



module Ce =

    let toString (ce: ce) : string =
        sprintf "(%d, %d)" ce.Low ce.Hi

    let fromString (s: string) : ce =
        let parts = s.Trim('(', ')').Split(',')
        if parts.Length <> 2 then
            failwith "Invalid ce string format"
        else
            let low = Int32.Parse(parts.[0].Trim())
            let hi = Int32.Parse(parts.[1].Trim())
            ce.create low hi

    let arrayToString (ces: ce[]) : string =
        let ceStrings = ces |> Array.map toString
        sprintf "[%s]" (String.Join("; ", ceStrings))

    let fromArrayString (s: string) : ce[] =
        let trimmed = s.Trim('[', ']')
        if String.IsNullOrWhiteSpace(trimmed) then
            [||]
        else
            let ceStrings = trimmed.Split(';')
            ceStrings |> Array.map (fun ceStr -> fromString (ceStr.Trim()))

    /// Converts a permSi's two-orbits into an array of ce's.
    let fromPermSi (psi: permSi) : ce [] =
        psi
        |> PermSi.getTwoOrbits
        |> Array.map (fun tbit -> ce.create tbit.First tbit.Second)


    // combine the upper and lower arrays, but increase the low and hi indexes of the ce's in cesLower
    // by lowerOffset. Verify that ce.Low, and ce.Hi in cesUpper are between 0 and (sortingWidthUpper - 1).
    // Verify that ce.Low, and ce.Hi in cesLower are between 0 and (sortingWidthLower - 1).
    let stack (cesUpper: ce[]) (cesLower: ce[]) 
              (lowerOffset: int<sortingWidth>)  : ce[] =

        // Shift cesLower indices by lowerOffset
        let shiftedLower =
            cesLower |> Array.map (fun _ce -> ce.create (_ce.Low + %lowerOffset) (_ce.Hi + %lowerOffset))

        // Combine the arrays
        Array.append cesUpper shiftedLower


    // use stack to combine arrays of ce arrays
    let stack2d (cesUpper: ce[][]) (cesLower: ce[][]) 
                (lowerOffset: int<sortingWidth>) : ce[][] =

        if cesUpper.Length <> cesLower.Length then
            failwith "cesUpper and cesLower must have the same length"
        
        Array.map2 (fun upper lower -> 
            stack upper lower lowerOffset
        ) cesUpper cesLower


    let inline private validateDivisibility (width: int<sortingWidth>) (divisor: int) (funcName: string) =
            let rawWidth = UMX.untag width
            if rawWidth % divisor <> 0 then
                invalidArg (nameof width) $"{funcName} requires mergedSortingWidth ({rawWidth}) to be divisible by {divisor}."


    let mergeN (dim: int<mergeDimension>) (mergedSortingWidth: int<sortingWidth>) (cesA: ce[][]) : ce[][] =
            validateDivisibility mergedSortingWidth %dim $"merge{dim}"
            let subWidth = mergedSortingWidth / %dim
            let mutable current = cesA
            for i in 1 .. (%dim - 1) do
                current <- stack2d current cesA (subWidth * i)
            current


    /// Returns an array of self-reflective CEs covering all remaining unused indices in 0 .. (sWidth - 1).
    /// Each index in 0 .. (sWidth - 1) will be covered exactly once across input 'ces' and returned 'ces'.
    /// Throws an exception if duplicate indices exist or if an unused index's mirror is already occupied.
    let getSelfReflectiveComplement (sWidth: int<sortingWidth>) (ces: ce[]) : ce[] =
        let rawWidth = %sWidth
        if rawWidth <= 0 then
            invalidArg (nameof sWidth) "Sorting width must be positive"

        let occupied = Array.create rawWidth false

        // Mark indices occupied by input CEs and validate input bounds & uniqueness
        for c in ces do
            if c.Low < 0 || c.Low >= rawWidth then
                invalidArg "ces" $"CE index {c.Low} is out of bounds for sorting width {rawWidth}"
            if c.Hi < 0 || c.Hi >= rawWidth then
                invalidArg "ces" $"CE index {c.Hi} is out of bounds for sorting width {rawWidth}"
            
            if occupied.[c.Low] then
                invalidArg "ces" $"Index {c.Low} is used multiple times in input CEs"
            occupied.[c.Low] <- true

            if c.Low <> c.Hi then
                if occupied.[c.Hi] then
                    invalidArg "ces" $"Index {c.Hi} is used multiple times in input CEs"
                occupied.[c.Hi] <- true

        let complement = ResizeArray()

        // Pair remaining unused indices with their reflected counter-parts
        for i in 0 .. (rawWidth - 1) do
            if not occupied.[i] then
                let mirror = rawWidth - 1 - i
                
                if occupied.[mirror] then
                    failwithf "Cannot form self-reflective complement: index %d is available but its mirror index %d is already occupied." i mirror

                let newCe = ce.create i mirror
                complement.Add(newCe)
                
                // Mark both low and high mirror indices as processed
                occupied.[i] <- true
                occupied.[mirror] <- true

        complement.ToArray()


    let maxIndexForWdith (width: int) : int =
        width*(width - 1) / 2

    let toIndex (ce: ce) : int =
        let i = ce.Low
        let j = ce.Hi
        (j * (j + 1)) / 2 + i

    let fromIndex (dex:int) : ce = 
        if dex < 0 then
            failwith "Index must be non-negative"
        else
        let indexFlt = (dex |> float) + 1.0
        let p = (sqrt (1.0 + 8.0 * indexFlt) - 1.0) / 2.0
        let pfloor = int p
        if (p = pfloor) then 
            ce.create (pfloor - 1) (pfloor - 1)
        else
            let lo = (float dex) - (float (pfloor * (pfloor + 1))) / 2.0 |> int
            let hi = (int pfloor)
            ce.create (lo) (hi)


    let permute (perm: permutation) (cer:ce) =
        ce.create (perm.permute cer.Low) (perm.permute cer.Hi)


    let reflect (sortingWidth: int<sortingWidth>) (cer: ce) : ce =
            ce.create 
                (cer.Hi |> GeneSort.Core.Combinatorics.reflect %sortingWidth) 
                (cer.Low |>  GeneSort.Core.Combinatorics.reflect %sortingWidth)


    let isSelfReflection (sortingWidth: int<sortingWidth>) (cer: ce) : bool =
            let reflected = reflect sortingWidth cer
            reflected = cer


    /// Counts the number of CEs in the array that are either self-reflective OR are 
    /// the reflection of at least one CE (including itself) present in the array.
    let countReflectiveOrReflected (sortingWidth: int<sortingWidth>) (ces: ce[]) : int<reflectiveCount> =
        if ces.Length = 0 then
            0 |> UMX.tag<reflectiveCount>
        else
            let ceSet = Set.ofArray ces
            ces
            |> Array.filter (fun c ->
                // Check if the CE is self-reflective OR its reflection exists in the array
                isSelfReflection sortingWidth c || Set.contains (reflect sortingWidth c) ceSet)
            |> Array.length |> UMX.tag<reflectiveCount>


    let generateCeCode (excludeSelfCe:bool)
                       (width:int) 
                       (indexPicker: int -> int) : int =
        if width < 1 then
            failwith "Width must be at least 1"
        let ww = if excludeSelfCe then width else width + 1
        let indexMax = maxIndexForWdith ww
        let dex = indexPicker indexMax
        if excludeSelfCe then
            let ceTemp = fromIndex dex
            ce.create (ceTemp.Low) (ceTemp.Hi + 1) |> toIndex
        else
            dex


    /// returns random Ce's to make Sorters, including Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCeCodes (indexPicker: int -> int) 
                        (width:int) : int seq =
        if width < 1 then
            failwith "Width must be at least 1"
        let indexMax = maxIndexForWdith width
        seq {
            while true do
                indexPicker indexMax
        }


    /// returns random Ce's to make Sorters, excluding Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCeCodesExcludeSelf 
                    (indexPicker: int -> int) 
                    (width:int) : int seq =
        if width < 2 then
            failwith "Width must be at least 2"
        let indexMax = maxIndexForWdith width
        seq {
            while true do
                let ceTemp = indexPicker indexMax |> fromIndex
                ce.create (ceTemp.Low) (ceTemp.Hi + 1) |> toIndex
        }


    /// returns random Ce's to make Sorters, including Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCes (indexPicker: int -> int) 
                    (width:int) : ce seq =
        generateCeCodes indexPicker width |> Seq.map(fromIndex)


    /// returns random Ce's to make Sorters, excluding Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCesExcludeSelf 
                    (indexPicker: int -> int) 
                    (width:int) : ce seq =
        generateCeCodesExcludeSelf indexPicker width |> Seq.map(fromIndex)


    let fromTwoOrbit (toOb:twoOrbit) :ce = 
            ce.create (toOb.First) (toOb.Second)
