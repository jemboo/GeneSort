namespace GeneSort.SortingOps

open System.Runtime.Intrinsics
open System.Threading.Tasks
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open System.Collections.Concurrent
open System.Collections.Generic
open System
open System.Threading


module CeBlockOpsUint8v256 =

    /// Check if all 32 lanes in the block are sorted.
    let IsBlockSorted (len: int) (vectors: Vector256<uint8>[]) =
        let mutable sorted = true
        let mutable i = 0
        while i < len - 1 && sorted do
            // In a sorted lane, v[i] <= v[i+1]. 
            // Therefore, Min(v[i], v[i+1]) must be equal to v[i].
            let vLow = vectors.[i]
            let vHi = vectors.[i+1]
            if Vector256.Min(vLow, vHi) <> vLow then
                sorted <- false
            else
                i <- i + 1
        sorted

/// Mask-optimized: Check if all 32 lanes in the block are sorted.
    let IsBlockSortedMask (len: int) (vectors: Vector256<uint8>[]) =
        let mutable sorted = true
        let mutable i = 0
        while i < len - 1 do
            let vLow = vectors.[i]
            let vHi = vectors.[i+1]
            
            // Mask: 0xFF where ordered (Low <= Hi), 0x00 otherwise
            let mask = Vector256.Equals(Vector256.Min(vLow, vHi), vLow)
            
            // Extract the MSB of each of the 32 bytes into a 32-bit integer
            let bitmask = Vector256.ExtractMostSignificantBits(mask)
            
            // If all 32 lanes are sorted, bitmask will be 0xFFFFFFFFu
            if bitmask <> 0xffffffffu then
                sorted <- false
                i <- len // Break
            else
                i <- i + 1
        sorted


    /// Computes 32 separate hashes for the 32 lanes in the block.
    let computeLaneHashes32 (len: int) (vectors: Vector256<uint8>[]) =
        let hashes = Array.create 32 17u
        for i = 0 to len - 1 do
            let v = vectors.[i]
            for lane = 0 to 31 do
                hashes.[lane] <- hashes.[lane] * 31u + uint32 (v.GetElement(lane))
        hashes


    let evalSimdSortBlocks
            (simdSortBlocks: SortBlockUint8v256 seq) 
            (ceBlocks: ceBlock array) 
            : ceBlockEval [] =
    
            let numNetworks = ceBlocks.Length
            if numNetworks = 0 then [||] else

            let maxWidth = ceBlocks |> Array.maxBy (fun c -> %c.SortingWidth) |> (fun c -> %c.SortingWidth)
            let networkData = ceBlocks |> Array.map (fun ceb -> 
                {| Lows = ceb.CeArray |> Array.map (fun c -> c.Low)
                   Highs = ceb.CeArray |> Array.map (fun c -> c.Hi)
                   CeLen = ceb.CeArray.Length |})

            let globalUsage = Array.init numNetworks (fun i -> ceUseCounts.Create(ceBlocks.[i].Length))
            let globalUnsorted = Array.zeroCreate<int> numNetworks

            Parallel.ForEach(
                simdSortBlocks, 
                // 1. Initialize Thread-Local State (TLS)
                (fun () -> 
                    let usage = Array.init numNetworks (fun i -> Array.zeroCreate<int> networkData.[i].CeLen)
                    let unsorted = Array.zeroCreate<int> numNetworks
                    let buffer = Array.zeroCreate<Vector256<uint8>> maxWidth
                    (usage, unsorted, buffer)),
    
                // 2. The Hot Loop
                // Explicit type annotations for the state tuple to satisfy the F# compiler
                (fun currentBlock loopState ((localUsage: int [][]), (localUnsorted: int []), (workBuffer: Vector256<uint8> [])) ->
                    let currentLen = currentBlock.Length

                    for nIdx = 0 to numNetworks - 1 do
                        // Copy source vectors to local work buffer
                        Array.blit currentBlock.Vectors 0 workBuffer 0 currentLen
                        let data = networkData.[nIdx]
                        let ceCount = data.CeLen
                        let currentNetUsage = localUsage.[nIdx]
                
                        // --- Hot Loop: Compare and Swap ---
                        for cIdx = 0 to ceCount - 1 do
                            let lIdx = data.Lows.[cIdx]
                            let hIdx = data.Highs.[cIdx]
                            let vLow = workBuffer.[lIdx]
                            let vHi = workBuffer.[hIdx]
                        
                            let vMin = Vector256.Min(vLow, vHi)
                
                            // If vLow is not the minimum, at least one lane swapped
                            if vLow <> vMin then
                                currentNetUsage.[cIdx] <- currentNetUsage.[cIdx] + 1
                                workBuffer.[lIdx] <- vMin
                                workBuffer.[hIdx] <- Vector256.Max(vLow, vHi)

                        // Verification
                        if not (IsBlockSortedMask currentLen workBuffer) then
                            localUnsorted.[nIdx] <- localUnsorted.[nIdx] + 1
                
                    // RETURN the state so it is available for the next iteration/merge
                    (localUsage, localUnsorted, workBuffer)),

                // 3. Final Merge (TLS to Global)
                (fun ((localUsage: int [][]), (localUnsorted: int []), _) ->
                    for nIdx = 0 to numNetworks - 1 do
                        // Merge unsorted counts
                        if localUnsorted.[nIdx] > 0 then
                            Interlocked.Add(&globalUnsorted.[nIdx], localUnsorted.[nIdx]) |> ignore
                    
                        // Merge usage counts
                        let globalArr = globalUsage.[nIdx]
                        let threadLocalArr = localUsage.[nIdx]
                        for i = 0 to threadLocalArr.Length - 1 do
                            if threadLocalArr.[i] > 0 then
                                globalArr.IncrementAtomicBy (i |> UMX.tag) threadLocalArr.[i]
                )
            ) |> ignore

            Array.init numNetworks (fun i ->
                ceBlockEval.create ceBlocks.[i] globalUsage.[i] (globalUnsorted.[i] |> UMX.tag) None
            )


    let eval 
            (test: sortableUint8v256Test) 
            (ceBlocks: ceBlock []) =
            //let chunkedStream = test.SimdSortBlocks |> Seq.chunkBySize chunkSize
            //evalSimdSortBlockChunks chunkedStream ceBlocks
            evalSimdSortBlocks test.SimdSortBlocks ceBlocks







    /// Computes a hash for a single lane across all vectors in the block.
    /// This allows us to check for duplicates without extracting the full array.
    let inline hashLane (block: Vector256<uint8>[]) (lane: int) =
        let mutable h = 17
        for i = 0 to block.Length - 1 do
            h <- h * 31 + int (block.[i].GetElement(lane))
        h


    type ArrayContentComparer<'T when 'T : equality>() =
        interface IEqualityComparer<'T[]> with
            member _.Equals(x, y) =
                if obj.ReferenceEquals(x, y) then true
                elif isNull x || isNull y then false
                elif x.Length <> y.Length then false
                else ReadOnlySpan<'T>(x).SequenceEqual(ReadOnlySpan<'T>(y))

            member _.GetHashCode(obj) =
                let mutable h = 17
                for i = 0 to obj.Length - 1 do
                    h <- h * 31 + hash (obj.[i])
                h

    let evalAndCollectUniqueFailures
        (simdSortBlocks: SortBlockUint8v256 seq) 
        (ceBlocks: ceBlock array) 
        : ceBlockEval [] =
    
        let numNetworks = ceBlocks.Length
        if numNetworks = 0 then [||] else

        // Pre-calculate network metadata for fast access in the hot loop
        let maxWidth = ceBlocks |> Array.maxBy (fun c -> %c.SortingWidth) |> (fun c -> %c.SortingWidth)
        let networkData = ceBlocks |> Array.map (fun ceb -> 
            {| Lows = ceb.CeArray |> Array.map (fun c -> c.Low)
               Highs = ceb.CeArray |> Array.map (fun c -> c.Hi)
               SortingWidth = ceb.SortingWidth
               CeLen = ceb.CeArray.Length |})

        // Final global storage
        let globalUsage = Array.init numNetworks (fun i -> ceUseCounts.Create(ceBlocks.[i].Length))
        let globalUnsorted = Array.zeroCreate<int> numNetworks
    
        // ConcurrentDictionary with explicit type annotation and custom content comparer
        let failureSets: ConcurrentDictionary<int[], byte> [] = 
            Array.init numNetworks (fun _ -> 
                ConcurrentDictionary<int[], byte>(ArrayContentComparer<int>()))

        Parallel.ForEach(
            simdSortBlocks, 
            // 1. Initialize Thread-Local State (TLS)
            (fun () -> 
                let usage = Array.init numNetworks (fun i -> Array.zeroCreate<int> networkData.[i].CeLen)
                let unsorted = Array.zeroCreate<int> numNetworks
                let buffer = Array.zeroCreate<Vector256<uint8>> maxWidth
                (usage, unsorted, buffer)),
    
            // 2. The Hot Loop (Processing one block at a time)
            (fun currentBlock loopState ((localUsage: int [][]), (localUnsorted: int []), workBuffer) ->
                let currentLen = currentBlock.Length
            


                for nIdx = 0 to numNetworks - 1 do
                    // Blit current vectors into the reusable thread-local buffer
                    Array.blit currentBlock.Vectors 0 workBuffer 0 currentLen
                    let data = networkData.[nIdx]
                    let ceCount = data.CeLen
                    let goldenHashes = Simd256GoldenHashProvider.GetGoldenHashes data.SortingWidth
            
                    // --- Hot Loop: CAS Sorting ---
                    for cIdx = 0 to ceCount - 1 do
                        let lIdx = data.Lows.[cIdx]
                        let hIdx = data.Highs.[cIdx]
                        let vLow = workBuffer.[lIdx]
                        let vHi = workBuffer.[hIdx]
                        let vMin = Vector256.Min(vLow, vHi)
                
                        if vLow <> vMin then
                            localUsage.[nIdx].[cIdx] <- localUsage.[nIdx].[cIdx] + 1
                            workBuffer.[lIdx] <- vMin
                            workBuffer.[hIdx] <- Vector256.Max(vLow, vHi)

                    // --- Verification and Failure Extraction ---
                    // Vertical check (SIMD)
                    if not (IsBlockSortedMask currentLen workBuffer) then
                        localUnsorted.[nIdx] <- localUnsorted.[nIdx] + 1
                    
                        // Horizontal check (32-bit Hash)
                        let currentHashes = computeLaneHashes32 currentLen workBuffer
                    
                        // Only process lanes that actually contain test data
                        for lane = 0 to currentBlock.SortableCount - 1 do
                            if currentHashes.[lane] <> goldenHashes.[lane] then
                                // Rare path: Extract unique failure sequence
                                let laneArray = Array.init currentLen (fun i -> 
                                    int (workBuffer.[i].GetElement(lane)))
                            
                                failureSets.[nIdx].TryAdd(laneArray, 0uy) |> ignore
            
                (localUsage, localUnsorted, workBuffer)),

            // 3. Final Merge (Consolidating TLS into Global state)
            (fun ((localUsage: int [][]), (localUnsorted: int []), _) ->
                for nIdx = 0 to numNetworks - 1 do
                    Interlocked.Add(&globalUnsorted.[nIdx], localUnsorted.[nIdx]) |> ignore
                    let globalArr = globalUsage.[nIdx]
                    for i = 0 to localUsage.[nIdx].Length - 1 do
                        globalArr.IncrementAtomicBy (i |> UMX.tag<ceIndex>) localUsage.[nIdx].[i]
            )
        ) |> ignore

        // 4. Assemble final ceBlockEval results
        Array.init numNetworks (fun i ->
            let uniqueFailures = failureSets.[i].Keys |> Seq.toArray
            let failTest = 
                if uniqueFailures.Length > 0 then
                    let sw = ceBlocks.[i].SortingWidth
                    let sss = int sw |> UMX.tag<symbolSetSize>
                    uniqueFailures 
                    |> Array.map (fun arr -> sortableIntArray.create(arr, sw, sss))
                    |> sortableIntTest.create (Guid.NewGuid() |> UMX.tag) sw
                    |> sortableTest.Ints
                    |> Some
                else None

            ceBlockEval.create 
                ceBlocks.[i] 
                globalUsage.[i] 
                (globalUnsorted.[i] |> UMX.tag<sortableCount>) 
                failTest
        )


    let evalAndCollectResults 
                    (test: sortableUint8v256Test) 
                    (ceBlocks: ceBlock []) =
        evalAndCollectUniqueFailures test.SimdSortBlocks ceBlocks