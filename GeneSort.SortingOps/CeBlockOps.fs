namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorting.Sortable
open System.Collections.Generic
open System.Buffers    
open GeneSort.Sorting



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
                (ceBlock: ceBlock) 
                (collectNewSortableTests: bool) : ceBlockEval =
        match sortableTs with
        | sortableTest.Ints sits ->
            if collectNewSortableTests then
                CeBlockOpsInt.evalAndCollectNewSortableTests sits ceBlock
            else
                CeBlockOpsInt.evalAndCollectNewSortableTests sits ceBlock

        | sortableTest.Bools sbts ->
            if collectNewSortableTests then
                CeBlockOpsBinary.evalAndCollectNewSortableTests sbts ceBlock
            else
                CeBlockOpsBinary.evalAndCollectNewSortableTests sbts ceBlock

        | sortableTest.PackedInts packedTs ->
            if collectNewSortableTests then
                CeBlockOpsPacked.evalAndCollectNewSortableTests packedTs ceBlock
            else
                CeBlockOpsPacked.evalAndCollectNewSortableTests packedTs ceBlock

        | sortableTest.Uint8v256 su8v256ts ->
            if collectNewSortableTests then
                CeBlockOpsUint8v256.evalAndCollectNewSortableTests su8v256ts [| ceBlock |] |> Array.head
            else
                CeBlockOpsUint8v256.evalAndCollectNewSortableTests su8v256ts [| ceBlock |] |> Array.head

        | sortableTest.Uint8v512 su8by512ts ->
            if collectNewSortableTests then
                CeBlockOpsUint8v512.evalAndCollectNewSortableTests su8by512ts [| ceBlock |] |> Array.head
            else
            CeBlockOpsUint8v512.evalAndCollectNewSortableTests su8by512ts [| ceBlock |] |> Array.head

        | sortableTest.Bitv512 su8v256ts ->
            if collectNewSortableTests then
                CeBlockOpsBitv512.evalAndCollectNewSortableTests su8v256ts [| ceBlock |] |> Array.head
            else
                CeBlockOpsBitv512.evalAndCollectNewSortableTests su8v256ts [| ceBlock |] |> Array.head


    let evalWithSorterTests
                (sortableTs: sortableTest) 
                (ceBlocks: ceBlock []) 
                (collectNewSortableTests: bool) : ceBlockEval [] =

        match sortableTs with

        | sortableTest.Bitv512 su8v256ts ->
                if collectNewSortableTests then
                    CeBlockOpsBitv512.evalAndCollectNewSortableTests su8v256ts ceBlocks
                else
                    CeBlockOpsBitv512.eval su8v256ts ceBlocks

        | sortableTest.Bools sbts ->
            ceBlocks |> Array.map (fun ceBlock ->
                if collectNewSortableTests then
                    CeBlockOpsBinary.evalAndCollectNewSortableTests sbts ceBlock
                else
                    CeBlockOpsBinary.eval sbts ceBlock)

        | sortableTest.Ints sits ->
            ceBlocks |> Array.map (fun ceBlock ->
                if collectNewSortableTests then
                    CeBlockOpsInt.evalAndCollectNewSortableTests sits ceBlock
                else
                    CeBlockOpsInt.eval sits ceBlock)

        | sortableTest.PackedInts packedTs ->
            ceBlocks |> Array.map (fun ceBlock ->
                if collectNewSortableTests then
                    CeBlockOpsPacked.evalAndCollectNewSortableTests packedTs ceBlock
                else
                    CeBlockOpsPacked.eval packedTs ceBlock)

        | sortableTest.Uint8v256 su8v256ts ->
                if collectNewSortableTests then
                    CeBlockOpsUint8v256.evalAndCollectNewSortableTests su8v256ts ceBlocks
                else
                    CeBlockOpsUint8v256.eval su8v256ts ceBlocks

        | sortableTest.Uint8v512 su8by512ts ->
                if collectNewSortableTests then
                    CeBlockOpsUint8v512.evalAndCollectNewSortableTests su8by512ts ceBlocks
                else
                    CeBlockOpsUint8v512.eval su8by512ts ceBlocks


