open GeneSort.Sorter

let yab () = 
    let sortingWidth = 10
    let ceCount = 100000000
    for i in 0 .. ceCount - 1 do
        let ce = Ce.fromIndex i
        if Ce.toIndex ce <> i then
            printfn " *********************  Error at index %d: expected %d, got %d" i i (Ce.toIndex ce)  
        //printfn "Ce %d: %s  %d" i (GeneSort.Sorter.Ce.toString ce)  (Ce.toIndex ce)

    printfn "done"
// For more information see https://aka.ms/fsharp-console-apps
(yab())
