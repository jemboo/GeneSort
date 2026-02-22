namespace GeneSort.SortingOps

open System
open FSharp.UMX
open System.Collections.Generic
open System.Buffers    
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

module CeBlockOpsInt = 


    let eval (sits: sortableIntTest) (ceBlock: ceBlock) =
            let ceUseCounts = ceUseCounts.Create ceBlock.CeLength
            let mutable unsortedCount = 0
            let ces = ceBlock.CeArray
            let sw = sits.SortingWidth
            let pool = ArrayPool<int>.Shared

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
                        ceUseCounts.Increment (i |> UMX.tag<ceIndex>) 

                let isSorted = 
                    let mutable ok = true
                    let mutable j = 0
                    while j < %sw - 1 && ok do
                        if workArray.[j] > workArray.[j+1] then ok <- false
                        else j <- j + 1
                    ok

                if not isSorted then
                    unsortedCount <- unsortedCount + 1

                pool.Return(workArray)

            ceBlockEval.create ceBlock ceUseCounts (unsortedCount |> UMX.tag<sortableCount>) None



    let evalAndCollectNewSortableTests (sits: sortableIntTest) (ceBlock: ceBlock) =
            let ceUseCounts = ceUseCounts.Create ceBlock.CeLength
            let ces = ceBlock.CeArray
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
                        ceUseCounts.Increment (i |> UMX.tag<ceIndex>) 

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

            let newTests = Seq.toArray results |> sortableIntTest.create (Guid.NewGuid() |> UMX.tag) sw
            ceBlockEval.create 
                        ceBlock 
                        ceUseCounts 
                        (results.Count |> UMX.tag<sortableCount>) 
                        (Some (sortableTest.Ints newTests))



    let evalAndDedupeCeFetch (sits: sortableIntTest) (ceBlock: ceBlock) =
            let ceUseCounts = ceUseCounts.Create ceBlock.CeLength
            let lows = Array.init %ceBlock.CeLength (fun i -> ceBlock.CeArray.[i].Low)
            let highs = Array.init %ceBlock.CeLength (fun i -> ceBlock.CeArray.[i].Hi)
            let sw = sits.SortingWidth
            let pool = ArrayPool<int>.Shared
            let results = HashSet<sortableIntArray>(SortableIntArray.SortableIntArrayValueComparer())

            for sia in sits.SortableIntArrays do
                let workArray = pool.Rent(%sw)
                Array.blit sia.Values 0 workArray 0 %sw

                // HOT LOOP: Logic reduced to simple primitive array lookups
                for i = 0 to %ceBlock.CeLength - 1 do
                    let lIdx = lows.[i]
                    let hIdx = highs.[i]
                    let a = workArray.[lIdx]
                    let b = workArray.[hIdx]
                    if a > b then
                        workArray.[lIdx] <- b
                        workArray.[hIdx] <- a
                        ceUseCounts.Increment (i |> UMX.tag<ceIndex>)

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

            let newTests = Seq.toArray results |> sortableIntTest.create (Guid.NewGuid() |> UMX.tag) sw
            ceBlockEval.create 
                        ceBlock 
                        ceUseCounts 
                        (results.Count |> UMX.tag<sortableCount>) 
                        (Some (sortableTest.Ints newTests))



    let evalAndDedupeUnsafe (sits: sortableIntTest) (ceBlock: ceBlock) =
        let ces = ceBlock.CeArray
        let ceLen = ces.Length |> UMX.tag<ceLength>
        let ceUseCounts = ceUseCounts.Create ceLen
        let sw = %sits.SortingWidth
        let pool = ArrayPool<int>.Shared
        let results = HashSet<sortableIntArray>(SortableIntArray.SortableIntArrayValueComparer())
        
        // POINT 2: Hoist Renting outside the loop
        let workArray = pool.Rent(sw)
        let workSpan = workArray.AsSpan(0, sw)
        // Get a reference to the first element for Unsafe access
        let baseRef = &MemoryMarshal.GetReference(workSpan)

        for sia in sits.SortableIntArrays do
            // Skip logic: if already sorted, don't bother processing
            if not sia.IsSorted then
                sia.Values.AsSpan().CopyTo(workSpan)

                // POINT 1: Inner HOT LOOP with Unsafe Access
                for i = 0 to %ceLen - 1 do
                    let ce = ces.[i]
                    let lIdx = ce.Low
                    let hIdx = ce.Hi
                    
                    // Direct pointer offset access
                    let a = Unsafe.Add(&baseRef, lIdx)
                    let b = Unsafe.Add(&baseRef, hIdx)
                    
                    if a > b then
                        Unsafe.Add(&baseRef, lIdx) <- b
                        Unsafe.Add(&baseRef, hIdx) <- a
                        ceUseCounts.Increment (i |> UMX.tag<ceIndex>)

                // Post-sort sorted check
                let isSortedNow = 
                    let mutable ok = true
                    let mutable j = 0
                    while j < sw - 1 && ok do
                        if Unsafe.Add(&baseRef, j) > Unsafe.Add(&baseRef, j + 1) then ok <- false
                        else j <- j + 1
                    ok

                if not isSortedNow then
                    let finalValues = Array.zeroCreate sw
                    workSpan.CopyTo(finalValues.AsSpan())
                    results.Add(sortableIntArray.create(finalValues, sits.SortingWidth, sia.SymbolSetSize)) |> ignore

        pool.Return(workArray)
        let newTests = Seq.toArray results |> sortableIntTest.create (Guid.NewGuid() |> UMX.tag) sits.SortingWidth
        ceBlockEval.create ceBlock ceUseCounts (results.Count |> UMX.tag<sortableCount>) (Some (sortableTest.Ints newTests))


