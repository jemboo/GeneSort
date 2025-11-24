open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Core


let sortingWidth = 12<sortingWidth>

let mergeCases = SortableIntArray.getIntArrayMerge3Cases sortingWidth

let plainArrays = mergeCases |> Array.map(fun mc -> mc.Values)
for i = 0 to plainArrays.Length - 1 do
    printfn "%A" plainArrays.[i]

printfn "---------------------"

let boolArrays = mergeCases |> Array.map(fun mc -> mc.ToSortableBoolArrays()) |> Array.concat

let distincto = boolArrays |> Array.map(fun sba -> sba.Values)  |> Set.ofArray

let wak = distincto |> Set.toArray
for i = 0 to wak.Length - 1 do
    printfn "%A" wak.[i]

let k = mergeCases



//let mergeCases = SortableIntArray.getMerge2TestCases sortingWidth

//let boolArrays = mergeCases |> Array.map(fun mc -> mc.ToSortableBoolArrays()) |> Array.concat

//let distincto = boolArrays |> Array.map(fun sba -> sba.Values)  |> Set.ofArray

//let wak = distincto |> Set.toArray
//for i = 0 to wak.Length - 1 do
//    printfn "%A" wak.[i]

//let k = mergeCases
