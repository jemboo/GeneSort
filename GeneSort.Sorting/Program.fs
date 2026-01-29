

namespace GeneSort.Sorting

open System
open FSharp.UMX
open GeneSort.Sorting.Sortable

module Program =

    [<EntryPoint>]
    let main argv =
        let stWidth = 11<sortingWidth>

        let grayBlocks = Sortable.GrayVectorGenerator.getAllSortBlockBitv512ForSortingWidth stWidth |> Seq.toArray
        let sortableInts = 
                grayBlocks 
                |> Array.map (fun su -> 
                    su |> SortBlockBitv512.toSortableIntArrays)
                |> Array.concat

        printfn "Length: %d" sortableInts.Length

        for i in 0 .. (sortableInts.Length - 1) do
            printfn "%s" (sortableInts.[i] |> SortableIntArray.toString)

        0 // return an integer exit code
