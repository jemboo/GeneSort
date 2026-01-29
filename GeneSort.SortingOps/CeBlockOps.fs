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
                (ceBlock: ceBlock) : ceBlockEval =
        match sortableTs with
        | sortableTest.Ints sits ->
            CeBlockOpsInt.evalAndCollectResults sits ceBlock

        | sortableTest.Bools sbts ->
            CeBlockOpsBinary.evalAndCollectResults sbts ceBlock

        | sortableTest.PackedInts packedTs ->
            CeBlockOpsPacked.evalAndCollectResults packedTs ceBlock

        | sortableTest.Uint8v256 su8v256ts ->
            CeBlockOpsUint8v256.evalAndCollectResults su8v256ts [| ceBlock |] |> Array.head

        | sortableTest.Uint8v512 su8by512ts ->
            CeBlockOpsUint8v512.evalAndCollectResults su8by512ts [| ceBlock |] |> Array.head



    let evalWithSorterTests
                (sortableTs: sortableTest) 
                (ceBlocks: ceBlock []) 
                (collectResults: bool) : ceBlockEval [] =

        match sortableTs with

        | sortableTest.Bitv512 su8v256ts ->
                if collectResults then
                    CeBlockOpsBitv512.evalAndCollectResults su8v256ts ceBlocks
                else
                    CeBlockOpsBitv512.eval su8v256ts ceBlocks

        | sortableTest.Bools sbts ->
            ceBlocks |> Array.map (fun ceBlock ->
                if collectResults then
                    CeBlockOpsBinary.evalAndCollectResults sbts ceBlock
                else
                    CeBlockOpsBinary.eval sbts ceBlock)

        | sortableTest.Ints sits ->
            ceBlocks |> Array.map (fun ceBlock ->
                if collectResults then
                    CeBlockOpsInt.evalAndCollectResults sits ceBlock
                else
                    CeBlockOpsInt.eval sits ceBlock)

        | sortableTest.PackedInts packedTs ->
            ceBlocks |> Array.map (fun ceBlock ->
                if collectResults then
                    CeBlockOpsPacked.evalAndCollectResults packedTs ceBlock
                else
                    CeBlockOpsPacked.eval packedTs ceBlock)

        | sortableTest.Uint8v256 su8v256ts ->
                if collectResults then
                    CeBlockOpsUint8v256.evalAndCollectResults su8v256ts ceBlocks
                else
                    CeBlockOpsUint8v256.eval su8v256ts ceBlocks

        | sortableTest.Uint8v512 su8by512ts ->
                if collectResults then
                    CeBlockOpsUint8v512.evalAndCollectResults su8by512ts ceBlocks
                else
                    CeBlockOpsUint8v512.eval su8by512ts ceBlocks


