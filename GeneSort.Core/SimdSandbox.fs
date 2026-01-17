namespace GeneSort.Core
open System
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86
open System.Runtime.Intrinsics.Arm
open System.Runtime.CompilerServices
open Microsoft.FSharp.NativeInterop

open ArrayUtils


//let a = 10y         // int8
//let a = 10uy        // uint8
//let a = 10          // int
//let b = 10u         // uint32
//let c = 10L         // int64
//let d = 10UL        // uint64
//let e = 3.14f       // float32
//let f = 3.14m       // decimal
//let g = 10I         // bigint


module SimdSandbox = 

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
        let result1 = stackAndBlock test1 2
        8

    // Type definitions
    [<Struct; CustomEquality; NoComparison>]
    type ce = private { low: int; hi: int } with

        static member create (lv: int) (hv: int) : ce =
            if lv < 0 || hv < 0 then
                failwith "Indices must be non-negative"
            else if lv < hv then
                { low = lv; hi = hv }
            else
                { low = hv; hi = lv }

        /// Gets the first TwoOrbit.
        member this.Low with get () = this.low

        /// Gets the second TwoOrbit (if present).
        member this.Hi with get () = this.hi

        override this.Equals(obj) = 
            match obj with
            | :? ce as other -> this.low = other.low && this.hi = other.hi
            | _ -> false
        override this.GetHashCode() = 
            hash (this.low, this.hi)
        interface IEquatable<ce> with
            member this.Equals(other) = 
                this.low = other.low && this.hi = other.hi



    let sortBy (block: Vector256<uint32> []) (cex: ce) : unit =
        let vLow = block.[cex.Low]
        let vHi = block.[cex.Hi]
        // SIMD Compare-and-Swap
        block.[cex.Low] <- Vector256.MinNumber(vLow, vHi)
        block.[cex.Hi] <- Vector256.MaxNumber(vLow, vHi)


    let sortBlock (block: Vector256<uint32> []) (cexs: ce []) : unit =
        for i = 0 to cexs.Length - 1 do
            sortBy block cexs.[i]















    let yab () =

        let test1 = [|
                        [| 1; 2; 3 |]
                        [| 3; 2; 1 |]
                        [| 4; 5; 6 |]
                        [| 6; 5; 4 |]
                        [| 7; 8; 9 |]
                        [| 9; 8; 7 |]
                        [| 10; 11; 12 |]
                        [| 12; 11; 10 |]
                        [| 15; 14; 13 |]
                        [| 13; 14; 15 |]
                    |] |> Array.map(fun v -> Array.map uint32 v)
        let result1 = stackAndBlock test1 8 |> Seq.toArray

        let qua = SimdUtils.V256.packToVector256 result1

        let block = qua.[0]

        let minny = Vector256.MinNumber (block.[0], block.[1])
        let maxy = Vector256.MaxNumber (block.[0], block.[1])

        let test1Back = unstackAndBlock result1 (Some 10) |> Seq.toArray

       // let qua = result1.
        1
