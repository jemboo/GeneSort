open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Core

for mc in Combinatorics.latticeLevelSetVV 3 24 do
        printfn "%A" mc



//let partitionPoints = [| 3; 6 |]

//let mergeCases = SortableIntArray.getMerge3TestCases sortingWidth

//let plainArrays = mergeCases |> Array.map(fun mc -> mc.Values)
//for i = 0 to plainArrays.Length - 1 do
//    printfn "%A" plainArrays.[i]

//printfn "---------------------"

//let boolArrays = mergeCases |> Array.map(fun mc -> mc.ToSortableBoolArrays()) |> Array.concat

//let distincto = boolArrays |> Array.map(fun sba -> sba.Values)  |> Set.ofArray

//let wak = distincto |> Set.toArray
//for i = 0 to wak.Length - 1 do
//    printfn "%A" wak.[i]


//let latticePoints = 
//    boolArrays 
//    |> Array.map(fun sba -> 
//        let ssa = 
//            SubSortedArray.create sba.Values partitionPoints
//            //SubSortedArray.create (sba.Values |> Array.map(fun b -> if b then 1 else 0)) partitionPoints
//        ssa.GetLatticePoint(true)
//    ) |> Set.ofArray |> Set.toArray


//for i = 0 to latticePoints.Length - 1 do
//    printfn "%A" latticePoints.[i]



//let k = mergeCases



//let mergeCases = SortableIntArray.getMerge2TestCases sortingWidth

//let boolArrays = mergeCases |> Array.map(fun mc -> mc.ToSortableBoolArrays()) |> Array.concat

//let distincto = boolArrays |> Array.map(fun sba -> sba.Values)  |> Set.ofArray

//let wak = distincto |> Set.toArray
//for i = 0 to wak.Length - 1 do
//    printfn "%A" wak.[i]

//let k = mergeCases
