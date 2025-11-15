open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter



let sortingWidth = 32<sortingWidth>
let offset = 0<sortingWidth>

//let wak = BitonicSorter2.bitonicMerge1 offset sortingWidth
let wak = BitonicSorter.bitonicSort2 sortingWidth

for i in 0 .. wak.Length - 1 do
printfn "%A \n" wak[i]

//let network = BitonicSorter.generateBitonicStageSequence (16 |> UMX.tag<sortingWidth>)
//printfn "%A" network
