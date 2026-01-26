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
            failwith "CeBlockOps: evalWithSorterTest not implemented for sortableUint8v256Test"



    let evalWithSorterTests
                (sortableTs: sortableTest) 
                (ceBlocks: ceBlock []) 
                (chunkSize: int option): ceBlockEval [] =
        match sortableTs with
        | sortableTest.Ints sits ->
            ceBlocks |> Array.map (fun ceBlock ->
                CeBlockOpsInt.evalAndCollectResults sits ceBlock)

        | sortableTest.Bools sbts ->
            ceBlocks |> Array.map (fun ceBlock ->
                CeBlockOpsBinary.evalAndCollectResults sbts ceBlock)

        | sortableTest.PackedInts packedTs ->
            ceBlocks |> Array.map (fun ceBlock ->
                CeBlockOpsPacked.evalAndCollectResults packedTs ceBlock)

        | sortableTest.Uint8v256 su8v256ts ->
            match chunkSize with
            | Some cs when cs > 0 ->
                CeBlockOpsSIMD256.evalAndCollectResults su8v256ts ceBlocks cs
            | _ -> 
                 failwith "CeBlockOps: evalWithSorterTests requires chunkSize > 0 for sortableUint8v256Test"


