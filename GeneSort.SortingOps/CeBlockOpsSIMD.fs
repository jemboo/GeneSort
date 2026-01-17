namespace GeneSort.SortingOps

open System.Runtime.CompilerServices
open System.Runtime.Intrinsics
open System.Threading.Tasks
open GeneSort.Sorter.Sorter


module CeBlockOpsSIMD256 =

    /// Checks if a block is vertically sorted across all SIMD lanes.
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    let IsBlockSorted (block: Vector256<uint32> []) : bool =
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
    /// This is memory-efficient for "Very Large" datasets.
    let BenchmarkStream 
        (blockStream: seq<Vector256<uint32>[]>) 
        (cexses: ce[][]) 
        : (int[] * int64)[] =
        
        let numNetworks = cexses.Length
        // Initialize global accumulators for each network
        let networkUseCounts = Array.init numNetworks (fun i -> Array.zeroCreate cexses.[i].Length)
        let networkUnsortedCounts = Array.zeroCreate<int64> numNetworks
        
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
                        networkUnsortedCounts.[nIdx] <- networkUnsortedCounts.[nIdx] + 1L
                    
                    for cIdx = 0 to network.Length - 1 do
                        if localUseCounts.[cIdx] > 0 then
                            networkUseCounts.[nIdx].[cIdx] <- networkUseCounts.[nIdx].[cIdx] + 1
                )
        )) |> ignore

        // Pair the results back together
        Array.zip networkUseCounts networkUnsortedCounts