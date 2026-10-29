
namespace GeneSort.Sorting.Sorter

open System
open System.Collections
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

[<Measure>] type sequenceHash
[<Measure>] type reflectiveCount

[<Struct>]
type ce = private { lowChannel: int<channelIndex>; highChannel: int<channelIndex> } with

    static member create (lhs: int) (rhs: int) : ce =
        if lhs < 0 || rhs < 0 then
            failwith "Indices must be non-negative"
        else if lhs < rhs then
            { lowChannel = ChannelIndex.create lhs; highChannel = ChannelIndex.create rhs }
        else
            { lowChannel = ChannelIndex.create rhs; highChannel = ChannelIndex.create lhs }

    /// The lower channel endpoint.
    member this.LowChannel with get () = this.lowChannel

    /// The higher channel endpoint.
    member this.HighChannel with get () = this.highChannel

    member this.ChannelSpan : int<channelCount> = UMX.tag (int this.HighChannel - int this.LowChannel)



module Ce =

    let toString (ce: ce) : string =
        sprintf "(%d, %d)" (int ce.LowChannel) (int ce.HighChannel)

    let fromString (s: string) : ce =
        let parts = s.Trim('(', ')').Split(',')
        if parts.Length <> 2 then
            failwith "Invalid ce string format"
        else
            let lowChannel = Int32.Parse(parts.[0].Trim())
            let highChannel = Int32.Parse(parts.[1].Trim())
            ce.create lowChannel highChannel

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


    // combine the upper and lower arrays, but increase the lowChannel and highChannel indexes of the ce's in cesLower
    // by channelOffset. Verify that ce.LowChannel, and ce.HighChannel in cesUpper are between 0 and (channelCountUpper - 1).
    // Verify that ce.LowChannel, and ce.HighChannel in cesLower are between 0 and (channelCountLower - 1).
    let stack (cesUpper: ce[]) (cesLower: ce[]) 
              (channelOffset: int<channelCount>)  : ce[] =

        // Shift cesLower indices by channelOffset
        let shiftedLower =
            cesLower |> Array.map (fun _ce -> ce.create ((int _ce.LowChannel) + %channelOffset) ((int _ce.HighChannel) + %channelOffset))

        // Combine the arrays
        Array.append cesUpper shiftedLower


    // use stack to combine arrays of ce arrays
    let stack2d (cesUpper: ce[][]) (cesLower: ce[][]) 
                (channelOffset: int<channelCount>) : ce[][] =

        if cesUpper.Length <> cesLower.Length then
            failwith "cesUpper and cesLower must have the same length"
        
        Array.map2 (fun upper lower -> 
            stack upper lower channelOffset
        ) cesUpper cesLower


    let inline private validateDivisibility (width: int<channelCount>) (divisor: int) (funcName: string) =
            let rawWidth = UMX.untag width
            if rawWidth % divisor <> 0 then
                invalidArg (nameof width) $"{funcName} requires mergedChannelCount ({rawWidth}) to be divisible by {divisor}."


    let mergeN (dim: int<mergeDimension>) (mergedChannelCount: int<channelCount>) (cesA: ce[][]) : ce[][] =
            validateDivisibility mergedChannelCount %dim $"merge{dim}"
            let subWidth = mergedChannelCount / %dim
            let mutable current = cesA
            for i in 1 .. (%dim - 1) do
                current <- stack2d current cesA (subWidth * i)
            current


    /// Returns an array of self-reflective CEs covering all remaining unused indices in 0 .. (sWidth - 1).
    /// Each index in 0 .. (sWidth - 1) will be covered exactly once across input 'ces' and returned 'ces'.
    /// Throws an exception if duplicate indices exist or if an unused index's mirror is already occupied.
    let getSelfReflectiveComplement (sWidth: int<channelCount>) (ces: ce[]) : ce[] =
        let rawWidth = %sWidth
        if rawWidth <= 0 then
            invalidArg (nameof sWidth) "Channel count must be positive"

        let occupied = Array.create rawWidth false

        // Mark indices occupied by input CEs and validate input bounds & uniqueness
        for c in ces do
            if (int c.LowChannel) < 0 || (int c.LowChannel) >= rawWidth then
                invalidArg "ces" $"CE index {(int c.LowChannel)} is out of bounds for channel count {rawWidth}"
            if (int c.HighChannel) < 0 || (int c.HighChannel) >= rawWidth then
                invalidArg "ces" $"CE index {(int c.HighChannel)} is out of bounds for channel count {rawWidth}"
            
            if occupied.[(int c.LowChannel)] then
                invalidArg "ces" $"Index {(int c.LowChannel)} is used multiple times in input CEs"
            occupied.[(int c.LowChannel)] <- true

            if (int c.LowChannel) <> (int c.HighChannel) then
                if occupied.[(int c.HighChannel)] then
                    invalidArg "ces" $"Index {(int c.HighChannel)} is used multiple times in input CEs"
                occupied.[(int c.HighChannel)] <- true

        let complement = ResizeArray()

        // Pair remaining unused indices with their reflected counter-parts
        for i in 0 .. (rawWidth - 1) do
            if not occupied.[i] then
                let mirror = rawWidth - 1 - i
                
                if occupied.[mirror] then
                    failwithf "Cannot form self-reflective complement: index %d is available but its mirror index %d is already occupied." i mirror

                let newCe = ce.create i mirror
                complement.Add(newCe)
                
                // Mark both lowChannel and high mirror indices as processed
                occupied.[i] <- true
                occupied.[mirror] <- true

        complement.ToArray()


    let maxIndexForChannelCount (width: int) : int =  width * (width - 1) / 2

    let toIndex (ce: ce) : int =
        let i = (int ce.LowChannel)
        let j = (int ce.HighChannel)
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
            let highChannel = (int pfloor)
            ce.create (lo) (highChannel)


    let permute (perm: permutation) (cer:ce) =
        ce.create (perm.permute (int cer.LowChannel)) (perm.permute (int cer.HighChannel))


    let reflect (channelCount: int<channelCount>) (cer: ce) : ce =
            ce.create 
                ((int cer.HighChannel) |> GeneSort.Core.Combinatorics.reflect %channelCount)
                ((int cer.LowChannel) |>  GeneSort.Core.Combinatorics.reflect %channelCount)


    let isSelfReflection (channelCount: int<channelCount>) (cer: ce) : bool =
            let reflected = reflect channelCount cer
            reflected = cer


    /// Counts the number of CEs in the array that are either self-reflective OR are 
    /// the reflection of at least one CE (including itself) present in the array.
    let countReflectiveOrReflected (channelCount: int<channelCount>) (ces: ce[]) : int<reflectiveCount> =
        if ces.Length = 0 then
            0 |> UMX.tag
        else
            let w = %channelCount
            let maxIdx = maxIndexForChannelCount (w + 1)
        
            // Create bit-presence mask (extremely fast bitwise operations)
            let presence = BitArray(maxIdx + 1, false)
            for i in 0 .. ces.Length - 1 do
                presence.Set(toIndex ces.[i], true)

            let mutable count = 0
            for i in 0 .. ces.Length - 1 do
                let c = ces.[i]
                let reflected = reflect channelCount c
                let reflectedIdx = toIndex reflected
            
                if presence.Get(reflectedIdx) then
                    count <- count + 1

            count |> UMX.tag


    /// Counts the number of CEs in the array that are either self-reflective OR are 
    /// the reflection of at least one CE (including itself) present in the array.
    let countReflectiveOrReflected_Old (channelCount: int<channelCount>) (ces: ce[]) : int<reflectiveCount> =
        if ces.Length = 0 then
            0 |> UMX.tag<reflectiveCount>
        else
            let ceSet = Set.ofArray ces
            ces
            |> Array.filter (fun c ->
                // Check if the CE is self-reflective OR its reflection exists in the array
                isSelfReflection channelCount c || Set.contains (reflect channelCount c) ceSet)
            |> Array.length |> UMX.tag<reflectiveCount>


    let generateCeCode (excludeSelfCe:bool)
                       (width:int) 
                       (indexPicker: int -> int) : int =
        if width < 1 then
            failwith "Width must be at least 1"
        let ww = if excludeSelfCe then width else width + 1
        let indexMax = maxIndexForChannelCount ww
        let dex = indexPicker indexMax
        if excludeSelfCe then
            let ceTemp = fromIndex dex
            ce.create ((int ceTemp.LowChannel)) ((int ceTemp.HighChannel) + 1) |> toIndex
        else
            dex


    /// returns random Ce's to make Sorters, including Ce's where lowChannel=highChannel
    /// <param name="width">The Sorter width</param>
    let generateCeCodes (indexPicker: int -> int) 
                        (width:int) : int seq =
        if width < 1 then
            failwith "Width must be at least 1"
        let indexMax = maxIndexForChannelCount width
        seq {
            while true do
                indexPicker indexMax
        }


    /// returns random Ce's to make Sorters, excluding Ce's where lowChannel=highChannel
    /// <param name="width">The Sorter width</param>
    let generateCeCodesExcludeSelf 
                    (indexPicker: int -> int) 
                    (width:int) : int seq =
        if width < 2 then
            failwith "Width must be at least 2"
        let indexMax = maxIndexForChannelCount width
        seq {
            while true do
                let ceTemp = indexPicker indexMax |> fromIndex
                ce.create ((int ceTemp.LowChannel)) ((int ceTemp.HighChannel) + 1) |> toIndex
        }


    /// returns random Ce's to make Sorters, including Ce's where lowChannel=highChannel
    /// <param name="width">The Sorter width</param>
    let generateCes (indexPicker: int -> int) 
                    (width:int) : ce seq =
        generateCeCodes indexPicker width |> Seq.map(fromIndex)


    /// returns random Ce's to make Sorters, excluding Ce's where lowChannel=highChannel
    /// <param name="width">The Sorter width</param>
    let generateCesExcludeSelf 
                    (indexPicker: int -> int) 
                    (width:int) : ce seq =
        generateCeCodesExcludeSelf indexPicker width |> Seq.map(fromIndex)


    let fromTwoOrbit (toOb:twoOrbit) :ce = 
            ce.create (toOb.First) (toOb.Second)
