namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open System.Linq
open System.Collections.Generic



module CeOps = 


   // mutates in placeby a sequence of ces, and returns the resulting sortable (values[]),
   // records the number of uses of each ce in useCounter, starting at useCounterOffset
    let inline sortBy< ^a when ^a: comparison> 
                (ceBlock: ceBlock) 
                (useCounter: int[])
                (values: ^a[]) : ^a[] =

        for i = 0 to %ceBlock.Length - 1 do
            let ce = ceBlock.Ces.[i]
            if values.[ce.Low] > values.[ce.Hi] then
                let temp = values.[ce.Low]
                values.[ce.Low] <- values.[ce.Hi]
                values.[ce.Hi] <- temp
                useCounter.[i] <- useCounter.[i] + 1
        values


    //let sortBy2
    //    (ceBlockEval: ceBlockEval)  : ceBlockEval =

    //    match ceBlockEval.SorterTests with
    //    | sorterTests.Ints sits -> 
            
    //        let yab = sits.SortableArrays
    //        for sortableArray in sits.SortableArrays do
    //            let _ = sortBy ceBlockEval.CeBlock ceBlockEval.CeBlockUsage.Values sortableArray.Values  |> ignore
            
    //        () |> ignore

    //    ceBlockEval




   //// mutates in placeby a sequence of ces, and returns the resulting sortable (values[]),
   //// records the number of uses of each ce in useCounter, starting at useCounterOffset
   // let inline sortBy2< ^a when ^a: comparison> 
   //             (sorterTests: sorterTests)
   //             (ceBlock: ceBlock)
   //             (values: ^a[]) : ceBlockEval =

   //     let ceBlockEval = ceBlockEval.create(ceBlock, sorterTests)
   //     match sorterTests with
   //     | sorterTests.Ints sits -> 
        
   //         for sortableArray in sits.SortableArrays do
   //             let _ = sortBy ceBlock ceBlockEval.CeBlockUsage sortableArray.Values  |> ignore
        
   //         () |> ignore
   //     | sorterTests.Bools sbts -> 
        
   //         () |> ignore
        
   //     ceBlockEval

   //     //for i = 0 to %ceBlock.Length - 1 do
   //     //    let ce = ceBlock.Ces.[i]
   //     //    if values.[ce.Low] > values.[ce.Hi] then
   //     //        let temp = values.[ce.Low]
   //     //        values.[ce.Low] <- values.[ce.Hi]
   //     //        values.[ce.Hi] <- temp
   //     //        useCounter.[i] <- useCounter.[i] + 1
         
