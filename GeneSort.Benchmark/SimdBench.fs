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