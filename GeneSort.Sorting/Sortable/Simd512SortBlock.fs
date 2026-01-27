
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86
open System.Collections.Concurrent

[<Struct>]
type simd512SortBlock = 
    private { 
        vectors: Vector512<uint8>[] 
        sortableCount: int 
    }
    
    static member createFromIntArrays (sw: int<sortingWidth>) (arrays: sortableIntArray[]) =
        let width = %sw
        let inputCount = arrays.Length
        if inputCount > 64 then 
            invalidArg "arrays" "A single Vector512 block can only hold up to 64 lanes."
        
        let vecs = Array.init width (fun vIdx ->
            let mutable v = Vector512<uint8>.Zero
            for lane = 0 to 63 do
                let value = 
                    if lane < inputCount then byte arrays.[lane].Values.[vIdx]
                    else byte vIdx // Identity padding
                v <- v.WithElement(lane, value)
            v
        )
        { vectors = vecs; sortableCount = inputCount }

    static member createFromVectors (vecs: Vector512<uint8>[]) (count: int) =
            { vectors = vecs; sortableCount = count }

    member this.Vectors = this.vectors
    member this.Length = this.vectors.Length
    member this.SortableCount = this.sortableCount


module Simd512SortBlock = 

    /// Computes 64 independent 32-bit hashes across all vectors in a block.
    let computeLaneHashes64 (vectors: Vector512<uint8>[]) : uint32[] =
        // 8 registers to hold 64 lanes (8 uint32 elements per Vector512<uint32>)
        // Wait! Vector512<uint32> actually holds 16 elements. 
        // So we only need 4 registers to hold 64 hashes.
        let mutable h0 = Vector512<uint32>.Zero // Lanes 0-15
        let mutable h1 = Vector512<uint32>.Zero // Lanes 16-31
        let mutable h2 = Vector512<uint32>.Zero // Lanes 32-47
        let mutable h3 = Vector512<uint32>.Zero // Lanes 48-63
        
        let multiplier = Vector512.Create(1000003u)

        for i = 0 to vectors.Length - 1 do
            let v = vectors.[i]

            // Step 1: uint8 -> uint16 (2 vectors of 32 elements)
            let w16Low = Vector512.WidenLower(v)
            let w16High = Vector512.WidenUpper(v)

            // Step 2: uint16 -> uint32 (4 vectors of 16 elements)
            let v0 = Vector512.WidenLower(w16Low)
            let v1 = Vector512.WidenUpper(w16Low)
            let v2 = Vector512.WidenLower(w16High)
            let v3 = Vector512.WidenUpper(w16High)

            // Mixing Step
            h0 <- Vector512.Add(Vector512.Multiply(h0, multiplier), v0)
            h1 <- Vector512.Add(Vector512.Multiply(h1, multiplier), v1)
            h2 <- Vector512.Add(Vector512.Multiply(h2, multiplier), v2)
            h3 <- Vector512.Add(Vector512.Multiply(h3, multiplier), v3)

        let result = Array.zeroCreate<uint32> 64
        h0.CopyTo(result, 0)
        h1.CopyTo(result, 16)
        h2.CopyTo(result, 32)
        h3.CopyTo(result, 48)
        result


module Simd512GoldenHashProvider =
    
    let private goldenCache = ConcurrentDictionary<int<sortingWidth>, uint32[]>()

    let private calculateGolden (width: int<sortingWidth>) : uint32[] =
        let w = %width
        let vectors = Array.init w (fun i -> Vector512.Create(byte i))
        let tempBlock = simd512SortBlock.createFromVectors vectors 64
        Simd512SortBlock.computeLaneHashes64 tempBlock.Vectors

    let GetGoldenHashes (width: int<sortingWidth>) : uint32[] =
        goldenCache.GetOrAdd(width, (fun w -> calculateGolden w))