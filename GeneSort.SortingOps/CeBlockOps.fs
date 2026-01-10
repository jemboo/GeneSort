namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter.Sortable
open System.Collections.Generic
open System.Buffers

module CeBlockOps = 


   // mutates in placeby a sequence of ces, and returns the resulting sortable (values[]),
   // records the number of uses of each ce in useCounter, starting at useCounterOffset
    let inline sortBy< ^a when ^a: comparison> 
                (ceBlock: ceBlock) 
                (ceUseCounts: ceUseCounts)
                (values: ^a[]) : ^a[] =

        for i = 0 to %ceBlock.Length - 1 do
            let ce = ceBlock.getCe i
            if values.[ce.Low] > values.[ce.Hi] then
                let temp = values.[ce.Low]
                values.[ce.Low] <- values.[ce.Hi]
                values.[ce.Hi] <- temp
                ceUseCounts.Increment i 1
        values


    let inline sortByBranchless 
            (ceBlock: ceBlock) 
            (ceUseCounts: ceUseCounts)
            (values: int[]) : int[] =

        for i = 0 to %ceBlock.Length - 1 do
            let ce = ceBlock.getCe i
            let a = values.[ce.Low]
            let b = values.[ce.Hi]
        
            // Tracking "uses" still requires a branch, but we can make it branchless too!
            let diff = if a > b then 1 else 0
            ceUseCounts.Increment i diff
        
            let mask = -diff
            let t = (a ^^^ b) &&& mask
            values.[ce.Low] <- a ^^^ t
            values.[ce.Hi] <- b ^^^ t
        values



    let evalWithSorterTest (sortableTests: sortableTests) (ceBlock: ceBlock) =
        let counts = ceUseCounts.create ceBlock.Length
        let ces = ceBlock.CeArray
    
        match sortableTests with
        | Ints sits ->
            let sw = sits.SortingWidth
            let pool = ArrayPool<int>.Shared
            let results = HashSet<sortableIntArray>(SortableIntArray.SortableIntArrayValueComparer())

            for sia in sits.SortableIntArrays do
                let workArray = pool.Rent(%sw)
                Array.blit sia.Values 0 workArray 0 %sw

                for i = 0 to ces.Length - 1 do
                    let ce = ces.[i]
                    let a = workArray.[ce.Low]
                    let b = workArray.[ce.Hi]
                    if a > b then
                        workArray.[ce.Low] <- b
                        workArray.[ce.Hi] <- a
                        counts.Increment i 1

                let isSorted = 
                    let mutable ok = true
                    let mutable j = 0
                    while j < %sw - 1 && ok do
                        if workArray.[j] > workArray.[j+1] then ok <- false
                        else j <- j + 1
                    ok

                if not isSorted then
                    let finalValues = Array.zeroCreate %sw
                    Array.blit workArray 0 finalValues 0 %sw
                    let resultSia = sortableIntArray.create(finalValues, sw, sia.SymbolSetSize)
                    results.Add(resultSia) |> ignore

                pool.Return(workArray)

            let newTests = Seq.toArray results |> sortableIntTests.create (Guid.NewGuid() |> UMX.tag) sw |> Ints
            ceBlockEval.create (ceBlockWithUsage.create ceBlock (counts.UseCounts)) newTests

        | Bools sbts ->
            let sw = sbts.SortingWidth
            let pool = ArrayPool<bool>.Shared
            // Ensure SortableBoolArrayValueComparer is defined similarly to the Int version
            let results = HashSet<sortableBoolArray>(SortableBoolArray.SortableBoolArrayValueComparer())

            for sba in sbts.SortableBoolArrays do
                let workArray = pool.Rent(%sw)
                Array.blit sba.Values 0 workArray 0 %sw

                for i = 0 to ces.Length - 1 do
                    let ce = ces.[i]
                    // Boolean Comparison: true (1) > false (0)
                    if workArray.[ce.Low] && not workArray.[ce.Hi] then
                        workArray.[ce.Low] <- false
                        workArray.[ce.Hi] <- true
                        counts.Increment i 1

                // A bool array is sorted if it's in the form [false, false, ..., true, true]
                let isSorted = 
                    let mutable ok = true
                    let mutable j = 0
                    while j < %sw - 1 && ok do
                        // Check if we have a 'true' followed by a 'false'
                        if workArray.[j] && not workArray.[j+1] then ok <- false
                        else j <- j + 1
                    ok

                if not isSorted then
                    let finalValues = Array.zeroCreate %sw
                    Array.blit workArray 0 finalValues 0 %sw
                    let resultSba = sortableBoolArray.Create(finalValues, sw)
                    results.Add(resultSba) |> ignore

                pool.Return(workArray)

            let newTests = Seq.toArray results |> sortableBoolTests.create (Guid.NewGuid() |> UMX.tag) sw |> Bools
            ceBlockEval.create (ceBlockWithUsage.create ceBlock (counts.UseCounts)) newTests