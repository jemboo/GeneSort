namespace GeneSort.SortingOps

open System.Runtime.CompilerServices
open System.Runtime.Intrinsics
open System.Threading.Tasks
open FSharp.UMX
open GeneSort.Sorting.Sorter
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open System.Collections.Concurrent
open System.Collections.Generic
open System


module CeBlockOpsSIMD256 =

    let packToVector256Stream (data: seq<uint8[][]>) : seq<Vector256<uint8>[]> =
        data |> Seq.map (fun block ->
            let k = block.Length
            Array.init k (fun i -> Vector256.Create(block.[i], 0))
        )


    /// Checks if a block is vertically sorted across all SIMD lanes.
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    let IsBlockSorted (block: Vector256<uint8> []) : bool =
        let mutable sorted = true
        let mutable i = 0
        // Check if block.[i] <= block.[i+1] for all i
        while sorted && i < block.Length - 1 do
            // Vector256.LessThanOrEqual returns a mask. 
            // We need to check if that mask is 'AllOnes' (true for all lanes).
            let mask = Vector256.LessThanOrEqual(block.[i], block.[i+1])
            if Vector256.ExtractMostSignificantBits(mask) <> 0xffffffffu then
                sorted <- false
            i <- i + 1
        sorted

    /// Processes a stream of blocks through multiple networks.
    let EvalCeBlockStream 
        (blockStream: seq<Vector256<uint8>[]>) 
        (cexses: ce[][]) 
        : (int[] * int)[] =
        
        let numNetworks = cexses.Length
        // Initialize global accumulators for each network
        let networkUseCounts = Array.init numNetworks (fun i -> Array.zeroCreate cexses.[i].Length)
        let networkUnsortedCounts = Array.zeroCreate<int> numNetworks
        
        // We use a lock per network to allow parallel block processing
        let locks = Array.init numNetworks (fun _ -> obj())

        // Process the stream in parallel
        Parallel.ForEach(blockStream, (fun block ->
            // For each block in the stream, test it against every network
            for nIdx = 0 to numNetworks - 1 do
                let network = cexses.[nIdx]
                
                // 1. Clone the block locally for this specific network
                let testBlock = Array.copy block 
                
                // 2. Run the sorting network and track use counts
                let mutable blockWasModified = false
                let localUseCounts = Array.zeroCreate network.Length
                
                for cIdx = 0 to network.Length - 1 do
                    let cex = network.[cIdx]
                    let vLow = testBlock.[cex.Low]
                    let vHi = testBlock.[cex.Hi]
                    let vMin = Vector256.Min(vLow, vHi)
                    let vMax = Vector256.Max(vLow, vHi)
                    
                    if vLow <> vMin then
                        localUseCounts.[cIdx] <- 1
                        testBlock.[cex.Low] <- vMin
                        testBlock.[cex.Hi] <- vMax
                        blockWasModified <- true

                // 3. Post-Check: Is it sorted?
                let isSorted = IsBlockSorted testBlock
                
                // 4. Thread-safe accumulation into global results
                lock locks.[nIdx] (fun () ->
                    if not isSorted then 
                        networkUnsortedCounts.[nIdx] <- networkUnsortedCounts.[nIdx] + 1
                    
                    for cIdx = 0 to network.Length - 1 do
                        if localUseCounts.[cIdx] > 0 then
                            networkUseCounts.[nIdx].[cIdx] <- networkUseCounts.[nIdx].[cIdx] + 1
                )
        )) |> ignore

        // Pair the results back together
        Array.zip networkUseCounts networkUnsortedCounts


    let EvalChunkedBlockStream 
        (chunkedStream: seq<Vector256<uint8>[][]>) 
        (cexses: ce[][]) 
        : (int[] * int64)[] =
        
        let numNetworks = cexses.Length
        let networkUseCounts = Array.init numNetworks (fun i -> Array.zeroCreate cexses.[i].Length)
        let networkUnsortedCounts = Array.zeroCreate<int64> numNetworks
        let locks = Array.init numNetworks (fun _ -> obj())

        Parallel.ForEach(chunkedStream, (fun (chunk: Vector256<uint8> array array) ->
            // Process each network one by one for this chunk
            for nIdx = 0 to numNetworks - 1 do
                let network = cexses.[nIdx]
                let numComparators = network.Length
                
                // Local accumulators for THIS chunk for THIS network
                let localUseCounts = Array.zeroCreate numComparators
                let mutable localUnsortedInChunk = 0L
                
                // Process every block in the chunk
                for bIdx = 0 to chunk.Length - 1 do
                    // Copy original block so other networks get fresh data
                    let testBlock = Array.copy chunk.[bIdx]
                    let mutable blockWasModified = false
                    
                    // Apply sorting network
                    for cIdx = 0 to numComparators - 1 do
                        let cex = network.[cIdx]
                        let vLow = testBlock.[cex.Low]
                        let vHi = testBlock.[cex.Hi]
                        let vMin = Vector256.Min(vLow, vHi)
                        let vMax = Vector256.Max(vLow, vHi)
                        
                        if vLow <> vMin then
                            localUseCounts.[cIdx] <- localUseCounts.[cIdx] + 1
                            testBlock.[cex.Low] <- vMin
                            testBlock.[cex.Hi] <- vMax
                    
                    // Verify if block ended up sorted
                    if not (IsBlockSorted testBlock) then
                        localUnsortedInChunk <- localUnsortedInChunk + 1L

                // Merge chunk results into global state - ONLY ONCE PER CHUNK
                lock locks.[nIdx] (fun () ->
                    networkUnsortedCounts.[nIdx] <- networkUnsortedCounts.[nIdx] + localUnsortedInChunk
                    for cIdx = 0 to numComparators - 1 do
                        networkUseCounts.[nIdx].[cIdx] <- networkUseCounts.[nIdx].[cIdx] + localUseCounts.[cIdx]
                )
        )) |> ignore

        Array.zip networkUseCounts networkUnsortedCounts


    //let evalChunkedWithTypes 
    //    (chunkedStream: seq<Vector256<uint8>[][]>) 
    //    (ceBlocks: ceBlock array) 
    //    : ceBlockEval [] =
        
    //    let numNetworks = ceBlocks.Length
    //    // Use your ceUseCounts type for global storage
    //    let globalUsage = Array.init numNetworks (fun i -> 
    //        ceUseCounts.Create(ceBlocks.[i].Length))
    //    let globalUnsorted = Array.zeroCreate<int> numNetworks
    //    let locks = Array.init numNetworks (fun _ -> obj())

    //    Parallel.ForEach(chunkedStream, (fun (chunk: Vector256<uint8> array array) ->
    //        for nIdx = 0 to numNetworks - 1 do
    //            let ceb = ceBlocks.[nIdx]
    //            let len = %ceb.Length
                
    //            // Local tracking for this chunk
    //            let localCounts = Array.zeroCreate<int> len
    //            let mutable localUnsorted = 0
                
    //            for bIdx = 0 to chunk.Length - 1 do
    //                let testBlock = Array.copy chunk.[bIdx]
    //                for cIdx = 0 to len - 1 do
    //                    let cex = ceb.getCe cIdx
    //                    let vLow = testBlock.[cex.Low]
    //                    let vHi = testBlock.[cex.Hi]
    //                    let vMin = Vector256.Min(vLow, vHi)
                        
    //                    if vLow <> vMin then
    //                        localCounts.[cIdx] <- localCounts.[cIdx] + 1
    //                        testBlock.[cex.Low] <- vMin
    //                        testBlock.[cex.Hi] <- Vector256.Max(vLow, vHi)
                    
    //                if not (IsBlockSorted testBlock) then
    //                    localUnsorted <- localUnsorted + 1

    //            // Sync local results to the ceUseCounts container
    //            lock locks.[nIdx] (fun () ->
    //                globalUnsorted.[nIdx] <- globalUnsorted.[nIdx] + localUnsorted
    //                for i = 0 to len - 1 do
    //                    // Using your custom IncrementBy
    //                    globalUsage.[nIdx].IncrementBy (i |> UMX.tag<ceIndex>) localCounts.[i]
    //            )
    //    )) |> ignore

    //    // Return the high-level ceBlockWithUsage type
    //    Array.init numNetworks (fun i ->
    //        let usageWithBlock = ceBlockEval.create 
    //                                    ceBlocks.[i] 
    //                                    globalUsage.[i]
    //                                    (globalUnsorted.[i] |> UMX.tag<sortableCount>)
    //                                    None
    //        usageWithBlock
    //    )

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
                    let testBlock = Array.copy chunk.[bIdx].Vectors
                    for cIdx = 0 to len - 1 do
                        let cex = ceb.getCe cIdx
                        let vLow = testBlock.[cex.Low]
                        let vHi = testBlock.[cex.Hi]
                        let vMin = Vector256.Min(vLow, vHi)
                        
                        if vLow <> vMin then
                            localCounts.[cIdx] <- localCounts.[cIdx] + 1
                            testBlock.[cex.Low] <- vMin
                            testBlock.[cex.Hi] <- Vector256.Max(vLow, vHi)
                    
                    if not (IsBlockSorted testBlock) then
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



    let evalBp (test: sortableUint8v256Test) (ceBlocks: ceBlock []) : ceBlockEval[] =
        let chunkedStream = test.SimdSortBlocks |> Seq.chunkBySize 64
        evalSimdSortBlockChunks chunkedStream ceBlocks




    ///// Computes a hash for a single lane across all vectors in the block.
    ///// This allows us to check for duplicates without extracting the full array.
    //let inline hashLane (block: Vector256<uint8>[]) (lane: int) =
    //    let mutable h = 17
    //    for i = 0 to block.Length - 1 do
    //        h <- h * 31 + int (block.[i].GetElement(lane))
    //    h

    //let EvalChunkedAndCollectUniqueFailures
    //    (chunkedStream: seq<simdSortBlock[]>) 
    //    (ceBlocks: ceBlock array) 
    //    : ceBlockEval [] =
        
    //    let numNetworks = ceBlocks.Length
    //    let globalUsage = Array.init numNetworks (fun i -> ceUseCounts.Create(ceBlocks.[i].Length))
    //    let globalUnsorted = Array.zeroCreate<int> numNetworks
        
    //    // A set of unique failing sequences (int[]) per network
    //    // We use a custom comparer to ensure int[] equality is based on content
    //    let failureSets = Array.init numNetworks (fun _ -> 
    //        ConcurrentDictionary<int[], byte>(ArrayEqualityComparer<int>()))
        
    //    let locks = Array.init numNetworks (fun _ -> obj())

    //    Parallel.ForEach(chunkedStream, (fun chunk ->
    //        for nIdx = 0 to numNetworks - 1 do
    //            let ceb = ceBlocks.[nIdx]
    //            let networkLen = %ceb.Length
    //            let localCounts = Array.zeroCreate<int> networkLen
    //            let mutable localUnsortedCount = 0
                
    //            for bIdx = 0 to chunk.Length - 1 do
    //                let testBlock = Array.copy chunk.[bIdx].Vectors
                    
    //                // ... [Standard CAS Sorting Logic] ...

    //                // Check if the block is sorted
    //                if not (IsBlockSorted testBlock) then
    //                    localUnsortedCount <- localUnsortedCount + 1
                        
    //                    // Check each of the 32 lanes
    //                    for lane = 0 to 31 do
    //                        // We only extract and add if the lane is actually unsorted
    //                        // A block being "unsorted" only means AT LEAST one lane failed.
    //                        let mutable laneSorted = true
    //                        let mutable vIdx = 0
    //                        while laneSorted && vIdx < testBlock.Length - 1 do
    //                            if testBlock.[vIdx].GetElement(lane) > testBlock.[vIdx+1].GetElement(lane) then
    //                                laneSorted <- false
    //                            vIdx <- vIdx + 1
                            
    //                        if not laneSorted then
    //                            // Extract the lane to an array
    //                            let laneArray = Array.init testBlock.Length (fun i -> 
    //                                int (testBlock.[i].GetElement(lane)))
                                
    //                            // TryAdd acts as our Set "Insert"
    //                            failureSets.[nIdx].TryAdd(laneArray, 0uy) |> ignore

    //            lock locks.[nIdx] (fun () ->
    //                globalUnsorted.[nIdx] <- globalUnsorted.[nIdx] + localUnsortedCount
    //                for i = 0 to networkLen - 1 do
    //                    globalUsage.[nIdx].IncrementBy (i |> UMX.tag<ceIndex>) localCounts.[i]
    //            )
    //    )) |> ignore

    //    // Assemble ceBlockEval
    //    Array.init numNetworks (fun i ->
    //        let uniqueFailures = failureSets.[i].Keys |> Seq.toArray
    //        // Map uniqueFailures (int[][]) into your sortableTest.Ints or similar here
    //        let failTest = None // TODO: Wrap uniqueFailures

    //        ceBlockEval.create 
    //            ceBlocks.[i] 
    //            globalUsage.[i] 
    //            (globalUnsorted.[i] |> UMX.tag<sortableCount>) 
    //            failTest
    //    )



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
                    if not (IsBlockSorted testVecs) then
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


    let evalAndCollectResults (test: sortableUint8v256Test) (ceBlocks: ceBlock []) : ceBlockEval[] =
        let chunkedStream = test.SimdSortBlocks |> Seq.chunkBySize 64
        evalChunkedAndCollectUniqueFailures chunkedStream ceBlocks