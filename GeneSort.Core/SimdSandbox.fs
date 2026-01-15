namespace GeneSort.Core
open System
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86
open System.Runtime.Intrinsics.Arm
open System.Runtime.CompilerServices
open Microsoft.FSharp.NativeInterop

open ArrayUtils


[<Measure>] type simdLength



//let a = 10y         // int8
//let a = 10uy        // uint8
//let a = 10          // int
//let b = 10u         // uint32
//let c = 10L         // int64
//let d = 10UL        // uint64
//let e = 3.14f       // float32
//let f = 3.14m       // decimal
//let g = 10I         // bigint


module SimdUtils = 

    let printCapabilities () =
        printfn "=== CPU SIMD Capabilities ==="
        printfn "SSE2:    %b (128-bit)" Sse2.IsSupported
        printfn "AVX2:    %b (256-bit)" Avx2.IsSupported
        printfn "AVX-512: %b (512-bit)" Avx512F.IsSupported
        printfn "ARM NEON: %b (128-bit)" AdvSimd.IsSupported
        printfn "Vector512 IsHardwareAccelerated: %b" Vector512.IsHardwareAccelerated
        printfn "Vector256 IsHardwareAccelerated: %b" Vector256.IsHardwareAccelerated
        printfn ""


    module V512 =

        let inline Create<'T when 'T : struct> (data: 'T[]) : Vector512<'T> =
            if data.Length <> Vector512<'T>.Count then
                raise (ArgumentException(sprintf "Input array must have exactly %d elements." Vector512<'T>.Count))
            Vector512.Create<'T>(data)

        let inline tile512<'T when 'T : struct>
            (data: Span<'T>) : Vector512<'T>[] =

            let vectorSize = Vector512<'T>.Count
            let totalVectors = (data.Length + vectorSize - 1) / vectorSize

            let result = Array.zeroCreate<Vector512<'T>> totalVectors

            // scratch buffer for the final (partial) vector only
            let scratch = Array.zeroCreate<'T> vectorSize
            let scratchSpan = scratch.AsSpan()

            for i = 0 to totalVectors - 1 do
                let start = i * vectorSize
                let remaining = data.Length - start

                if remaining >= vectorSize then
                    // fast path: full vector, no copy
                    let slice = data.Slice(start, vectorSize)
                    result.[i] <- Vector512.Create<'T>(slice)
                else
                    // tail: copy once into zeroed scratch
                    scratchSpan.Clear()
                    data.Slice(start, remaining).CopyTo(scratchSpan)
                    result.[i] <- Vector512.Create<'T>(scratch)

            result


        /// data: [vectorIndex][i][j]
        let packToVector512<'T when 'T : struct>
            (data: 'T[][][]) : Vector512<'T>[][] =

            let lanes = data.Length
            let vectorSize = Vector512<'T>.Count

            if lanes <> vectorSize then
                invalidArg "data"
                    (sprintf "First dimension must be %d for Vector512<%s>"
                        vectorSize typeof<'T>.Name)

            let a = if lanes > 0 then data.[0].Length else 0
            let b = if a > 0 then data.[0].[0].Length else 0

            Array.init a (fun i ->
                Array.init b (fun j ->
                    let values = Array.init vectorSize (fun idx ->
                        data.[idx].[i].[j])
                    Vector512.Create<'T>(values)
                )
            )


       /// Multiply each vector by a scalar and add offset
        let inline multiplyAdd (data: Span<Vector512<uint16>>) (multiplier: uint16) (offset: uint16) =
            let mult = Vector512.Create(multiplier)
            let off = Vector512.Create(offset)
            for i = 0 to data.Length - 1 do
                data.[i] <- Vector512.Add(Vector512.Multiply(data.[i], mult), off)
        
        /// Bitwise operations: XOR with pattern
        let inline xorPattern (data: Span<Vector512<uint16>>) (pattern: uint16) =
            let pat = Vector512.Create(pattern)
            for i = 0 to data.Length - 1 do
                data.[i] <- Vector512.Xor(data.[i], pat)
        
        /// Min/Max clamping
        let inline clamp (data: Span<Vector512<uint16>>) (minVal: uint16) (maxVal: uint16) =
            let minVec = Vector512.Create(minVal)
            let maxVec = Vector512.Create(maxVal)
            for i = 0 to data.Length - 1 do
                data.[i] <- Vector512.Min(Vector512.Max(data.[i], minVec), maxVec)
        
        /// Shift operations
        let inline shiftAndAdd (data: Span<Vector512<uint16>>) (shiftAmount: int) =
            for i = 0 to data.Length - 1 do
                let shifted = Vector512.ShiftRightLogical(data.[i], shiftAmount)
                data.[i] <- Vector512.Add(data.[i], shifted)
        
        /// Complex pipeline: multiply, clamp, xor
        let inline complexPipeline (data: Span<Vector512<uint16>>) =
            let mult = Vector512.Create(3us)
            let minVec = Vector512.Create(100us)
            let maxVec = Vector512.Create(60000us)
            let xorPat = Vector512.Create(0xAAAAus)
            
            for i = 0 to data.Length - 1 do
                // Multiply by 3
                let temp = Vector512.Multiply(data.[i], mult)
                // Clamp
                let temp2 = Vector512.Min(Vector512.Max(temp, minVec), maxVec)
                // XOR
                data.[i] <- Vector512.Xor(temp2, xorPat)




    /// SIMD operations on Vector512<uint16>
    module SimdOps =
        /// Multiply each vector by a scalar and add offset
        let inline multiplyAdd (data: Span<Vector512<uint16>>) (multiplier: uint16) (offset: uint16) =
            let mult = Vector512.Create(multiplier)
            let off = Vector512.Create(offset)
            for i = 0 to data.Length - 1 do
                data.[i] <- Vector512.Add(Vector512.Multiply(data.[i], mult), off)
        
        /// Bitwise operations: XOR with pattern
        let inline xorPattern (data: Span<Vector512<uint16>>) (pattern: uint16) =
            let pat = Vector512.Create(pattern)
            for i = 0 to data.Length - 1 do
                data.[i] <- Vector512.Xor(data.[i], pat)
        
        /// Min/Max clamping
        let inline clamp (data: Span<Vector512<uint16>>) (minVal: uint16) (maxVal: uint16) =
            let minVec = Vector512.Create(minVal)
            let maxVec = Vector512.Create(maxVal)
            for i = 0 to data.Length - 1 do
                data.[i] <- Vector512.Min(Vector512.Max(data.[i], minVec), maxVec)
        
        /// Shift operations
        let inline shiftAndAdd (data: Span<Vector512<uint16>>) (shiftAmount: int) =
            for i = 0 to data.Length - 1 do
                let shifted = Vector512.ShiftRightLogical(data.[i], shiftAmount)
                data.[i] <- Vector512.Add(data.[i], shifted)
        
        /// Complex pipeline: multiply, clamp, xor
        let inline complexPipeline (data: Span<Vector512<uint16>>) =
            let mult = Vector512.Create(3us)
            let minVec = Vector512.Create(100us)
            let maxVec = Vector512.Create(60000us)
            let xorPat = Vector512.Create(0xAAAAus)
            
            for i = 0 to data.Length - 1 do
                // Multiply by 3
                let temp = Vector512.Multiply(data.[i], mult)
                // Clamp
                let temp2 = Vector512.Min(Vector512.Max(temp, minVec), maxVec)
                // XOR
                data.[i] <- Vector512.Xor(temp2, xorPat)



    module SimdOps512 =

        /// Multiply each vector by a scalar and add offset
        let inline multiplyAdd<'T when 'T : struct>
            (data: Span<Vector512<'T>>)
            (multiplier: 'T)
            (offset: 'T) =

            let mult = Vector512.Create(multiplier)
            let off  = Vector512.Create(offset)

            for i = 0 to data.Length - 1 do
                data.[i] <-
                    Vector512.Add(
                        Vector512.Multiply(data.[i], mult),
                        off
                    )


        /// Bitwise XOR with a scalar pattern
        /// Valid for integral SIMD element types
        let inline xorPattern<'T when 'T : struct>
            (data: Span<Vector512<'T>>)
            (pattern: 'T) =

            let pat = Vector512.Create(pattern)

            for i = 0 to data.Length - 1 do
                data.[i] <- Vector512.Xor(data.[i], pat)

        /// Clamp values to [minVal, maxVal]
        /// Valid for numeric SIMD element types
        let inline clamp<'T when 'T : struct>
            (data: Span<Vector512<'T>>)
            (minVal: 'T)
            (maxVal: 'T) =

            let minVec = Vector512.Create(minVal)
            let maxVec = Vector512.Create(maxVal)

            for i = 0 to data.Length - 1 do
                data.[i] <-
                    Vector512.Min(
                        Vector512.Max(data.[i], minVec),
                        maxVec
                    )

        ///// Shift right logically and add original value
        let inline shiftAndAdd_u8
            (data: Span<Vector512<uint8>>)
            (shiftAmount: int) =

            for i = 0 to data.Length - 1 do
                let shifted =
                    Vector512.ShiftRightLogical(data.[i], shiftAmount)
                data.[i] <- Vector512.Add(data.[i], shifted)

        let inline shiftAndAdd_u16
            (data: Span<Vector512<uint16>>)
            (shiftAmount: int) =

            for i = 0 to data.Length - 1 do
                let shifted =
                    Vector512.ShiftRightLogical(data.[i], shiftAmount)
                data.[i] <- Vector512.Add(data.[i], shifted)


        /// Complex pipeline: multiply, clamp, xor
        /// Valid for integral SIMD element types
        let inline complexPipeline<'T when 'T : struct>
            (data: Span<Vector512<'T>>)
            (multiplier: 'T)
            (minVal: 'T)
            (maxVal: 'T)
            (xorPattern: 'T) =

            let mult   = Vector512.Create(multiplier)
            let minVec = Vector512.Create(minVal)
            let maxVec = Vector512.Create(maxVal)
            let xorVec = Vector512.Create(xorPattern)

            for i = 0 to data.Length - 1 do
                let t1 = Vector512.Multiply(data.[i], mult)
                let t2 = Vector512.Min(Vector512.Max(t1, minVec), maxVec)
                data.[i] <- Vector512.Xor(t2, xorVec)





    module SimdOps256 =

        /// Multiply each vector by a scalar and add offset
        let inline multiplyAdd<'T when 'T : struct>
            (data: Span<Vector256<'T>>)
            (multiplier: 'T)
            (offset: 'T) =

            let mult = Vector256.Create(multiplier)
            let off  = Vector256.Create(offset)

            for i = 0 to data.Length - 1 do
                data.[i] <-
                    Vector256.Add(
                        Vector256.Multiply(data.[i], mult),
                        off
                    )


        /// Bitwise XOR with a scalar pattern
        /// Valid for integral SIMD element types
        let inline xorPattern<'T when 'T : struct>
            (data: Span<Vector256<'T>>)
            (pattern: 'T) =

            let pat = Vector256.Create(pattern)

            for i = 0 to data.Length - 1 do
                data.[i] <- Vector256.Xor(data.[i], pat)

        /// Clamp values to [minVal, maxVal]
        /// Valid for numeric SIMD element types
        let inline clamp<'T when 'T : struct>
            (data: Span<Vector256<'T>>)
            (minVal: 'T)
            (maxVal: 'T) =

            let minVec = Vector256.Create(minVal)
            let maxVec = Vector256.Create(maxVal)

            for i = 0 to data.Length - 1 do
                data.[i] <-
                    Vector256.Min(
                        Vector256.Max(data.[i], minVec),
                        maxVec
                    )

        ///// Shift right logically and add original value
        let inline shiftAndAdd_u8
            (data: Span<Vector256<uint8>>)
            (shiftAmount: int) =

            for i = 0 to data.Length - 1 do
                let shifted =
                    Vector256.ShiftRightLogical(data.[i], shiftAmount)
                data.[i] <- Vector256.Add(data.[i], shifted)

        let inline shiftAndAdd_u16
            (data: Span<Vector256<uint16>>)
            (shiftAmount: int) =

            for i = 0 to data.Length - 1 do
                let shifted =
                    Vector256.ShiftRightLogical(data.[i], shiftAmount)
                data.[i] <- Vector256.Add(data.[i], shifted)


        /// Complex pipeline: multiply, clamp, xor
        /// Valid for integral SIMD element types
        let inline complexPipeline<'T when 'T : struct>
            (data: Span<Vector256<'T>>)
            (multiplier: 'T)
            (minVal: 'T)
            (maxVal: 'T)
            (xorPattern: 'T) =

            let mult   = Vector256.Create(multiplier)
            let minVec = Vector256.Create(minVal)
            let maxVec = Vector256.Create(maxVal)
            let xorVec = Vector256.Create(xorPattern)

            for i = 0 to data.Length - 1 do
                let t1 = Vector256.Multiply(data.[i], mult)
                let t2 = Vector256.Min(Vector256.Max(t1, minVec), maxVec)
                data.[i] <- Vector256.Xor(t2, xorVec)





    let testStackTileByK () =
        printfn "=== Testing stackTileByK ==="
    
        // Test 1: Simple small example
        printfn "\nTest 1: 5x3 array with k=2"
        let test1 = [|
            [| 1; 2; 3 |]
            [| 4; 5; 6 |]
            [| 7; 8; 9 |]
            [| 10; 11; 12 |]
            [| 13; 14; 15 |]
        |]
        let result1 = stackTileByK test1 2
        8




    let yab () =

        let daterByte = [| 0 .. 999|] |> Array.map uint8
        let quab = V512.tile512 (daterByte.AsSpan())
        1


module Fused256 =


    /// dst[i] = src[i] * multiplier + offset
    let inline multiplyAddCopy
        (src: ReadOnlySpan<Vector256<uint16>>)
        (dst: Span<Vector256<uint16>>)
        (multiplier: uint16)
        (offset: uint16) =

        let mult = Vector256.Create(multiplier)
        let off  = Vector256.Create(offset)

        let len = src.Length
        for i = 0 to len - 1 do
            let v = src.[i]
            let r = Vector256.Add(Vector256.Multiply(v, mult), off)
            dst.[i] <- r



module Fused512 =

    /// dst[i] = src[i] * multiplier + offset
    let inline multiplyAddCopy
        (src: ReadOnlySpan<Vector512<uint16>>)
        (dst: Span<Vector512<uint16>>)
        (multiplier: uint16)
        (offset: uint16) =

        let mult = Vector512.Create(multiplier)
        let off  = Vector512.Create(offset)

        let len = src.Length
        for i = 0 to len - 1 do
            let v = src.[i]
            let r = Vector512.Add(Vector512.Multiply(v, mult), off)
            dst.[i] <- r
