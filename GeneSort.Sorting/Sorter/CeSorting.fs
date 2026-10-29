
namespace GeneSort.Sorting.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

module CeSorting =

    let inline sortBy< ^a when ^a: comparison> 
                (ces: ce[]) 
                (values: ^a[]) : ^a[] =

        for i = 0 to ces.Length - 1 do
            let ce = ces.[i]
            let lowIdx = (int ce.LowChannel)
            let hiIdx = (int ce.HighChannel)
            
            if values.[lowIdx] > values.[hiIdx] then
                let temp = values.[lowIdx]
                values.[lowIdx] <- values.[hiIdx]
                values.[hiIdx] <- temp
        values


    let inline sortByBranchlessWithUses 
            (ces: ce[]) 
            (useCounter: int[]) 
            (values: int[]) : int[] =

        for i = 0 to ces.Length - 1 do
            let ce = ces.[i]
            let a = values.[(int ce.LowChannel)]
            let b = values.[(int ce.HighChannel)]
        
            // Tracking "uses" still requires a branch, but we can make it branchless too!
            let diff = if a > b then 1 else 0
            useCounter.[i] <- useCounter.[i] + diff
        
            let mask = -diff
            let t = (a ^^^ b) &&& mask
            values.[(int ce.LowChannel)] <- a ^^^ t
            values.[(int ce.HighChannel)] <- b ^^^ t
        values


   // mutates in placeby a sequence of ces, and returns the resulting sortable (values[]),
   // records the number of uses of each ce in useCounter, starting at useCounterOffset
    let inline sortByWithUses< ^a when ^a: comparison> 
                (ces: ce[]) 
                (useCounter: int[])
                (values: ^a[]) : ^a[] =

        for i = 0 to ces.Length - 1 do
            let ce = ces.[i]
            if values.[(int ce.LowChannel)] > values.[(int ce.HighChannel)] then
                let temp = values.[(int ce.LowChannel)]
                values.[(int ce.LowChannel)] <- values.[(int ce.HighChannel)]
                values.[(int ce.HighChannel)] <- temp
                useCounter.[i] <- useCounter.[i] + 1
        values


   // mutates in placeby a sequence of ces, returning an array of the final and
   // intermediate results (values[][]) 
   // records the number of uses of each ce in useCounter, starting at useCounterOffset
    let inline sortByWithHistoryAndUses< ^a when ^a: comparison> 
                (ces: ce[]) 
                (useCounter: int[])
                (values: ^a[]) : ^a[][] =
        let result = Array.init (ces.Length + 1) (fun _ -> Array.copy values)
        for i = 0 to ces.Length - 1 do
            let ce = ces.[i]
            result.[i + 1] <- Array.copy result.[i]
            if result.[i + 1].[(int ce.LowChannel)] > result.[i + 1].[(int ce.HighChannel)] then
                let temp = result.[i + 1].[(int ce.LowChannel)]
                result.[i + 1].[(int ce.LowChannel)] <- result.[i + 1].[(int ce.HighChannel)]
                result.[i + 1].[(int ce.HighChannel)] <- temp
                useCounter.[i] <- useCounter.[i] + 1
        result

