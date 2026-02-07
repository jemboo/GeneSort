namespace global

open BenchmarkDotNet.Attributes
open System
open GeneSort.Core
open System.Runtime.Intrinsics
open System.Buffers
open System.Threading.Tasks
open System.Runtime.InteropServices



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
type ParallelPipelineBenchmark_uint16() =
    
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

    [<Benchmark>]
    member this.Vector256_MultiplyAdd_Unrolled() =
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

                Fused256.multiplyAddCopyUnrolled (MemoryMarshal.CreateReadOnlySpan(&src.GetPinnableReference(), src.Length)) dst 5us 100us
        ) |> ignore

    [<Benchmark>]
    member this.Vector256_MultiplyAdd_Uint16_Unrolled() =
        let options =
            ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)

        let chunkSize =
            (DataSize512 + this.ParallelTracks - 1)
            / this.ParallelTracks

        Parallel.For(0, this.ParallelTracks, options, fun trackId ->
            let startIdx = trackId * chunkSize
            let endIdx   = min DataSize512 (startIdx + chunkSize)

            if startIdx < endIdx then
                let src =
                    sourceData256.AsSpan(startIdx, endIdx - startIdx)

                let dst =
                    destData256.AsSpan(startIdx, endIdx - startIdx)
                    
                Fused256.multiplyAddCopyUint16Unrolled (MemoryMarshal.CreateReadOnlySpan(&src.GetPinnableReference(), src.Length)) dst 5us 100us
        ) |> ignore





[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 10)>]
type ParallelPipelineBenchmark_uint8() =
    
    let DataSize256 = 200_000 
    let DataSize512 = 100_000 

    let mutable sourceData512 : Vector512<uint8>[] = [||]
    let mutable destData512 : Vector512<uint8>[] = [||]
        
    let mutable sourceData256 : Vector256<uint8>[] = [||]
    let mutable destData256 : Vector256<uint8>[] = [||]

    [<Params(4, 16)>] 
    member val DegreeOfParallelism = 1 with get, set

    [<Params(8, 16, 24, 32, 48, 64, 128)>] 
    member val ParallelTracks = 1 with get, set
    
    [<GlobalSetup>]
    member this.Setup() =

        sourceData512 <- Array.init DataSize512 (fun i -> Vector512.Create(uint8 (i % 65535)))
        destData512 <- Array.zeroCreate DataSize512
        sourceData256 <- Array.init DataSize256 (fun i -> Vector256.Create(uint8 (i % 65535)))
        destData256 <- Array.zeroCreate DataSize256

    [<Benchmark>]
    member this.Vector256_MultiplyAdd_Unrolled() =
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

                Fused256.multiplyAddCopyUnrolled (MemoryMarshal.CreateReadOnlySpan(&src.GetPinnableReference(), src.Length)) dst 5uy 100uy
        ) |> ignore

    [<Benchmark>]
    member this.Vector256_MultiplyAdd_Uint8_Unrolled() =
        let options =
            ParallelOptions(MaxDegreeOfParallelism = this.DegreeOfParallelism)

        let chunkSize =
            (DataSize512 + this.ParallelTracks - 1)
            / this.ParallelTracks

        Parallel.For(0, this.ParallelTracks, options, fun trackId ->
            let startIdx = trackId * chunkSize
            let endIdx   = min DataSize512 (startIdx + chunkSize)

            if startIdx < endIdx then
                let src =
                    sourceData256.AsSpan(startIdx, endIdx - startIdx)

                let dst =
                    destData256.AsSpan(startIdx, endIdx - startIdx)
                    
                Fused256.multiplyAddCopyUint8Unrolled (MemoryMarshal.CreateReadOnlySpan(&src.GetPinnableReference(), src.Length)) dst 5uy 100uy
        ) |> ignore


