
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter

[<Struct>]
type packedSortableIntTests =
    private { id: Guid<sortableTestsId>
              sortingWidth: int<sortingWidth>
              count: int<sortableCount>
              unsortedCount: int<sortableCount>
              // All arrays flattened into one: [Test0_0..Test0_N, Test1_0..Test1_N...]
              packedValues: int[] }

    static member create
                    (sw: int<sortingWidth>)
                    (arrays: sortableIntArray[])
                    (unsortedCount: int<sortableCount>) =

        let n = arrays.Length
        let width = %sw
        let flat = Array.zeroCreate (n * width)
        
        // Pack the data into the flat array
        for i = 0 to n - 1 do
            let source = arrays.[i].Values
            let destinationOffset = i * width
            Array.blit source 0 flat destinationOffset width
            
        { id = Guid.NewGuid() |> UMX.tag
          sortingWidth = sw
          count = n |> UMX.tag<sortableCount>
          unsortedCount = unsortedCount
          packedValues = flat }


    static member createFromPackedValues
                    (sw: int<sortingWidth>)
                    (packedValues: int[])
                    (unsortedCount: int<sortableCount>) =
        let width = %sw
        if packedValues.Length % width <> 0 then
            invalidArg "packedValues" "Length of packedValues must be a multiple of sortingWidth."
        let n = packedValues.Length / width
        { id = Guid.NewGuid() |> UMX.tag
          sortingWidth = sw
          count = n |> UMX.tag<sortableCount>
          unsortedCount = unsortedCount
          packedValues = Array.copy packedValues }


    member this.PackedValues with get() = this.packedValues
    member this.SortableArrayType with get() = sortableDataType.PackedInts
    member this.SoratbleCount with get() = this.count
    member this.SortingWidth with get() = this.sortingWidth
    member this.UnsortedCount with get() = this.unsortedCount

    member this.Id with get() = this.id
 