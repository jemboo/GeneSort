
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86
open System.Collections.Concurrent

[<Struct>]
type simd256SortBlock = 
    private { 
        vectors: Vector256<uint8>[] 
        sortableCount: int 
    }
    
    /// Creates a SIMD block from a slice of 1-32 sortableIntArrays.
    static member createFromIntArrays (sw: int<sortingWidth>) (arrays: sortableIntArray[]) =
        let width = %sw
        let inputCount = arrays.Length
        if inputCount > 32 then 
            invalidArg "arrays" "A single SIMD block can only hold up to 32 lanes."
        
        // Vertical transposition: arrays[lane][vIdx] -> vectors[vIdx].lane
        let vecs = Array.init width (fun vIdx ->
            let mutable v = Vector256<uint8>.Zero
            for lane = 0 to 31 do
                let value = 
                    if lane < inputCount then 
                        byte arrays.[lane].Values.[vIdx]
                    else 
                        byte vIdx // Golden padding: identity value
                v <- v.WithElement(lane, value)
            v
        )
        { vectors = vecs; sortableCount = inputCount }

    static member createFromVectors (vecs: Vector256<uint8>[]) (count: int) =
            { vectors = vecs; sortableCount = count }

    member this.Vectors = this.vectors
    member this.Length = this.vectors.Length
    member this.SortableCount = this.sortableCount


module Simd256SortBlock = 

    /// Computes 32 independent 32-bit hashes (one per lane) across all vectors in a block.
    let computeLaneHashes32 (vectors: Vector256<uint8>[]) : uint32[] =
        let mutable h0 = Vector256<uint32>.Zero // Lanes 0-7
        let mutable h1 = Vector256<uint32>.Zero // Lanes 8-15
        let mutable h2 = Vector256<uint32>.Zero // Lanes 16-23
        let mutable h3 = Vector256<uint32>.Zero // Lanes 24-31
    
        // Prime multiplier for the hash mix
        let multiplier = Vector256.Create(1000003u)

        for i = 0 to vectors.Length - 1 do
            let v = vectors.[i]

            // --- Widening Step ---
            // Unpack bytes (uint8) to words (uint16)
            let resLow16 = Avx2.UnpackLow(v, Vector256<uint8>.Zero).AsUInt16()
            let resHi16 = Avx2.UnpackHigh(v, Vector256<uint8>.Zero).AsUInt16()

            // Unpack words (uint16) to double-words (uint32)
            let v0 = Avx2.UnpackLow(resLow16, Vector256<uint16>.Zero).AsUInt32()
            let v1 = Avx2.UnpackHigh(resLow16, Vector256<uint16>.Zero).AsUInt32()
            let v2 = Avx2.UnpackLow(resHi16, Vector256<uint16>.Zero).AsUInt32()
            let v3 = Avx2.UnpackHigh(resHi16, Vector256<uint16>.Zero).AsUInt32()

            // --- Mixing Step ---
            // hash = (hash * multiplier) + current_value
            h0 <- Avx2.Add(Avx2.MultiplyLow(h0, multiplier), v0)
            h1 <- Avx2.Add(Avx2.MultiplyLow(h1, multiplier), v1)
            h2 <- Avx2.Add(Avx2.MultiplyLow(h2, multiplier), v2)
            h3 <- Avx2.Add(Avx2.MultiplyLow(h3, multiplier), v3)

        // --- Extraction Step ---
        let result = Array.zeroCreate<uint32> 32
        h0.CopyTo(result, 0)
        h1.CopyTo(result, 8)
        h2.CopyTo(result, 16)
        h3.CopyTo(result, 24)
        result



module Simd256GoldenHashProvider =
    
    // Cache to store the 32-bit fingerprint for each sortingWidth
    // key: int<sortingWidth>, value: uint32[] (32 hashes)
    let private goldenCache = ConcurrentDictionary<int<sortingWidth>, uint32[]>()

    /// Calculates the 32-bit hash fingerprint for a perfectly sorted block.
    let private calculateGolden (width: int<sortingWidth>) : uint32[] =
        let w = %width
        if w > 256 then 
            failwithf "Sorting width %d exceeds uint8 capacity (256) for SIMD golden hashing." w

        // Create a 'fake' block where every lane is [0, 1, 2, ..., w-1]
        let vectors = Array.init w (fun i -> Vector256.Create(byte i))
        
        // We use the createFromVectors we added to simdSortBlock earlier
        let tempBlock = simd256SortBlock.createFromVectors vectors 32
        
        // ComputeLaneHashes32 is the method using UnpackLow/High + MultiplyLow
        Simd256SortBlock.computeLaneHashes32 tempBlock.Vectors

    /// Memoized access to the golden hashes. 
    /// Returns 32 uint32 values representing the 'sorted' state of each lane.
    let GetGoldenHashes (width: int<sortingWidth>) : uint32[] =
        goldenCache.GetOrAdd(width, (fun w -> calculateGolden w))