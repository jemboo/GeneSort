namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter.Sortable
open System.Collections.Generic
open System.Buffers    
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open GeneSort.Sorter


type PackedOffsetComparer(data: int[], sw: int) =
    interface IEqualityComparer<int> with
        member _.Equals(offsetA: int, offsetB: int) =
            let spanA = data.AsSpan(offsetA, sw)
            let spanB = data.AsSpan(offsetB, sw)
            spanA.SequenceEqual(spanB)

        member _.GetHashCode(offset: int) =
            let span = data.AsSpan(offset, sw)
            // Reinterpret the int span as bytes (zero-copy)
            let byteSpan = MemoryMarshal.Cast<int, byte>(span)
            
            let mutable hash = HashCode()
            // Force the conversion by defining a ReadOnlySpan binding
            let roByteSpan : ReadOnlySpan<byte> = byteSpan
            hash.AddBytes(roByteSpan)
            hash.ToHashCode()



module CeBlockOpsPacked = 


    let deduplicatePacked (packedTs: packedSortableIntTests) (resultsBuffer: int[]) =
        let sw = %packedTs.SortingWidth
        let totalTests = packedTs.SoratbleCount
    
        // 1. HashSet stores ONLY the starting index (offset) of unique unsorted tests
        let comparer = PackedOffsetComparer(resultsBuffer, sw)
        let uniqueOffsets = HashSet<int>(comparer)

        for t = 0 to %totalTests - 1 do
            let offset = t * sw
        
            // 2. Check sortedness in-place first (cheapest check)
            let mutable isSorted = true
            let mutable j = 0
            while j < sw - 1 && isSorted do
                if resultsBuffer.[offset + j] > resultsBuffer.[offset + j + 1] then 
                    isSorted <- false
                else j <- j + 1

            // 3. If unsorted, try to add the offset to our "unique" set
            if not isSorted then
                uniqueOffsets.Add(offset) |> ignore

        // 4. Re-pack only the unique failures into a new contiguous array
        let newCount = uniqueOffsets.Count
        let newPackedData = Array.zeroCreate (newCount * sw)
        let mutable targetOffset = 0
    
        for sourceOffset in uniqueOffsets do
            Array.blit resultsBuffer sourceOffset newPackedData targetOffset sw
            targetOffset <- targetOffset + sw

        packedSortableIntTests.createFromPackedValues 
                        packedTs.SortingWidth 
                        newPackedData
                        (newCount |> UMX.tag<sortableCount>)
                                           


    let evalWithSorterTest (sortableTs: sortableTests) (ceBlock: ceBlock) =
        let ceUseCounts = ceUseCounts.Create ceBlock.Length
        let ces = ceBlock.CeArray
    
        match sortableTs with
        | sortableTests.Ints sits ->
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

            let newTests = Seq.toArray results |> sortableIntTests.create (Guid.NewGuid() |> UMX.tag) sw
            let yab = sortableTests.Ints newTests
            ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) yab

        | sortableTests.Bools sbts ->
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

            let newTests = Seq.toArray results |> sortableBoolTests.create (Guid.NewGuid() |> UMX.tag) sw
            let yab = sortableTests.Bools newTests
            ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) yab


    let evalWithSorterTestRefined (sortableTs: sortableTests) (ceBlock: ceBlock) =

        let ces = ceBlock.CeArray
        let lows = Array.init %ceBlock.Length (fun i -> ces.[i].Low)
        let highs = Array.init %ceBlock.Length (fun i -> ces.[i].Hi)
        
        // Local counts array is significantly faster than using an object-based counter.
        let ceUseCounts = ceUseCounts.Create ceBlock.Length

        match sortableTs with
        | sortableTests.Ints sits ->
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

            let newTests = Seq.toArray results |> sortableIntTests.create (Guid.NewGuid() |> UMX.tag) sw
            let yab = sortableTests.Ints newTests
            // Create usage using the local counts array we populated
            ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) yab


        | sortableTests.Bools sbts ->
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

            let newTests = Seq.toArray results |> sortableBoolTests.create (Guid.NewGuid() |> UMX.tag) sw
            let yab = sortableTests.Bools newTests
            ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) yab

        | sortableTests.PackedInts _ -> 
            failwith "PackedInts should use evalPacked for maximum efficiency."





    let evalWithSorterTestNew 
                    (sortableTs: sortableTests) 
                    (ceBlock: ceBlock) : ceBlockEval =
        let ces = ceBlock.CeArray
        let ceUseCounts = ceUseCounts.Create ceBlock.Length

        match sortableTs with
        | sortableTests.Ints sits ->
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

                    for i = 0 to %ceBlock.Length - 1 do
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
            let newTests = Seq.toArray results |> sortableIntTests.create (Guid.NewGuid() |> UMX.tag) sits.SortingWidth
            let yab = sortableTests.Ints newTests
            ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) yab


        | sortableTests.Bools sbts ->
            let sw = %sbts.SortingWidth
            let pool = ArrayPool<bool>.Shared
            let results = HashSet<sortableBoolArray>(SortableBoolArray.SortableBoolArrayValueComparer())
        
            let workArray = pool.Rent(sw)
            let workSpan = workArray.AsSpan(0, sw)
            let baseRef = &MemoryMarshal.GetReference(workSpan)

            for sba in sbts.SortableBoolArrays do
                sba.Values.AsSpan().CopyTo(workSpan)
                
                for i = 0 to %ceBlock.Length - 1 do
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
            let newTests = Seq.toArray results |> (sortableBoolTests.create (Guid.NewGuid() |> UMX.tag) sbts.SortingWidth)
            let yab = sortableTests.Bools newTests
            ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) yab


        | sortableTests.PackedInts packedTs ->
            let sw = %packedTs.SortingWidth
            let totalTests = packedTs.SoratbleCount
            let ces = ceBlock.CeArray
        
            // Work on a copy of the values so the original 'tests' remain immutable
            let resultsBuffer = Array.copy packedTs.PackedValues
            let mutable dataRef = &MemoryMarshal.GetReference(resultsBuffer.AsSpan())

            // Inner loop optimization: Cache ces locally to improve L1 cache hit rate
            for t = 0 to %totalTests - 1 do
                let offset = t * sw
            
                for i = 0 to ces.Length - 1 do
                    let ce = ces.[i]
                    // Compute exact memory locations once per CE
                    let lPtr = &Unsafe.Add(&dataRef, offset + ce.Low)
                    let hPtr = &Unsafe.Add(&dataRef, offset + ce.Hi)
                
                    let a = lPtr
                    let b = hPtr
                
                    if a > b then
                        lPtr <- b
                        hPtr <- a
                        ceUseCounts.Increment (i |> UMX.tag<ceIndex>) 

            let uniqueUnsorted = HashSet<sortableIntArray>(SortableIntArray.SortableIntArrayValueComparer())
            let dataSpan = resultsBuffer.AsSpan()
        
            for t = 0 to %totalTests - 1 do
                let offset = t * sw
                // Get a slice of the flattened buffer for this specific test case
                let testSlice = dataSpan.Slice(offset, sw)

                // 1. Check sortedness IN-PLACE
                let mutable isSorted = true
                let mutable j = 0
                while j < sw - 1 && isSorted do
                    // Using Unsafe or indexed access on the slice
                    if testSlice.[j] > testSlice.[j+1] then 
                        isSorted <- false
                    else 
                        j <- j + 1

                // 2. ONLY if unsorted, do we allocate and deduplicate
                if not isSorted then
                    let finalValues = testSlice.ToArray() // Only allocates for failures
                    let resultSia = sortableIntArray.create(finalValues, packedTs.SortingWidth, 16<symbolSetSize>)
                    uniqueUnsorted.Add(resultSia) |> ignore

            // Return a fresh packed structure for the next generation/block
       
            let finalArrays = Seq.toArray uniqueUnsorted
            let unsortedCount = finalArrays.Length |> UMX.tag<sortableCount>
            let newPacked = packedSortableIntTests.create packedTs.SortingWidth finalArrays unsortedCount
            let yab = sortableTests.PackedInts newPacked
            ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) yab




    let evalPackedOptimized (tests: packedSortableIntTests) (ceBlock: ceBlock) =
        let sw = %tests.SortingWidth
        let totalTests = tests.SoratbleCount
        let ces = ceBlock.CeArray
    
        // Pre-deconstruct CEs for the hot loop
        let lows = Array.init %ceBlock.Length (fun i -> ces.[i].Low)
        let highs = Array.init %ceBlock.Length (fun i -> ces.[i].Hi)
        let ceUseCounts = ceUseCounts.Create ceBlock.Length
    
        // Work on a mutable copy
        let resultsBuffer = Array.copy tests.PackedValues
        let mutable dataRef = &MemoryMarshal.GetReference(resultsBuffer.AsSpan())

        // PHASE 1: THE SORTING (Hot Loop)
        for t = 0 to %totalTests - 1 do
            let offset = t * sw
            for i = 0 to %ceBlock.Length - 1 do
                let lPtr = &Unsafe.Add(&dataRef, offset + lows.[i])
                let hPtr = &Unsafe.Add(&dataRef, offset + highs.[i])
                let a = lPtr
                let b = hPtr
                if a > b then
                    lPtr <- b
                    hPtr <- a
                    ceUseCounts.Increment (i |> UMX.tag<ceIndex>)

        // PHASE 2: IN-PLACE DEDUPLICATION
        let comparer = PackedOffsetComparer(resultsBuffer, sw)
        let uniqueOffsets = HashSet<int>(comparer)
    
        for t = 0 to %totalTests - 1 do
            let offset = t * sw
            let mutable isSorted = true
            let mutable j = 0
            while j < sw - 1 && isSorted do
                if resultsBuffer.[offset + j] > resultsBuffer.[offset + j + 1] then 
                    isSorted <- false
                else j <- j + 1

            if not isSorted then
                uniqueOffsets.Add(offset) |> ignore

        // PHASE 3: MATERIALIZATION
        let newCount = uniqueOffsets.Count
        let newPackedData = Array.zeroCreate (newCount * sw)
        let mutable targetOffset = 0
        for sourceOffset in uniqueOffsets do
            Array.blit resultsBuffer sourceOffset newPackedData targetOffset sw
            targetOffset <- targetOffset + sw

        let newPacked = packedSortableIntTests.createFromPackedValues
                                                    tests.SortingWidth
                                                    newPackedData
                                                    (newCount |> UMX.tag<sortableCount>)
       
        ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) (sortableTests.PackedInts newPacked)



    let evalWithSorterTestNewer
                    (sortableTs: sortableTests) 
                    (ceBlock: ceBlock) : ceBlockEval =
        let ces = ceBlock.CeArray
        let ceUseCounts = ceUseCounts.Create ceBlock.Length
        let ceLen = ces.Length
        let lows = Array.init ceLen (fun i -> ces.[i].Low)
        let highs = Array.init ceLen (fun i -> ces.[i].Hi)
        let localCounts = Array.zeroCreate ceLen

        match sortableTs with
        | sortableTests.Ints sits ->
            let sw = %sits.SortingWidth
            let pool = ArrayPool<int>.Shared
            let results = HashSet<sortableIntArray>(SortableIntArray.SortableIntArrayValueComparer())

            for sia in sits.SortableIntArrays do
                // 1. Rent a tiny scratchpad (Fits in L1 Cache)
                let workArray = pool.Rent(sw)
                Array.blit sia.Values 0 workArray 0 sw

                // 2. THE HOT LOOP (Minimal indirection)
                for i = 0 to ceLen - 1 do
                    let lIdx = lows.[i]
                    let hIdx = highs.[i]
                    let a = workArray.[lIdx]
                    let b = workArray.[hIdx]
                    if a > b then
                        workArray.[lIdx] <- b
                        workArray.[hIdx] <- a
                        ceUseCounts.Increment (i |> UMX.tag<ceIndex>)

                // 3. Check sortedness and deduplicate
                let mutable isSorted = true
                let mutable j = 0
                while j < sw - 1 && isSorted do
                    if workArray.[j] > workArray.[j+1] then isSorted <- false
                    else j <- j + 1

                if not isSorted then
                    let finalValues = Array.zeroCreate sw
                    Array.blit workArray 0 finalValues 0 sw
                    results.Add(sortableIntArray.create(finalValues, sits.SortingWidth, sia.SymbolSetSize)) |> ignore

                pool.Return(workArray)

            let newTests = Seq.toArray results |> sortableIntTests.create (Guid.NewGuid() |> UMX.tag) sits.SortingWidth
            ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) (sortableTests.Ints newTests)


        | sortableTests.Bools sbts ->
            let sw = %sbts.SortingWidth
            let pool = ArrayPool<bool>.Shared
            let results = HashSet<sortableBoolArray>(SortableBoolArray.SortableBoolArrayValueComparer())
        
            let workArray = pool.Rent(sw)
            let workSpan = workArray.AsSpan(0, sw)
            let baseRef = &MemoryMarshal.GetReference(workSpan)

            for sba in sbts.SortableBoolArrays do
                sba.Values.AsSpan().CopyTo(workSpan)

                for i = 0 to %ceBlock.Length - 1 do
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
            let newTests = Seq.toArray results |> (sortableBoolTests.create (Guid.NewGuid() |> UMX.tag) sbts.SortingWidth)
            let yab = sortableTests.Bools newTests
            ceBlockEval.create (ceBlockWithUsage.create ceBlock ceUseCounts) yab

        | sortableTests.PackedInts packedTs ->
            evalPackedOptimized packedTs ceBlock










