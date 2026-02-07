

namespace GeneSort.Sorting

open System
open FSharp.UMX
open GeneSort.Sorting.Sortable

module Program =

    [<EntryPoint>]
    let main argv =
        let stWidth = 11<sortingWidth>

        // Keep this as a sequence if possible, or an array of blocks
        let grayBlocks = Sortable.GrayVectorGenerator.getAllSortBlockBitv512ForSortingWidth stWidth

        let mutable totalCount = 0
    
        // Process block-by-block to avoid allocating the massive 'sortableInts' array
        grayBlocks |> Seq.iter (fun block ->
            let intsInBlock = SortBlockBitv512.toSortableIntArrays block
            totalCount <- totalCount + intsInBlock.Length
        
            for i in 0 .. (intsInBlock.Length - 1) do
                printfn "%s" (intsInBlock.[i] |> SortableIntArray.toString)
        )

        printfn "Total Length: %d" totalCount
        0 // Return exit code
