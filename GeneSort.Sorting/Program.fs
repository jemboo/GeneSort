

namespace GeneSort.Sorting

open System
open FSharp.UMX
open GeneSort.Sorting.Sortable

module Program =

    [<EntryPoint>]
    let main argv =
        let stWidth = 13<sortingWidth>

        let yab = Sortable.GrayVectorGenerator.getAllSortableUint8v512TestForSortingWidth stWidth |> Seq.toArray
        let sortableInts = 
            yab |> Array.map (fun su -> 
                su.SimdSortBlocks |> Array.collect (fun block -> 
                    Sortable.Simd512SortBlock.toSortableIntArrays block))
                    |> Array.concat

        printfn "Length: %d" sortableInts.Length

        for i in 0 .. (sortableInts.Length - 1) do
            printfn "%s" (sortableInts.[i] |> SortableIntArray.toString)

        0 // return an integer exit code
