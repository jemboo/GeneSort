
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

[<Struct>]
type simdSortBlock = 
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

    member this.Vectors = this.vectors
    member this.Length = this.vectors.Length
    member this.SortableCount = this.sortableCount

    member this.ComputeLaneHashes32() : uint32[] =
        let width = this.vectors.Length
        // We track 32 hashes using four YMM registers (8 lanes each)
        let mutable h0 = Vector256<uint32>.Zero // Lanes 0-7
        let mutable h1 = Vector256<uint32>.Zero // Lanes 8-15
        let mutable h2 = Vector256<uint32>.Zero // Lanes 16-23
        let mutable h3 = Vector256<uint32>.Zero // Lanes 24-31
        
        let multiplier = Vector256.Create(1000003u) // A small prime for mixing

        for i = 0 to width - 1 do
            let v = this.vectors.[i]

            // 1. Promote uint8 lanes to uint32
            // Unpack into 16-bit, then 16-bit into 32-bit
            let resLow = Avx2.UnpackLow(v, Vector256<uint8>.Zero).AsUInt16()
            let resHi = Avx2.UnpackHigh(v, Vector256<uint8>.Zero).AsUInt16()

            let v0 = Avx2.UnpackLow(resLow, Vector256<uint16>.Zero).AsUInt32()
            let v1 = Avx2.UnpackHigh(resLow, Vector256<uint16>.Zero).AsUInt32()
            let v2 = Avx2.UnpackLow(resHi, Vector256<uint16>.Zero).AsUInt32()
            let v3 = Avx2.UnpackHigh(resHi, Vector256<uint16>.Zero).AsUInt32()

            // 2. Mix: h = (h * prime) + currentVal
            // Note: Avx2.MultiplyLow returns the lower 32 bits of the product
            h0 <- Avx2.Add(Avx2.MultiplyLow(h0, multiplier), v0)
            h1 <- Avx2.Add(Avx2.MultiplyLow(h1, multiplier), v1)
            h2 <- Avx2.Add(Avx2.MultiplyLow(h2, multiplier), v2)
            h3 <- Avx2.Add(Avx2.MultiplyLow(h3, multiplier), v3)

        // 3. Extract into a flat array
        let hashes = Array.zeroCreate<uint32> 32
        h0.CopyTo(hashes, 0)
        h1.CopyTo(hashes, 8)
        h2.CopyTo(hashes, 16)
        h3.CopyTo(hashes, 24)
        hashes