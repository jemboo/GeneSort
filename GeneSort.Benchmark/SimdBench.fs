namespace global

open BenchmarkDotNet.Attributes
open System.Security.Cryptography
open System
open GeneSort.Core
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.SortingOps
open GeneSort.Sorter.Sorter
open System.Runtime.Intrinsics
open System.Buffers
open System.Threading.Tasks



[<MemoryDiagnoser>] // Tracks GC allocations
[<SimpleJob(warmupCount = 3, iterationCount = 10)>]
type ParallelCopyBenchmark() =
    
    // Adjust these for your hardware: 
    // DataSize * ParallelTracks should fit in RAM but exceed L3 cache to test bandwidth.
    let DataSize = 100_000 
    let ParallelTracks = 64 
    
    let mutable sourceData : Vector512<uint16>[] = [||]
    
    [<Params(1, 2, 4, 8, 12, 16)>] 
    member val DegreeOfParallelism = 1 with get, set

    [<GlobalSetup>]
    member this.Setup() =
        sourceData <- Array.init DataSize (fun i -> Vector512.Create(uint16 i))

    [<Benchmark>]
    member this.fastGenericCopyToBuffer() =
        let pool = ArrayPool<Vector512<uint16>>.Shared
        let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
        Parallel.For(0, ParallelTracks, options, fun i ->
            // 1. Rent from pool (prevents GC pressure)
            let buffer = pool.Rent(DataSize)
            try
                // 2. Perform the ultra-fast copy
                ArrayUtils.fastGenericCopyToBuffer sourceData buffer
                
                // 3. Simulate work (e.g., a SIMD operation) 
                // to prevent the JIT from optimizing the copy away
                // Perform your Sorter logic here on the slice:
                let workArea = buffer.AsSpan(0, DataSize)
                // Example: let sorted = MySimdSorter.Sort(workArea)
                ()
            finally
                // 4. Return to pool
                pool.Return(buffer)
        ) |> ignore




    [<Benchmark>]
    member self.UltraFastCopy() =
        let pool = ArrayPool<Vector512<uint16>>.Shared
        let options = ParallelOptions(MaxDegreeOfParallelism = self.DegreeOfParallelism)
        
        Parallel.For(0, ParallelTracks, options, fun i ->
            // 1. Rent from pool (prevents GC pressure)
            let buffer = pool.Rent(DataSize)
            try
                // 2. Perform the ultra-fast copy
                ArrayUtils.ultraFastCopy sourceData buffer
                
                // 3. Simulate work (e.g., a SIMD operation) 
                // to prevent the JIT from optimizing the copy away
                // Perform your Sorter logic here on the slice:
                let workArea = buffer.AsSpan(0, DataSize)
                // Example: let sorted = MySimdSorter.Sort(workArea)
                ()
            finally
                // 4. Return to pool
                pool.Return(buffer)
        ) |> ignore




