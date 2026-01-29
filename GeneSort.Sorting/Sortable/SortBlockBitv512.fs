namespace GeneSort.Sorting.Sortable

open FSharp.UMX
open GeneSort.Sorting
open System.Runtime.Intrinsics

[<Struct>]
type sortBlockBitv512 = 
    private { 
        vectors: Vector512<uint64>[] 
        sortableCount: int 
    }
    
    /// Creates a SIMD block from a slice of 1-512 sortableIntArrays.
    /// Maps each input array to a specific bit across the Vector512 wires.
    static member createFromIntArrays (sw: int<sortingWidth>) (arrays: sortableIntArray[]) =
        let width = %sw
        let inputCount = arrays.Length
        if inputCount > 512 then 
            invalidArg "arrays" "A single SIMD block can only hold up to 512 lanes (bits)."
        
        // We need 'width' vectors. Each vector represents one wire across 512 cases.
        // We use a temporary buffer to build the bits before creating the Vector512.
        let vecs = Array.init width (fun wireIdx ->
            let buffer = Array.zeroCreate<uint64> 8 // 8 uint64s = 512 bits
            
            for testIdx = 0 to inputCount - 1 do
                // If value is non-zero, it's a '1' in 0-1 principle
                if arrays.[testIdx].Values.[wireIdx] <> 0 then
                    let lane = testIdx / 64
                    let bit = testIdx % 64
                    buffer.[lane] <- buffer.[lane] ||| (1uL <<< bit)
            
            // Create the Vector512 from the 8 uint64s
            Vector512.Create(
                buffer.[0], buffer.[1], buffer.[2], buffer.[3],
                buffer.[4], buffer.[5], buffer.[6], buffer.[7]
            )
        )

        { vectors = vecs; sortableCount = inputCount }

    static member createFromVectors (vecs: Vector512<uint64>[]) (count: int) =
            { vectors = vecs; sortableCount = count }

    member this.Vectors = this.vectors
    member this.Length = this.vectors.Length
    member this.SortableCount = this.sortableCount


module SortBlockBitv512 = 

// Lowers the sortable count to newCount by truncating excess lanes.
    let truncate (sb: sortBlockBitv512) (newCount: int) : sortBlockBitv512 =
        if newCount >= sb.SortableCount then
            sb // No change needed if count is higher or equal
        elif newCount <= 0 then
            // Return empty block
            let emptyVecs = sb.Vectors |> Array.map (fun _ -> Vector512<uint64>.Zero)
            sortBlockBitv512.createFromVectors emptyVecs 0
        else
            let laneIndex = newCount / 64
            let bitOffset = newCount % 64
            
            // Create a mask for the partially filled lane
            // Bits below 'bitOffset' are kept (1), bits at or above are cleared (0)
            let laneMask = (1uL <<< bitOffset) - 1uL

            let truncatedVectors = sb.Vectors |> Array.map (fun v ->
                // Copy the 8 uint64 lanes to a buffer to mutate
                let mutable buffer = Array.zeroCreate<uint64> 8
                v.CopyTo(System.Span<uint64>(buffer))
                
                // 1. Mask the specific lane where the truncation occurs
                if laneIndex < 8 then
                    buffer.[laneIndex] <- buffer.[laneIndex] &&& laneMask
                
                // 2. Zero out any lanes that follow
                for i = laneIndex + 1 to 7 do
                    buffer.[i] <- 0uL

                Vector512.Create(
                    buffer.[0], buffer.[1], buffer.[2], buffer.[3],
                    buffer.[4], buffer.[5], buffer.[6], buffer.[7]
                )
            )

            sortBlockBitv512.createFromVectors truncatedVectors newCount


    let toSortableIntArrays (s512: sortBlockBitv512) : sortableIntArray[] =
        let sw = s512.Length |> UMX.tag<sortingWidth>
        let sss = 2 |> UMX.tag<symbolSetSize>
        let result = Array.zeroCreate<sortableIntArray> s512.sortableCount

        // Extract raw bit-data. Copying 'v' to 'localV' prevents FS0406 
        // if sortBlockBitv512 is a struct.
        let rawData = s512.Vectors |> Array.map (fun v ->
            let buf = Array.zeroCreate<uint64> 8
            let localV = v 
            localV.CopyTo(System.Span<uint64>(buf))
            buf)

        for testIdx = 0 to s512.sortableCount - 1 do
            let lane = testIdx / 64
            let bitShift = testIdx % 64
            let mask = 1uL <<< bitShift
    
            let values = Array.zeroCreate<int> %sw
            for wireIdx = 0 to %sw - 1 do
                if (rawData.[wireIdx].[lane] &&& mask) <> 0uL then
                    values.[wireIdx] <- 1
                else
                    values.[wireIdx] <- 0
    
            result.[testIdx] <- sortableIntArray.create(values, sw, sss)
        result


/// Returns true if all packed 0-1 sequences are sorted.
    /// This is orders of magnitude faster than transposing to IntArrays.
    let isAllSorted (sb: sortBlockBitv512) : bool =
        let vecs = sb.Vectors
        let width = vecs.Length
        let mutable anyUnsorted = Vector512<uint64>.Zero
        
        // A sequence is unsorted if a '1' appears at index i 
        // while a '0' appears at index i + 1.
        for i = 0 to width - 2 do
            // Find bits where Low is 1 and High is 0
            let inversions = Vector512.AndNot(vecs.[i], vecs.[i+1])
            anyUnsorted <- anyUnsorted ||| inversions
            
        // If anyUnsorted is Zero, every bit in every lane is sorted.
        anyUnsorted = Vector512<uint64>.Zero


    /// Returns the indices of specific test cases that failed (0-511).
    let getFailedIndices (sb: sortBlockBitv512) : int[] =
        let vecs = sb.Vectors
        let width = vecs.Length
        let mutable combinedInversions = Vector512<uint64>.Zero
        
        for i = 0 to width - 2 do
            combinedInversions <- combinedInversions ||| Vector512.AndNot(vecs.[i], vecs.[i+1])
            
        // Extract the failed bit indices
        let mutable failures = []
        let buf = Array.zeroCreate<uint64> 8
        combinedInversions.CopyTo(System.Span<uint64>(buf))
        
        for lane = 0 to 7 do
            if buf.[lane] <> 0uL then
                for bit = 0 to 63 do
                    if (buf.[lane] &&& (1uL <<< bit)) <> 0uL then
                        failures <- (lane * 64 + bit) :: failures
        
        failures |> List.filter (fun i -> i < sb.SortableCount) |> List.toArray