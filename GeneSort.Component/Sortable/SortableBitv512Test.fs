namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open System.Threading.Tasks
open System.Runtime.Intrinsics

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



module SortableBitv512Test =

    let fromBoolArrays
        (id: Guid<sorterTestId>)
        (sw: int<sortingWidth>) 
        (allTests: sortableBoolArray[]) : sortableBitv512Test =
        
        let width = %sw
        // Chunk the input arrays into groups of 512
        let blocks = 
            allTests 
            |> Array.chunkBySize 512 
            |> Array.map (fun chunk ->
                let inputCount = chunk.Length
                
                // Map wires across the bit-lanes
                let vecs = Array.init width (fun wireIdx ->
                    let buffer = Array.zeroCreate<uint64> 8
                    
                    for testIdx = 0 to inputCount - 1 do
                        // Direct bool check is faster than int conversion
                        if chunk.[testIdx].Values.[wireIdx] then
                            let lane = testIdx / 64
                            let bit = testIdx % 64
                            buffer.[lane] <- buffer.[lane] ||| (1uL <<< bit)
                    
                    Vector512.Create(
                        buffer.[0], buffer.[1], buffer.[2], buffer.[3],
                        buffer.[4], buffer.[5], buffer.[6], buffer.[7]
                    )
                )
                sortBlockBitv512.createFromVectors vecs inputCount
            )

        sortableBitv512Test.create id sw blocks