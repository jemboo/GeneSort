namespace GeneSort.SortingOps

open System
open FSharp.UMX
open System.Collections.Generic
open System.Buffers    
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

module CeBlockOpsBool = 

    let evalAndDedupeBp (sbts: sortableBoolTest) (ceBlock: ceBlock) =
            let ceUseCounts = ceUseCounts.Create ceBlock.Length
            let ces = ceBlock.CeArray
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
                    let resultSba = sortableBoolArray.create(finalValues, sw)
                    results.Add(resultSba) |> ignore

                pool.Return(workArray)

            let newTests = Seq.toArray results |> sortableBoolTest.create (Guid.NewGuid() |> UMX.tag) sw
            ceBlockEval.create ceBlock ceUseCounts (results.Count |> UMX.tag<sortableCount>) (Some (sortableTest.Bools newTests))


    let evalAndDedupeCeFetch (sbts: sortableBoolTest) (ceBlock: ceBlock) =
            let ceUseCounts = ceUseCounts.Create ceBlock.Length
            let lows = Array.init %ceBlock.Length (fun i -> ceBlock.CeArray.[i].Low)
            let highs = Array.init %ceBlock.Length (fun i -> ceBlock.CeArray.[i].Hi)
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
                    let resultSba = sortableBoolArray.create(finalValues, sw)
                    results.Add(resultSba) |> ignore

                pool.Return(workArray)

            let newTests = Seq.toArray results |> sortableBoolTest.create (Guid.NewGuid() |> UMX.tag) sw
            ceBlockEval.create ceBlock ceUseCounts (results.Count |> UMX.tag<sortableCount>) (Some (sortableTest.Bools newTests))



    let evalAndDedupeUnsafe (sbts: sortableBoolTest) (ceBlock: ceBlock) =
        let ces = ceBlock.CeArray
        let ceLen = ces.Length |> UMX.tag<ceBlockLength>
        let ceUseCounts = ceUseCounts.Create ceLen
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
                results.Add(sortableBoolArray.create(finalValues, sbts.SortingWidth)) |> ignore

        pool.Return(workArray)
        let newTests = Seq.toArray results |> (sortableBoolTest.create (Guid.NewGuid() |> UMX.tag) sbts.SortingWidth)
        ceBlockEval.create ceBlock ceUseCounts (results.Count |> UMX.tag<sortableCount>) (Some (sortableTest.Bools newTests))