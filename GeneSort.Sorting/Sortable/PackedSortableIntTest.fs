
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting

[<Struct>]
type packedSortableIntTests =
    private { id: Guid<sortableTestsId>
              sortingWidth: int<sortingWidth>
              count: int<sortableCount>
              // All arrays flattened into one: [Test0_0..Test0_N, Test1_0..Test1_N...]
              packedValues: int[] }

    static member create
                    (sw: int<sortingWidth>)
                    (arrays: sortableIntArray[]) =

        let n = arrays.Length
        let width = %sw
        let flat = Array.zeroCreate (n * width)
        
        // Pack the data into the flat array
        for i = 0 to n - 1 do
            let destinationOffset = i * width
            for j = 0 to width - 1 do
                flat.[destinationOffset + j] <- arrays.[i].Item(j)
            
        { id = Guid.NewGuid() |> UMX.tag
          sortingWidth = sw
          count = n |> UMX.tag<sortableCount>
          packedValues = flat }


    static member createFromPackedValues
                    (sw: int<sortingWidth>)
                    (packedValues: int[]) =
        let width = %sw
        if packedValues.Length % width <> 0 then
            invalidArg "packedValues" "Length of packedValues must be a multiple of sortingWidth."
        let n = packedValues.Length / width
        { id = Guid.NewGuid() |> UMX.tag
          sortingWidth = sw
          count = n |> UMX.tag<sortableCount>
          packedValues = Array.copy packedValues }

    
    member this.Id with get() = this.id
    member this.PackedValues with get() = this.packedValues
    member this.SortableDataFormat with get() = sortableDataFormat.PackedIntArray
    member this.SortableCount with get() = this.count
    member this.SortingWidth with get() = this.sortingWidth

 
