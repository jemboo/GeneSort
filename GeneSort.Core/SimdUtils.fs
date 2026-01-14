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