[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 10)>]
type ParallelPipelineBenchmark() =
    
    let DataSize256 = 200_000 
    let DataSize512 = 100_000 

    let mutable sourceData512 : Vector512<uint16>[] = [||]
    let mutable destData512 : Vector512<uint16>[] = [||]
        
    let mutable sourceData256 : Vector256<uint16>[] = [||]
    let mutable destData256 : Vector256<uint16>[] = [||]

    [<Params(4, 16)>] 
    member val DegreeOfParallelism = 1 with get, set

    [<Params(8, 16, 24, 32, 48, 64, 128)>] 
    member val ParallelTracks = 1 with get, set
    
    [<GlobalSetup>]
    member this.Setup() =

        sourceData512 <- Array.init DataSize512 (fun i -> Vector512.Create(uint16 (i % 65535)))
        destData512 <- Array.zeroCreate DataSize512
        sourceData256 <- Array.init DataSize256 (fun i -> Vector256.Create(uint16 (i % 65535)))
        destData256 <- Array.zeroCreate DataSize256
    
    //[<Benchmark(Baseline = true)>]
    //member this.BlockProcessingMultiplyAdd512() =
    //    let blockSize = DataSize512 / this.ParallelTracks
    //    let pool = ArrayPool<Vector512<uint16>>.Shared
    //    let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
    //    Parallel.For(0, this.ParallelTracks, options, fun trackIdx ->
    //        let startIdx = trackIdx * blockSize
    //        let endIdx = if trackIdx = this.ParallelTracks - 1 then DataSize512 else (trackIdx + 1) * blockSize
    //        let currentBlockSize = endIdx - startIdx
            
    //        let buffer = pool.Rent(currentBlockSize)
    //        try
    //            // Copy source block to work buffer
    //            sourceData512.AsSpan(startIdx, currentBlockSize).CopyTo(buffer.AsSpan(0, currentBlockSize))
    //            let workArea = buffer.AsSpan(0, currentBlockSize)
                
    //            // SIMD operation: multiply by 5, add 100
    //            SimdUtils.SimdOps.multiplyAdd workArea 5us 100us
                
    //            // Copy results back to destination block
    //            workArea.CopyTo(destData512.AsSpan(startIdx, currentBlockSize))
    //        finally
    //            pool.Return(buffer)
    //    ) |> ignore

        
    //[<Benchmark>]
    //member this.BlockProcessingMultiplyAdd256() =
    //    let blockSize = DataSize256 / this.ParallelTracks
    //    let pool = ArrayPool<Vector256<uint16>>.Shared
    //    let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
    //    Parallel.For(0, this.ParallelTracks, options, fun trackIdx ->
    //        let startIdx = trackIdx * blockSize
    //        let endIdx = if trackIdx = this.ParallelTracks - 1 then DataSize256 else (trackIdx + 1) * blockSize
    //        let currentBlockSize = endIdx - startIdx
            
    //        let buffer = pool.Rent(currentBlockSize)
    //        try
    //            // Copy source block to work buffer
    //            sourceData256.AsSpan(startIdx, currentBlockSize).CopyTo(buffer.AsSpan(0, currentBlockSize))
    //            let workArea = buffer.AsSpan(0, currentBlockSize)
                
    //            // SIMD operation: multiply by 5, add 100
    //            SimdUtils.SimdOps256.multiplyAdd<uint16> workArea 5us 100us
                
    //            // Copy results back to destination block
    //            workArea.CopyTo(destData256.AsSpan(startIdx, currentBlockSize))
    //        finally
    //            pool.Return(buffer)
    //    ) |> ignore


    // ---------------------------
    // Vector256 Copy-On-Write
    // ---------------------------

    [<Benchmark>]
    member this.Vector256_MultiplyAdd_CopyOnWrite() =
        let options =
            ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)

        let chunkSize =
            (DataSize256 + this.ParallelTracks - 1)
            / this.ParallelTracks

        Parallel.For(0, this.ParallelTracks, options, fun trackId ->
            let startIdx = trackId * chunkSize
            let endIdx   = min DataSize256 (startIdx + chunkSize)

            if startIdx < endIdx then
                let src =
                    sourceData256.AsSpan(startIdx, endIdx - startIdx)

                let dst =
                    destData256.AsSpan(startIdx, endIdx - startIdx)

                // Copy-on-write
                src.CopyTo(dst)

                SimdUtils.SimdOps256.multiplyAdd<uint16> dst 5us 100us
        ) |> ignore


    [<Benchmark(Baseline = true)>]
    member this.Vector256_MultiplyAdd_Fused() =
        let options =
            ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)

        let chunkSize =
            (DataSize256 + this.ParallelTracks - 1)
            / this.ParallelTracks

        Parallel.For(0, this.ParallelTracks, options, fun trackId ->
            let startIdx = trackId * chunkSize
            let endIdx   = min DataSize256 (startIdx + chunkSize)

            if startIdx < endIdx then
                let src =
                    sourceData256.AsSpan(startIdx, endIdx - startIdx)

                let dst =
                    destData256.AsSpan(startIdx, endIdx - startIdx)

                Fused256.multiplyAddCopy src dst 5us 100us
        ) |> ignore


    //[<Benchmark>]
    //member this.Vector512_MultiplyAdd_Fused() =
    //    let options =
    //        ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)

    //    let chunkSize =
    //        (DataSize512 + this.ParallelTracks - 1)
    //        / this.ParallelTracks

    //    Parallel.For(0, this.ParallelTracks, options, fun trackId ->
    //        let startIdx = trackId * chunkSize
    //        let endIdx   = min DataSize512 (startIdx + chunkSize)

    //        if startIdx < endIdx then
    //            let src =
    //                sourceData512.AsSpan(startIdx, endIdx - startIdx)

    //            let dst =
    //                destData512.AsSpan(startIdx, endIdx - startIdx)

    //            Fused512.multiplyAddCopy src dst 5us 100us
    //    ) |> ignore






    //// ---------------------------
    //// Vector512 Copy-On-Write
    //// ---------------------------
    //[<Benchmark>]
    //member this.Vector512_MultiplyAdd_CopyOnWrite() =
    //    let options =
    //        ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)

    //    let chunkSize =
    //        (DataSize512 + this.ParallelTracks - 1)
    //        / this.ParallelTracks

    //    Parallel.For(0, this.ParallelTracks, options, fun trackId ->
    //        let startIdx = trackId * chunkSize
    //        let endIdx   = min DataSize512 (startIdx + chunkSize)

    //        if startIdx < endIdx then
    //            let src =
    //                sourceData512.AsSpan(startIdx, endIdx - startIdx)

    //            let dst =
    //                destData512.AsSpan(startIdx, endIdx - startIdx)

    //            // Copy-on-write
    //            src.CopyTo(dst)

    //            // SIMD operates on private cache lines
    //            SimdUtils.SimdOps.multiplyAdd dst 5us 100us
    //    ) |> ignore




    
    //[<Benchmark>]
    //member this.BlockProcessingXorPattern() =
    //    let blockSize = DataSize / ParallelTracks
    //    let pool = ArrayPool<Vector512<uint16>>.Shared
    //    let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
    //    Parallel.For(0, ParallelTracks, options, fun trackIdx ->
    //        let startIdx = trackIdx * blockSize
    //        let endIdx = if trackIdx = ParallelTracks - 1 then DataSize else (trackIdx + 1) * blockSize
    //        let currentBlockSize = endIdx - startIdx
            
    //        let buffer = pool.Rent(currentBlockSize)
    //        try
    //            sourceData.AsSpan(startIdx, currentBlockSize).CopyTo(buffer.AsSpan(0, currentBlockSize))
    //            let workArea = buffer.AsSpan(0, currentBlockSize)
                
    //            // SIMD operation: XOR with pattern
    //            SimdUtils.SimdOps.xorPattern workArea 0x5555us
                
    //            workArea.CopyTo(destData.AsSpan(startIdx, currentBlockSize))
    //        finally
    //            pool.Return(buffer)
    //    ) |> ignore
    
    //[<Benchmark>]
    //member this.BlockProcessingClamp() =
    //    let blockSize = DataSize256 / ParallelTracks
    //    let pool = ArrayPool<Vector512<uint16>>.Shared
    //    let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
    //    Parallel.For(0, ParallelTracks, options, fun trackIdx ->
    //        let startIdx = trackIdx * blockSize
    //        let endIdx = if trackIdx = ParallelTracks - 1 then DataSize256 else (trackIdx + 1) * blockSize
    //        let currentBlockSize = endIdx - startIdx
            
    //        let buffer = pool.Rent(currentBlockSize)
    //        try
    //            sourceData512.AsSpan(startIdx, currentBlockSize).CopyTo(buffer.AsSpan(0, currentBlockSize))
    //            let workArea = buffer.AsSpan(0, currentBlockSize)
                
    //            // SIMD operation: clamp values between 1000 and 50000
    //            SimdUtils.SimdOps.clamp workArea 1000us 50000us
                
    //            workArea.CopyTo(destData512.AsSpan(startIdx, currentBlockSize))
    //        finally
    //            pool.Return(buffer)
    //    ) |> ignore

    //[<Benchmark>]
    //member this.BlockProcessingClampGeneric() =
    //    let blockSize = DataSize256 / ParallelTracks
    //    let pool = ArrayPool<Vector512<uint16>>.Shared
    //    let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
    //    Parallel.For(0, ParallelTracks, options, fun trackIdx ->
    //        let startIdx = trackIdx * blockSize
    //        let endIdx = if trackIdx = ParallelTracks - 1 then DataSize256 else (trackIdx + 1) * blockSize
    //        let currentBlockSize = endIdx - startIdx
            
    //        let buffer = pool.Rent(currentBlockSize)
    //        try
    //            sourceData512.AsSpan(startIdx, currentBlockSize).CopyTo(buffer.AsSpan(0, currentBlockSize))
    //            let workArea = buffer.AsSpan(0, currentBlockSize)
                
    //            // SIMD operation: clamp values between 1000 and 50000
    //            SimdUtils.SimdOps.clamp workArea 1000us 50000us
                
    //            workArea.CopyTo(destData512.AsSpan(startIdx, currentBlockSize))
    //        finally
    //            pool.Return(buffer)
    //    ) |> ignore








    
    //[<Benchmark>]
    //member this.BlockProcessingShiftAndAdd() =
    //    let blockSize = DataSize / ParallelTracks
    //    let pool = ArrayPool<Vector512<uint16>>.Shared
    //    let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
    //    Parallel.For(0, ParallelTracks, options, fun trackIdx ->
    //        let startIdx = trackIdx * blockSize
    //        let endIdx = if trackIdx = ParallelTracks - 1 then DataSize else (trackIdx + 1) * blockSize
    //        let currentBlockSize = endIdx - startIdx
            
    //        let buffer = pool.Rent(currentBlockSize)
    //        try
    //            sourceData.AsSpan(startIdx, currentBlockSize).CopyTo(buffer.AsSpan(0, currentBlockSize))
    //            let workArea = buffer.AsSpan(0, currentBlockSize)
                
    //            // SIMD operation: shift right by 2 and add to original
    //            SimdUtils.SimdOps.shiftAndAdd workArea 2
                
    //            workArea.CopyTo(destData.AsSpan(startIdx, currentBlockSize))
    //        finally
    //            pool.Return(buffer)
    //    ) |> ignore
    
    //[<Benchmark>]
    //member this.BlockProcessingComplexPipeline() =
    //    let blockSize = DataSize / ParallelTracks
    //    let pool = ArrayPool<Vector512<uint16>>.Shared
    //    let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
    //    Parallel.For(0, ParallelTracks, options, fun trackIdx ->
    //        let startIdx = trackIdx * blockSize
    //        let endIdx = if trackIdx = ParallelTracks - 1 then DataSize else (trackIdx + 1) * blockSize
    //        let currentBlockSize = endIdx - startIdx
            
    //        let buffer = pool.Rent(currentBlockSize)
    //        try
    //            sourceData.AsSpan(startIdx, currentBlockSize).CopyTo(buffer.AsSpan(0, currentBlockSize))
    //            let workArea = buffer.AsSpan(0, currentBlockSize)
                
    //            // SIMD operation: complex pipeline (multiply, clamp, xor)
    //            SimdUtils.SimdOps.complexPipeline workArea
                
    //            workArea.CopyTo(destData.AsSpan(startIdx, currentBlockSize))
    //        finally
    //            pool.Return(buffer)
    //    ) |> ignore
    
    //[<Benchmark>]
    //member this.BlockProcessingMultipleOperations() =
    //    let blockSize = DataSize / ParallelTracks
    //    let pool = ArrayPool<Vector512<uint16>>.Shared
    //    let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
    //    Parallel.For(0, ParallelTracks, options, fun trackIdx ->
    //        let startIdx = trackIdx * blockSize
    //        let endIdx = if trackIdx = ParallelTracks - 1 then DataSize else (trackIdx + 1) * blockSize
    //        let currentBlockSize = endIdx - startIdx
            
    //        let buffer = pool.Rent(currentBlockSize)
    //        try
    //            sourceData.AsSpan(startIdx, currentBlockSize).CopyTo(buffer.AsSpan(0, currentBlockSize))
    //            let workArea = buffer.AsSpan(0, currentBlockSize)
                
    //            // Chain multiple SIMD operations
    //            SimdUtils.SimdOps.multiplyAdd workArea 2us 50us
    //            SimdUtils.SimdOps.xorPattern workArea 0xFFFFus
    //            SimdUtils.SimdOps.clamp workArea 500us 40000us
                
    //            workArea.CopyTo(destData.AsSpan(startIdx, currentBlockSize))
    //        finally
    //            pool.Return(buffer)
    //    ) |> ignore
    
    //[<Benchmark>]
    //member this.BlockProcessingWithVariableTrackLoad() =
    //    // Simulate variable workload per track (some blocks need more processing)
    //    let blockSize = DataSize / ParallelTracks
    //    let pool = ArrayPool<Vector512<uint16>>.Shared
    //    let options = ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)
        
    //    Parallel.For(0, ParallelTracks, options, fun trackIdx ->
    //        let startIdx = trackIdx * blockSize
    //        let endIdx = if trackIdx = ParallelTracks - 1 then DataSize else (trackIdx + 1) * blockSize
    //        let currentBlockSize = endIdx - startIdx
            
    //        let buffer = pool.Rent(currentBlockSize)
    //        try
    //            sourceData.AsSpan(startIdx, currentBlockSize).CopyTo(buffer.AsSpan(0, currentBlockSize))
    //            let workArea = buffer.AsSpan(0, currentBlockSize)
                
    //            // Variable workload based on track index
    //            if trackIdx % 4 = 0 then
    //                // Heavy processing for every 4th track
    //                SimdUtils.SimdOps.complexPipeline workArea
    //                SimdUtils.SimdOps.multiplyAdd workArea 2us 100us
    //            elif trackIdx % 2 = 0 then
    //                // Medium processing
    //                SimdUtils.SimdOps.multiplyAdd workArea 3us 50us
    //            else
    //                // Light processing
    //                SimdUtils.SimdOps.xorPattern workArea 0xAAAAus
                
    //            workArea.CopyTo(destData.AsSpan(startIdx, currentBlockSize))
    //        finally
    //            pool.Return(buffer)
    //    ) |> ignore
