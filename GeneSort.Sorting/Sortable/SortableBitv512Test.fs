namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting

[<Struct>]
type sortableBitv512Test =
    private { 
        id: Guid<sorterTestId>
        sortingWidth: int<sortingWidth>
        simdSortBlocks: sortBlockBitv512[] 
    }

    static member create 
            (id: Guid<sorterTestId>) 
            (sw: int<sortingWidth>) 
            (blocks: sortBlockBitv512[]) =
        { id = id
          sortingWidth = sw
          simdSortBlocks = blocks }

    static member Empty =
        let id = Guid.NewGuid() |> UMX.tag<sorterTestId>
        sortableBitv512Test.create id 0<sortingWidth> [||]

    member this.Id with get() = this.id

    member this.SimdSortBlocks with get() = this.simdSortBlocks
    
    member this.SortableDataFormat with get() = sortableDataFormat.BitVector512
    
    member this.SoratbleCount with get() = 
        this.simdSortBlocks 
        |> Array.sumBy (fun block -> block.SortableCount) 
        |> UMX.tag<sortableCount>
        
    member this.SortingWidth with get() = this.sortingWidth


//module SortableBitv512Test =

//    let fromIntArrays
//        (id: Guid<sorterTestId>)
//        (sw: int<sortingWidth>) 
//        (allTests: sortableIntArray[]) 
//                    : sortableBitv512Test =

//        let totalTests = allTests.Length
        
//        // --- 64 Lanes per block for Vector512 ---
//        let totalBlocksCount = (totalTests + 63) / 64
//        let blocks = Array.zeroCreate<simd512SortBlock> totalBlocksCount

//        // Parallel Pack: use all cores to build the 64-lane blocks
//        Parallel.For(0, totalBlocksCount, (fun i ->
//            let startIdx = i * 64
//            let length = min 64 (totalTests - startIdx)
            
//            // Extract the slice for this block
//            let slice = Array.sub allTests startIdx length
//            blocks.[i] <- simd512SortBlock.createFromIntArrays sw slice
//        )) |> ignore

//        sortableBitv512Test.create id sw blocks