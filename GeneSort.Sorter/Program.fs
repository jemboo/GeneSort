open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter



let sortingWidth = 8<sortingWidth>
//let offset = 0<sortingWidth>

//let bs = BitonicSorter2.bitonicMerge1 offset sortingWidth
//let bs = BitonicSorter.bitonicSort2 sortingWidth

//for i in 0 .. bs.Length - 1 do
//printfn "%A \n" bs[i]

//let network = BitonicSorter.generateBitonicStageSequence (16 |> UMX.tag<sortingWidth>)
//printfn "%A" network

let mergeCases = SortableIntArray.getIntArrayMerge2Cases sortingWidth

let boolArrays = mergeCases |> Array.map(fun mc -> mc.ToSortableBoolArrays()) |> Array.concat

let distincto = boolArrays |> Array.map(fun sba -> sba.Values)  |> Set.ofArray

let wak = distincto |> Set.toArray
for i = 0 to wak.Length - 1 do
    printfn "%A" wak.[i]

let k = mergeCases
