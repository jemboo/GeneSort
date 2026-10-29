
namespace GeneSort.Sorting.Sorter

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting


module BitonicSorter =

    let mergeSubLists<'a> (list1: 'a list) (list2: 'a list) : 'a list = 
        let rec mergeHelper l1 l2 acc =
            match l1, l2 with
            | [], [] -> List.rev acc
            | [], _ -> List.rev acc @ l2
            | _, [] -> List.rev acc @ l1
            | h1::t1, h2::t2 -> mergeHelper t1 t2 (h1::h2::acc)
        mergeHelper list1 list2 []
    

    ///// Generate comparators for a bitonic merge (scheme 1)
    //// For the full merge sequence for a given soringWidth, set upperOffset = 0<channelCount>
    let rec bitonicMerge1 (upperOffset: int<channelCount>) (fullWidth: int<channelCount>) : ce [][] =
        if %fullWidth > 1 then
            let halfWidth = (%fullWidth / 2) |> UMX.tag<channelCount>
            let prefix = 
                [| for i in %upperOffset .. %upperOffset + %halfWidth - 1 -> ce.create i (i + %halfWidth) |]
            let upperSuffix = bitonicMerge1 %upperOffset halfWidth
            (Ce.stack2d upperSuffix upperSuffix halfWidth) |> Array.append [| prefix |]
        else
            [||]


    ///// Generate comparators for a bitonic merge (scheme 2)
    //// For the full merge sequence for a given soringWidth, set upperOffset = 0<channelCount>
    let bitonicMerge2 (upperOffset: int<channelCount>) (fullWidth: int<channelCount>) : ce [][] =
        if %fullWidth > 1 then
            let halfWidth = (%fullWidth / 2) |> UMX.tag<channelCount>
            let prefix = 
                [| for i in %upperOffset .. %upperOffset + %halfWidth - 1 -> ce.create i (%upperOffset + %fullWidth - 1 - i) |]
            let upperSuffix = bitonicMerge1 %upperOffset halfWidth
            Array.append [| prefix |] (Ce.stack2d upperSuffix upperSuffix halfWidth)
        else
            [||]


    ///// Generate comparators for a bitonic sort (scheme 1)
    let bitonicSort1 (channelCount: int<channelCount>) : ce [][] =
        let rec bitonicSortHelper (upperOffset: int<channelCount>) (fullWidth: int<channelCount>) : ce [][] =
            if %fullWidth = 2 then
                (bitonicMerge1 upperOffset fullWidth)
            else
               let halfWidth = (%fullWidth / 2) |> UMX.tag<channelCount>
               let middle = 
                    [| for i in 0 .. %halfWidth - 1 -> ce.create (%upperOffset + i * 2) (%upperOffset + i * 2 + 1) |]
               let suffix = bitonicMerge1 %upperOffset fullWidth
               let upperPrefix = bitonicSortHelper %upperOffset halfWidth
               let fullPrefix = (Ce.stack2d upperPrefix upperPrefix halfWidth)
               Array.append fullPrefix suffix

        bitonicSortHelper 0<channelCount> channelCount


    ///// Generate comparators for a bitonic sort (scheme 2)
    let bitonicSort2 (channelCount: int<channelCount>) : ce [][] =
        let rec bitonicSortHelper (upperOffset: int<channelCount>) (fullWidth: int<channelCount>) : ce [][] =
            if %fullWidth = 2 then
                (bitonicMerge1 upperOffset fullWidth)
            else
               let halfWidth = (%fullWidth / 2) |> UMX.tag<channelCount>
               let middle = 
                    [| for i in 0 .. %halfWidth - 1 -> ce.create (%upperOffset + i * 2) (%upperOffset + i * 2 + 1) |]
               let suffix = bitonicMerge2 %upperOffset fullWidth
               let upperPrefix = bitonicSortHelper %upperOffset halfWidth
               let fullPrefix = (Ce.stack2d upperPrefix upperPrefix halfWidth)
               Array.append fullPrefix suffix

        bitonicSortHelper 0<channelCount> channelCount


    let prefix1 : sorter =
        let sorterId =
            [
                "prefix1-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 2<channelCount>
        let ces = bitonicSort1 channelCount |> Array.concat
        sorter.create sorterId channelCount ces


    let prefix2 : sorter =
        let sorterId =
            [
                "prefix2-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 4<channelCount>
        let ces = bitonicSort1 channelCount |> Array.take 2 |> Array.concat
        sorter.create sorterId channelCount ces


    let prefix3 : sorter =
        let sorterId =
            [
                "prefix3-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 4<channelCount>
        let ces = bitonicSort1 channelCount |> Array.take 3 |> Array.concat
        sorter.create sorterId channelCount ces


    let prefix4 : sorter =
        let sorterId =
            [
                "prefix4-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 8<channelCount>
        let ces = bitonicSort1 channelCount |> Array.take 4 |> Array.concat
        sorter.create sorterId channelCount ces


    let prefix5 : sorter =
        let sorterId =
            [
                "prefix5-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 8<channelCount>
        let ces = bitonicSort1 channelCount |> Array.take 5 |> Array.concat
        sorter.create sorterId channelCount ces


    let prefix6 : sorter =
        let sorterId =
            [
                "prefix6-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 8<channelCount>
        let ces = bitonicSort1 channelCount |> Array.take 6 |> Array.concat
        sorter.create sorterId channelCount ces


    let prefix7 : sorter =
        let sorterId =
            [
                "prefix7-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 16<channelCount>
        let ces = bitonicSort1 channelCount |> Array.take 7 |> Array.concat
        sorter.create sorterId channelCount ces


    let prefix8 : sorter =
        let sorterId =
            [
                "prefix8-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 8<channelCount>
        let ces = bitonicSort1 channelCount |> Array.take 8 |> Array.concat
        sorter.create sorterId channelCount ces


    let prefix9 : sorter =
        let sorterId =
            [
                "prefix9-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 8<channelCount>
        let ces = bitonicSort1 channelCount |> Array.take 9 |> Array.concat
        sorter.create sorterId channelCount ces


    let prefix10 : sorter =
        let sorterId =
            [
                "prefix10-bitonic-sorter"  :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterId>

        let channelCount = 16<channelCount>
        let ces = bitonicSort1 channelCount |> Array.take 10 |> Array.concat
        sorter.create sorterId channelCount ces

