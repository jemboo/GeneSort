namespace GeneSort.Sorting.Sortable

open System
open System.Runtime.Intrinsics
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

module GrayVectorGenerator =

    /// Generates the base block where bits 0-63 are a Gray code sequence.
    /// This fills the 64 lanes of the Vector512.
    let private generateInitialSeedBlock (sw: int<sortingWidth>) : Vector512<uint8>[] =
            let width = int sw
            let vectors = Array.init width (fun _ -> Array.zeroCreate<uint8> 64)
        
            for lane = 0 to 63 do
                let laneUL = uint64 lane
                // Use the explicit bitwise shift if the operator is being shadowed
                let gray = laneUL ^^^ (laneUL >>> 1) 
            
                for wire = 0 to width - 1 do
                    let mask = 1uL <<< wire
                    if (gray &&& mask) <> 0uL then
                        vectors.[wire].[lane] <- 255uy
                    else
                        vectors.[wire].[lane] <- 0uy
        
            vectors |> Array.map (fun v -> Vector512.Create<uint8>(v))


    /// Emits a sequence of tests, each containing one block of 64 parallel test cases.
    let getAllSortableUint8v512TestForSortingWidth 
        (sw: int<sortingWidth>) : seq<sortableUint8v512Test> =
        
        let width = int sw
        // 2^width total cases / 64 cases per block
        let totalBlocks = 1uL <<< (max 0 (width - 6))

        seq {
            // Start with the seed block (the first 64 Gray codes)
            let currentVectors = generateInitialSeedBlock sw
            
            for blockIdx = 0uL to totalBlocks - 1uL do
                // 1. Create the SIMD block for the current state
                // We must copy the vectors because we mutate currentVectors for the next step
                let block = simd512SortBlock.create sw (Array.copy currentVectors) 64
                
                let testId = Guid.NewGuid() |> UMX.tag<sorterTestId>
                yield sortableUint8v512Test.create testId sw [| block |]

                // 2. Compute the flip for the NEXT block
                // In a Gray sequence of blocks, we flip the bit corresponding 
                // to the lowest set bit of (blockIdx + 1)
                if blockIdx < totalBlocks - 1uL then
                    let mutable c = blockIdx + 1uL
                    let mutable wireToFlip = 6 // Bits 0-5 are handled by the 64 lanes
                    while (c &&& 1uL) = 0uL && wireToFlip < width do
                        c <- c >>> 1
                        wireToFlip <- wireToFlip + 1
                    
                    if wireToFlip < width then
                        let flipMask = Vector512<uint8>.AllBitsSet
                        currentVectors.[wireToFlip] <- currentVectors.[wireToFlip] ^^^ flipMask
        }


