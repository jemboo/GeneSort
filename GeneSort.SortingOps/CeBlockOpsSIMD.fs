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


module CeBlockOpsSIMD256 =

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


    let evalSimdSortBlockChunks 
        (simdSortBlockChunks: simdSortBlock array seq) 
        (ceBlocks: ceBlock array) 
        : ceBlockEval [] =
        
        let numNetworks = ceBlocks.Length
        // Use your ceUseCounts type for global storage
        let globalUsage = Array.init numNetworks (fun i -> 
            ceUseCounts.Create(ceBlocks.[i].Length))
        let globalUnsorted = Array.zeroCreate<int> numNetworks
        let locks = Array.init numNetworks (fun _ -> obj())

        Parallel.ForEach(simdSortBlockChunks, (fun (chunk: simdSortBlock array) ->
            for nIdx = 0 to numNetworks - 1 do
                let ceb = ceBlocks.[nIdx]
                let len = %ceb.Length
                
                // Local tracking for this chunk
                let localCounts = Array.zeroCreate<int> len
                let mutable localUnsorted = 0
                
                for bIdx = 0 to chunk.Length - 1 do
                    let currentBlock = chunk.[bIdx]
                    let testBlock = Array.copy currentBlock.Vectors
                    for cIdx = 0 to len - 1 do
                        let cex = ceb.getCe cIdx
                        let vLow = testBlock.[cex.Low]
                        let vHi = testBlock.[cex.Hi]
                        let vMin = Vector256.Min(vLow, vHi)
                        
                        if vLow <> vMin then
                            localCounts.[cIdx] <- localCounts.[cIdx] + 1
                            testBlock.[cex.Low] <- vMin
                            testBlock.[cex.Hi] <- Vector256.Max(vLow, vHi)
                    
                    if not (IsBlockSortedMask currentBlock.Length  testBlock) then
                        localUnsorted <- localUnsorted + 1

                // Sync local results to the ceUseCounts container
                lock locks.[nIdx] (fun () ->
                    globalUnsorted.[nIdx] <- globalUnsorted.[nIdx] + localUnsorted
                    for i = 0 to len - 1 do
                        // Using your custom IncrementBy
                        globalUsage.[nIdx].IncrementBy (i |> UMX.tag<ceIndex>) localCounts.[i]
                )
        )) |> ignore

        // Return the high-level ceBlockWithUsage type
        Array.init numNetworks (fun i ->
            let usageWithBlock = ceBlockEval.create 
                                        ceBlocks.[i] 
                                        globalUsage.[i]
                                        (globalUnsorted.[i] |> UMX.tag<sortableCount>)
                                        None
            usageWithBlock
        )




    let evalSimdSortBlockChunks2
        (simdSortBlockChunks: simdSortBlock array seq) 
        (ceBlocks: ceBlock array) 
        : ceBlockEval [] =

        let numNetworks = ceBlocks.Length
        if numNetworks = 0 then [||] else

        let maxWidth = ceBlocks |> Array.maxBy (fun c -> %c.SortingWidth) |> (fun c -> %c.SortingWidth)
        let networkData = ceBlocks |> Array.map (fun ceb -> 
            {| Lows = ceb.CeArray |> Array.map (fun c -> c.Low)
               Highs = ceb.CeArray |> Array.map (fun c -> c.Hi)
               SortingWidth = ceb.SortingWidth
               CeLen = ceb.CeArray.Length |})

        // Final global storage
        let globalUsage = Array.init numNetworks (fun i -> ceUseCounts.Create(ceBlocks.[i].Length))
        let globalUnsorted = Array.zeroCreate<int> numNetworks

        Parallel.ForEach(
            simdSortBlockChunks, 
            // 1. Initialize Thread-Local State
            (fun () -> 
                let usage = Array.init numNetworks (fun i -> Array.zeroCreate<int> networkData.[i].CeLen)
                let unsorted = Array.zeroCreate<int> numNetworks
                let buffer = Array.zeroCreate<Vector256<uint8>> maxWidth
                (usage, unsorted, buffer)),
        
            // 2. The Hot Loop (ZERO LOCKS)
            (fun chunk loopState ((localUsage: int [][]), (localUnsorted: int []), workBuffer) ->
                for bIdx = 0 to chunk.Length - 1 do
                    let currentBlock = chunk.[bIdx]
                    let currentLen = currentBlock.Length
                    Array.blit currentBlock.Vectors 0 workBuffer 0 currentLen

                    for nIdx = 0 to numNetworks - 1 do
                        let data = networkData.[nIdx]
                        let ceCount = data.CeLen
                        let hashes = SimdGoldenHashProvider.GetGoldenHashes data.SortingWidth
                    
                        // Sorting...
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

                        // Verification...
                        if not (IsBlockSortedMask currentLen workBuffer) then
                            localUnsorted.[nIdx] <- localUnsorted.[nIdx] + 1
            
                (localUsage, localUnsorted, workBuffer)),

            // 3. Final Merge (Happens once per thread at the end)
            (fun ((localUsage: int [][]), (localUnsorted: int []), _) ->
                for nIdx = 0 to numNetworks - 1 do
                    Interlocked.Add(&globalUnsorted.[nIdx], localUnsorted.[nIdx]) |> ignore
                    let globalArr = globalUsage.[nIdx]
                    for i = 0 to localUsage.[nIdx].Length - 1 do
                        globalArr.IncrementAtomicBy (i |> UMX.tag) localUsage.[nIdx].[i]
            )
        ) |> ignore

        // 3. Assemble final domain objects
        Array.init numNetworks (fun i ->
            ceBlockEval.create 
                ceBlocks.[i] 
                globalUsage.[i] 
                (globalUnsorted.[i] |> UMX.tag) 
                None
        )

    let eval 
            (test: sortableUint8v256Test) 
            (ceBlocks: ceBlock []) 
            (chunkSize: int) : ceBlockEval[] =
            let chunkedStream = test.SimdSortBlocks |> Seq.chunkBySize chunkSize
            evalSimdSortBlockChunks2 chunkedStream ceBlocks
        



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


    let evalChunkedAndCollectUniqueFailures
        (simdSortBlockChunks: simdSortBlock array seq) 
        (ceBlocks: ceBlock array) 
        : ceBlockEval [] =
    
        let numNetworks = ceBlocks.Length
        let globalUsage = Array.init numNetworks (fun i -> ceUseCounts.Create(ceBlocks.[i].Length))
        let globalUnsorted = Array.zeroCreate<int> numNetworks
    
        // Explicit type annotation for the ConcurrentDictionary to satisfy the compiler
        let failureSets: ConcurrentDictionary<int[], byte> [] = 
            Array.init numNetworks (fun _ -> 
                ConcurrentDictionary<int[], byte>(ArrayContentComparer<int>()))
    
        let locks = Array.init numNetworks (fun _ -> obj())

        Parallel.ForEach(simdSortBlockChunks, (fun (chunk: simdSortBlock array) ->
            for nIdx = 0 to numNetworks - 1 do
                let ceb = ceBlocks.[nIdx]
                let networkLen = %ceb.Length
                let goldenHashes = SimdGoldenHashProvider.GetGoldenHashes ceb.SortingWidth
            
                let localCounts = Array.zeroCreate<int> networkLen
                let mutable localUnsortedCount = 0
            
                for bIdx = 0 to chunk.Length - 1 do
                    let currentBlock = chunk.[bIdx]
                    // Work on a copy to preserve original test data for other networks
                    let testVecs = Array.copy currentBlock.Vectors
                
                    // --- Hot Loop: CAS Sorting ---
                    for cIdx = 0 to networkLen - 1 do
                        let cex = ceb.getCe cIdx
                        let vLow = testVecs.[cex.Low]
                        let vHi = testVecs.[cex.Hi]
                        let vMin = Vector256.Min(vLow, vHi)
                    
                        if vLow <> vMin then
                            localCounts.[cIdx] <- localCounts.[cIdx] + 1
                            testVecs.[cex.Low] <- vMin
                            testVecs.[cex.Hi] <- Vector256.Max(vLow, vHi)

                    // --- Failure Detection & Collection ---
                    // Vertical check first: fast path if all 32 lanes are sorted
                    if not (IsBlockSortedMask currentBlock.Length testVecs) then
                        // Horizontal check: identify specific failing lanes using 32-bit hashes
                        let currentHashes = SimdSortBlock.computeLaneHashes32 testVecs
                        let mutable blockHadAtLeastOneRealFailure = false
                    
                        // Respect the SortableCount to ignore golden padding lanes
                        for lane = 0 to currentBlock.SortableCount - 1 do
                            if currentHashes.[lane] <> goldenHashes.[lane] then
                                blockHadAtLeastOneRealFailure <- true
                            
                                // Extract only the specific failing lane
                                let laneArray = Array.init testVecs.Length (fun i -> 
                                    int (testVecs.[i].GetElement(lane)))
                            
                                // Deduplicate via ConcurrentDictionary
                                failureSets.[nIdx].TryAdd(laneArray, 0uy) |> ignore
                    
                        if blockHadAtLeastOneRealFailure then
                            localUnsortedCount <- localUnsortedCount + 1

                lock locks.[nIdx] (fun () ->
                    globalUnsorted.[nIdx] <- globalUnsorted.[nIdx] + localUnsortedCount
                    for i = 0 to networkLen - 1 do
                        globalUsage.[nIdx].IncrementBy (i |> UMX.tag<ceIndex>) localCounts.[i]
                )
        )) |> ignore

        // Assemble final evaluations
        Array.init numNetworks (fun i ->
            let uniqueFailures = failureSets.[i].Keys |> Seq.toArray
        
            let failTest = 
                if uniqueFailures.Length > 0 then
                    let sw = ceBlocks.[i].SortingWidth
                    let sss = int sw |> UMX.tag<symbolSetSize>
                    uniqueFailures 
                    |> Array.map (fun arr -> sortableIntArray.create(arr, sw, sss))
                    |> sortableIntTest.create (Guid.NewGuid() |> UMX.tag<sorterTestId>) sw
                    |> sortableTest.Ints
                    |> Some
                else 
                    None

            ceBlockEval.create 
                ceBlocks.[i] 
                globalUsage.[i] 
                (globalUnsorted.[i] |> UMX.tag<sortableCount>) 
                failTest
        )



    let evalChunkedAndCollectUniqueFailures2
        (simdSortBlockChunks: simdSortBlock array seq) 
        (ceBlocks: ceBlock array) 
        : ceBlockEval [] =

        let numNetworks = ceBlocks.Length
        if numNetworks = 0 then [||] else

        let maxWidth = ceBlocks |> Array.maxBy (fun c -> %c.SortingWidth) |> (fun c -> %c.SortingWidth)
        let networkData = ceBlocks |> Array.map (fun ceb -> 
            {| Lows = ceb.CeArray |> Array.map (fun c -> c.Low)
               Highs = ceb.CeArray |> Array.map (fun c -> c.Hi)
               SortingWidth = ceb.SortingWidth
               CeLen = ceb.CeArray.Length |})

        // Final global storage
        let globalUsage = Array.init numNetworks (fun i -> ceUseCounts.Create(ceBlocks.[i].Length))
        let globalUnsorted = Array.zeroCreate<int> numNetworks
        let failureSets = Array.init numNetworks (fun _ -> ConcurrentDictionary<int[], byte>(ArrayContentComparer<int>()))

        Parallel.ForEach(
            simdSortBlockChunks, 
            // 1. Initialize Thread-Local State
            (fun () -> 
                let usage = Array.init numNetworks (fun i -> Array.zeroCreate<int> networkData.[i].CeLen)
                let unsorted = Array.zeroCreate<int> numNetworks
                let buffer = Array.zeroCreate<Vector256<uint8>> maxWidth
                (usage, unsorted, buffer)),
        
            // 2. The Hot Loop (ZERO LOCKS)
            (fun chunk loopState ((localUsage: int [][]), (localUnsorted: int []), workBuffer) ->
                for bIdx = 0 to chunk.Length - 1 do
                    let currentBlock = chunk.[bIdx]
                    let currentLen = currentBlock.Length
                    Array.blit currentBlock.Vectors 0 workBuffer 0 currentLen

                    for nIdx = 0 to numNetworks - 1 do
                        let data = networkData.[nIdx]
                        let ceCount = data.CeLen
                        let hashes = SimdGoldenHashProvider.GetGoldenHashes data.SortingWidth
                    
                        // Sorting...
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

                        // Verification...
                        if not (IsBlockSortedMask currentLen workBuffer) then
                            localUnsorted.[nIdx] <- localUnsorted.[nIdx] + 1
                            let currentHashes = computeLaneHashes32 currentLen workBuffer
                            for lane = 0 to currentBlock.SortableCount - 1 do
                                if currentHashes.[lane] <> hashes.[lane] then
                                    let laneArray = Array.init currentLen (fun i -> int (workBuffer.[i].GetElement(lane)))
                                    failureSets.[nIdx].TryAdd(laneArray, 0uy) |> ignore
            
                (localUsage, localUnsorted, workBuffer)),

            // 3. Final Merge (Happens once per thread at the end)
            (fun ((localUsage: int [][]), (localUnsorted: int []), _) ->
                for nIdx = 0 to numNetworks - 1 do
                    Interlocked.Add(&globalUnsorted.[nIdx], localUnsorted.[nIdx]) |> ignore
                    let globalArr = globalUsage.[nIdx]
                    for i = 0 to localUsage.[nIdx].Length - 1 do
                        globalArr.IncrementAtomicBy (i |> UMX.tag) localUsage.[nIdx].[i]
            )
        ) |> ignore

        // 3. Assemble final domain objects
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
                (globalUnsorted.[i] |> UMX.tag) 
                failTest
        )



    let evalAndCollectResults 
                    (test: sortableUint8v256Test) 
                    (ceBlocks: ceBlock []) 
                    (chunkSize: int) : ceBlockEval[] =
        let chunkedStream = test.SimdSortBlocks |> Seq.chunkBySize chunkSize
        evalChunkedAndCollectUniqueFailures2 chunkedStream ceBlocks