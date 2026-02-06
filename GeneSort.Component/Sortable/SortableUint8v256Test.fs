
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open System.Threading.Tasks

[<Struct>]
type sortableUint8v256Test =
    private { 
        id: Guid<sorterTestId>
        sortingWidth: int<sortingWidth>
        simdSortBlocks: SortBlockUint8v256[]
    }

    static member create 
            (id: Guid<sorterTestId>) 
            (sw: int<sortingWidth>) 
            (blocks: SortBlockUint8v256[]) =
        { id = id
          sortingWidth = sw
          simdSortBlocks = blocks }

    static member Empty =
        let id = Guid.NewGuid() |> UMX.tag<sorterTestId>
        sortableUint8v256Test.create id 0<sortingWidth> [||]

    member this.Id with get() = this.id
    member this.SimdSortBlocks with get() = this.simdSortBlocks
    member this.SortableDataFormat with get() = sortableDataFormat.Int8Vector256
    member this.SoratbleCount with get() = 
        this.simdSortBlocks 
        |> Array.sumBy (fun block -> block.SortableCount) 
        |> UMX.tag<sortableCount>
    member this.SortingWidth with get() = this.sortingWidth



module SortableUint8v256Test =

    let fromIntArrays
        (id: Guid<sorterTestId>)
        (sw: int<sortingWidth>)
        (allTests: sortableIntArray[]) 
                    : sortableUint8v256Test =
        
        let totalTests = allTests.Length
        // Calculate how many SIMD blocks we need (32 lanes per block)
        let totalBlocksCount = (totalTests + 31) / 32
        let blocks = Array.zeroCreate<SortBlockUint8v256> totalBlocksCount

        // Parallel Pack: use all cores to build the blocks
        Parallel.For(0, totalBlocksCount, (fun i ->
            let startIdx = i * 32
            let length = min 32 (totalTests - startIdx)
            // Extract the slice for this block
            let slice = Array.sub allTests startIdx length
            blocks.[i] <- SortBlockUint8v256.createFromIntArrays sw slice
        )) |> ignore

        sortableUint8v256Test.create id sw blocks
