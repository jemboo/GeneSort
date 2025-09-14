namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter


module StageCount =

    let fromString (s: string) : int<stageLength> =
        // Ensure the string is not null or empty
        if String.IsNullOrEmpty(s) then
            failwith "Stage count string cannot be null or empty"
        try
            System.Int32.Parse(s) |> UMX.tag<stageLength>
        with 
        | :? System.FormatException as ex ->
            failwithf "Invalid stage count string format: %s. Error: %s" s ex.Message




    //// **********  Full sort   *****************


    let getRecordStageCountForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


    let get0P5StageCountForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


         
    let get0P9StageCountForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)

         
    let get0P99StageCountForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)

    let get0P999StageCountForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)





    //// ********** n/2 Merge sort   *****************


    let getRecordStageCountForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


    let get0P5StageCountForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


    let get0P9StageCountForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)



    let get0P99StageCountForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


         
    let get0P999StageCountForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)

