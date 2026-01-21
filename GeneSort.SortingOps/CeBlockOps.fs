namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter.Sortable
open System.Collections.Generic
open System.Buffers    
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open GeneSort.Sorter
open CeBlockOpsPacked



module CeBlockOps = 

    //let inline sortBy2< ^a when ^a: comparison> 
    //            (ceBlock: ceBlock) 
    //            (ceUseCounts: ceUseCounts)
    //            (values: ^a[]) : ^a[] =

    //    for i = 0 to %ceBlock.Length - 1 do
    //        let ce = ceBlock.getCe i
    //        if values.[ce.Low] > values.[ce.Hi] then
    //            let temp = values.[ce.Low]
    //            values.[ce.Low] <- values.[ce.Hi]
    //            values.[ce.Hi] <- temp
    //            ceUseCounts.Increment (i |> UMX.tag<ceIndex>)
    //    values


    //let inline sortBy
    //            (ceBlock: ceBlock)
    //            (values: int[]) : int[] * int[] =

    //    let localCounts = Array.zeroCreate %ceBlock.Length
    //    for i = 0 to %ceBlock.Length - 1 do
    //        let ce = ceBlock.getCe i
    //        if values.[ce.Low] > values.[ce.Hi] then
    //            let temp = values.[ce.Low]
    //            values.[ce.Low] <- values.[ce.Hi]
    //            values.[ce.Hi] <- temp
    //            localCounts.[i] <- localCounts.[i] + 1
    //    (values, localCounts)



    let evalWithSorterTest 
                (sortableTs: sortableTest) 
                (ceBlock: ceBlock) : ceBlockEval =
        let ceUseCounts = ceUseCounts.Create ceBlock.Length
        let ces = ceBlock.CeArray
    
        match sortableTs with
        | sortableTest.Ints sits ->
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
            ceBlockEval.create ceBlock ceUseCounts (results.Count |> UMX.tag<sortableCount>) (Some (sortableTest.Ints newTests))


        | sortableTest.Bools sbts ->
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
                        ceUseCounts.Increment (i |> UMX.tag<ceIndex>) 

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

            let newTests = Seq.toArray results |> sortableBoolTest.create (Guid.NewGuid() |> UMX.tag) sw
            ceBlockEval.create ceBlock ceUseCounts (results.Count |> UMX.tag<sortableCount>) (Some (sortableTest.Bools newTests))

        | sortableTest.PackedInts _ -> 
            failwith "PackedInts should use evalPacked for maximum efficiency."



    let evalWithSorterTestCeFetch 
                            (sortableTs: sortableTest) 
                            (ceBlock: ceBlock) : ceBlockEval =
        // Fetch indices into flat arrays once.
        // This avoids the overhead of property access (ce.Low/ce.Hi) in the hot loop.
        let ces = ceBlock.CeArray
        let lows = Array.init %ceBlock.Length (fun i -> ces.[i].Low)
        let highs = Array.init %ceBlock.Length (fun i -> ces.[i].Hi)
        
        // Local counts array is significantly faster than using an object-based counter.
        let ceUseCounts = ceUseCounts.Create ceBlock.Length

        match sortableTs with
        | sortableTest.Ints sits ->
            let sw = sits.SortingWidth
            let pool = ArrayPool<int>.Shared
            let results = HashSet<sortableIntArray>(SortableIntArray.SortableIntArrayValueComparer())

            for sia in sits.SortableIntArrays do
                let workArray = pool.Rent(%sw)
                Array.blit sia.Values 0 workArray 0 %sw

                // HOT LOOP: Logic reduced to simple primitive array lookups
                for i = 0 to %ceBlock.Length - 1 do
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
            ceBlockEval.create ceBlock ceUseCounts (results.Count |> UMX.tag<sortableCount>) (Some (sortableTest.Ints newTests))


        | sortableTest.Bools sbts ->
            let sw = sbts.SortingWidth
            let pool = ArrayPool<bool>.Shared
            let results = HashSet<sortableBoolArray>(SortableBoolArray.SortableBoolArrayValueComparer())

            for sba in sbts.SortableBoolArrays do
                let workArray = pool.Rent(%sw)
                Array.blit sba.Values 0 workArray 0 %sw

                for i = 0 to %ceBlock.Length - 1 do
                    let lIdx = lows.[i]
                    let hIdx = highs.[i]
                    // Boolean Comparison: true (1) > false (0)
                    if workArray.[lIdx] && not workArray.[hIdx] then
                        workArray.[lIdx] <- false
                        workArray.[hIdx] <- true
                        ceUseCounts.Increment (i |> UMX.tag<ceIndex>)

                let isSorted = 
                    let mutable ok = true
                    let mutable j = 0
                    while j < %sw - 1 && ok do
                        if workArray.[j] && not workArray.[j+1] then ok <- false
                        else j <- j + 1
                    ok

                if not isSorted then
                    let finalValues = Array.zeroCreate %sw
                    Array.blit workArray 0 finalValues 0 %sw
                    let resultSba = sortableBoolArray.Create(finalValues, sw)
                    results.Add(resultSba) |> ignore

                pool.Return(workArray)

            let newTests = Seq.toArray results |> sortableBoolTest.create (Guid.NewGuid() |> UMX.tag) sw
            ceBlockEval.create ceBlock ceUseCounts (results.Count |> UMX.tag<sortableCount>) (Some (sortableTest.Bools newTests))


        | sortableTest.PackedInts packedTs ->
            evalPackedOptimized packedTs ceBlock



    let evalWithSorterTestUnsafe 
                    (sortableTs: sortableTest) 
                    (ceBlock: ceBlock) : ceBlockEval =
        let ces = ceBlock.CeArray
        let ceLen = ces.Length |> UMX.tag<ceBlockLength>
        // Hoist the raw counts array to avoid method call overhead in the hot loop
        let ceUseCounts = ceUseCounts.Create ceLen

        match sortableTs with
        | sortableTest.Ints sits ->
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


        | sortableTest.Bools sbts ->
            let sw = %sbts.SortingWidth
            let pool = ArrayPool<bool>.Shared
            let results = HashSet<sortableBoolArray>(SortableBoolArray.SortableBoolArrayValueComparer())
        
            let workArray = pool.Rent(sw)
            let workSpan = workArray.AsSpan(0, sw)
            let baseRef = &MemoryMarshal.GetReference(workSpan)

            for sba in sbts.SortableBoolArrays do
                sba.Values.AsSpan().CopyTo(workSpan)

                for i = 0 to %ceLen - 1 do
                    let ce = ces.[i]
                    let a = Unsafe.Add(&baseRef, ce.Low)
                    let b = Unsafe.Add(&baseRef, ce.Hi)
                
                    if a && not b then
                        Unsafe.Add(&baseRef, ce.Low) <- false
                        Unsafe.Add(&baseRef, ce.Hi) <- true
                        ceUseCounts.Increment (i |> UMX.tag<ceIndex>)

                let isSortedNow = 
                    let mutable ok = true
                    let mutable j = 0
                    while j < sw - 1 && ok do
                        if Unsafe.Add(&baseRef, j) && not (Unsafe.Add(&baseRef, j + 1)) then ok <- false
                        else j <- j + 1
                    ok

                if not isSortedNow then
                    let finalValues = Array.zeroCreate sw
                    workSpan.CopyTo(finalValues.AsSpan())
                    results.Add(sortableBoolArray.Create(finalValues, sbts.SortingWidth)) |> ignore

            pool.Return(workArray)
            let newTests = Seq.toArray results |> (sortableBoolTest.create (Guid.NewGuid() |> UMX.tag) sbts.SortingWidth)
            ceBlockEval.create ceBlock ceUseCounts (results.Count |> UMX.tag<sortableCount>) (Some (sortableTest.Bools newTests))


        | sortableTest.PackedInts packedTs ->
            evalPackedOptimized packedTs ceBlock







