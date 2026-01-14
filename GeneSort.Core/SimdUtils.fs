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

        let tile512uy (data: uint8[]) : Vector512<uint8>[] =
            let vectorSize = Vector512<uint8>.Count
            let totalVectors = (data.Length + vectorSize - 1) / vectorSize
            Array.init totalVectors (fun i ->
                let startIdx = i * vectorSize
                let length = Math.Min(vectorSize, data.Length - startIdx)
                let segment = Array.zeroCreate<uint8> vectorSize
                Array.Copy(data, startIdx, segment, 0, length)
                Vector512.Create<uint8>(segment)
            )


    let tile256uy (data: uint8[]) : Vector256<uint8>[] =
        let vectorSize = Vector256<uint8>.Count
        let totalVectors = (data.Length + vectorSize - 1) / vectorSize
        Array.init totalVectors (fun i ->
            let startIdx = i * vectorSize
            let length = Math.Min(vectorSize, data.Length - startIdx)
            let segment = Array.zeroCreate<uint8> vectorSize
            Array.Copy(data, startIdx, segment, 0, length)
            Vector256.Create<uint8>(segment)
        )

    let tile512us (data: uint16[]) : Vector512<uint16>[] =
        let vectorSize = Vector512<uint16>.Count
        let totalVectors = (data.Length + vectorSize - 1) / vectorSize
        Array.init totalVectors (fun i ->
            let startIdx = i * vectorSize
            let length = Math.Min(vectorSize, data.Length - startIdx)
            let segment = Array.zeroCreate<uint16> vectorSize
            Array.Copy(data, startIdx, segment, 0, length)
            Vector512.Create<uint16>(segment)
        )


    let tile256us (data: uint16[]) : Vector256<uint16>[] =
        let vectorSize = Vector256<uint16>.Count
        let totalVectors = (data.Length + vectorSize - 1) / vectorSize
        Array.init totalVectors (fun i ->
            let startIdx = i * vectorSize
            let length = Math.Min(vectorSize, data.Length - startIdx)
            let segment = Array.zeroCreate<uint16> vectorSize
            Array.Copy(data, startIdx, segment, 0, length)
            Vector256.Create<uint16>(segment)
        )


    // Vector256<uint16>: 16 elements
    let packToVector256_uint16 (data: uint16[][][]) : Vector256<uint16>[][] =
        let k = data.Length
        if k <> 16 then
            failwith "First dimension must be 16 for Vector256<uint16>"
    
        let a = if k > 0 then data.[0].Length else 0
        let b = if a > 0 then data.[0].[0].Length else 0
    
        Array.init a (fun i ->
            Array.init b (fun j ->
                let values = Array.init 16 (fun idx -> data.[idx].[i].[j])
                Vector256.Create(
                    values.[0], values.[1], values.[2], values.[3],
                    values.[4], values.[5], values.[6], values.[7],
                    values.[8], values.[9], values.[10], values.[11],
                    values.[12], values.[13], values.[14], values.[15]
                )
            )
        )

    // Vector256<uint8>: 32 elements
    let packToVector256_uint8 (data: uint8[][][]) : Vector256<uint8>[][] =
        let k = data.Length
        if k <> 32 then
            failwith "First dimension must be 32 for Vector256<uint8>"
    
        let a = if k > 0 then data.[0].Length else 0
        let b = if a > 0 then data.[0].[0].Length else 0
    
        Array.init a (fun i ->
            Array.init b (fun j ->
                let values = Array.init 32 (fun idx -> data.[idx].[i].[j])
                Vector256.Create(
                    values.[0], values.[1], values.[2], values.[3],
                    values.[4], values.[5], values.[6], values.[7],
                    values.[8], values.[9], values.[10], values.[11],
                    values.[12], values.[13], values.[14], values.[15],
                    values.[16], values.[17], values.[18], values.[19],
                    values.[20], values.[21], values.[22], values.[23],
                    values.[24], values.[25], values.[26], values.[27],
                    values.[28], values.[29], values.[30], values.[31]
                )
            )
        )

    // Vector512<uint8>: 64 elements
    let packToVector512_uint8 (data: uint8[][][]) : Vector512<uint8>[][] =
        let k = data.Length
        if k <> 64 then
            failwith "First dimension must be 64 for Vector512<uint8>"
    
        let a = if k > 0 then data.[0].Length else 0
        let b = if a > 0 then data.[0].[0].Length else 0
    
        Array.init a (fun i ->
            Array.init b (fun j ->
                let values = Array.init 64 (fun idx -> data.[idx].[i].[j])
                Vector512.Create(
                    values.[0], values.[1], values.[2], values.[3],
                    values.[4], values.[5], values.[6], values.[7],
                    values.[8], values.[9], values.[10], values.[11],
                    values.[12], values.[13], values.[14], values.[15],
                    values.[16], values.[17], values.[18], values.[19],
                    values.[20], values.[21], values.[22], values.[23],
                    values.[24], values.[25], values.[26], values.[27],
                    values.[28], values.[29], values.[30], values.[31],
                    values.[32], values.[33], values.[34], values.[35],
                    values.[36], values.[37], values.[38], values.[39],
                    values.[40], values.[41], values.[42], values.[43],
                    values.[44], values.[45], values.[46], values.[47],
                    values.[48], values.[49], values.[50], values.[51],
                    values.[52], values.[53], values.[54], values.[55],
                    values.[56], values.[57], values.[58], values.[59],
                    values.[60], values.[61], values.[62], values.[63]
                )
            )
        )

    // Vector512<uint16>: 32 elements
    let packToVector512_uint16 (data: uint16[][][]) : Vector512<uint16>[][] =
        let k = data.Length
        if k <> 32 then
            failwith "First dimension must be 32 for Vector512<uint16>"
    
        let a = if k > 0 then data.[0].Length else 0
        let b = if a > 0 then data.[0].[0].Length else 0
    
        Array.init a (fun i ->
            Array.init b (fun j ->
                let values = Array.init 32 (fun idx -> data.[idx].[i].[j])
                Vector512.Create(
                    values.[0], values.[1], values.[2], values.[3],
                    values.[4], values.[5], values.[6], values.[7],
                    values.[8], values.[9], values.[10], values.[11],
                    values.[12], values.[13], values.[14], values.[15],
                    values.[16], values.[17], values.[18], values.[19],
                    values.[20], values.[21], values.[22], values.[23],
                    values.[24], values.[25], values.[26], values.[27],
                    values.[28], values.[29], values.[30], values.[31]
                )
            )
        )


    let copyArray512uy (source: Vector512<uint8>[]) =
        let dest = Array.zeroCreate source.Length
        Array.blit source 0 dest 0 source.Length
        dest


    let copyWithSpan512uy (source: Vector512<uint8>[]) =
        let dest = Array.zeroCreate source.Length
        source.AsSpan().CopyTo(dest.AsSpan())
        dest


    let fastUnsafeCopy512uy (source: Vector512<uint8>[]) =
        let dest = Array.zeroCreate source.Length
        let byteCount = uint32 (source.Length * 64) // Vector512 is 64 bytes
    
        let srcPtr = &&source.[0] |> NativePtr.toVoidPtr
        let destPtr = &&dest.[0] |> NativePtr.toVoidPtr
    
        Unsafe.CopyBlock(destPtr, srcPtr, byteCount)
        dest


    // You must use the 'unmanaged' constraint for NativePtr operations
    let fastGenericCopy0 (source: 'T[]) : 'T[] when 'T : unmanaged =
        if source.Length = 0 then [||]
        else
            let dest = System.GC.AllocateUninitializedArray<'T>(source.Length)
            let byteCount = uint32 (source.Length * Unsafe.SizeOf<'T>())
        
            // Pin the arrays
            use pSrc = fixed source
            use pDest = fixed dest
        
            // Convert nativeptr<'T> to void* 
            let srcVoidPtr = NativePtr.toVoidPtr pSrc
            let destVoidPtr = NativePtr.toVoidPtr pDest
        
            Unsafe.CopyBlock(destVoidPtr, srcVoidPtr, byteCount)
            dest















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
        let daterInt = [| 0 .. 999|] |> Array.map uint16

        let quab = V512.tile512uy daterByte
        let quint = tile512us daterInt

        //let hoo = qua.[5]
        //let boo = qua.[6]
        //let coo = Vector512.Add<byte>(hoo, boo)

        //// take the array qua and fold over it to produce the sum of all elements in the vectors
        //let yow = qua |> Array.mapi(fun i v -> Vector512.Add<byte>(v, qua.[i]))
        1




