namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter


module StageLength =

    let fromString (s: string) : int<stageCount> =
        // Ensure the string is not null or empty
        if String.IsNullOrEmpty(s) then
            failwith "Stage count string cannot be null or empty"
        try
            System.Int32.Parse(s) |> UMX.tag<stageCount>
        with 
        | :? System.FormatException as ex ->
            failwithf "Invalid stage count string format: %s. Error: %s" s ex.Message




    //// **********  Full sort   *****************


    let getRecordStageLengthForFull (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 3<stageCount>
         | 6 -> 5<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 8<stageCount>
         | 16 -> 9<stageCount>
         | 24 -> 12<stageCount>
         | 32 -> 14<stageCount>
         | 48 -> 18<stageCount>
         | 64 -> 20<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


    let get0P5StageLengthForFull (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 3<stageCount>
         | 6 -> 5<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 8<stageCount>
         | 16 -> 9<stageCount>
         | 24 -> 12<stageCount>
         | 32 -> 14<stageCount>
         | 48 -> 18<stageCount>
         | 64 -> 20<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


         
    let get0P9StageLengthForFull (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 3<stageCount>
         | 6 -> 5<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 8<stageCount>
         | 16 -> 9<stageCount>
         | 24 -> 12<stageCount>
         | 32 -> 14<stageCount>
         | 48 -> 18<stageCount>
         | 64 -> 20<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)

         
    let get0P99StageLengthForFull (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 3<stageCount>
         | 6 -> 5<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 8<stageCount>
         | 16 -> 9<stageCount>
         | 24 -> 12<stageCount>
         | 32 -> 14<stageCount>
         | 48 -> 18<stageCount>
         | 64 -> 20<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)

    let get0P999StageLengthForFull (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 3<stageCount>
         | 6 -> 5<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 8<stageCount>
         | 16 -> 9<stageCount>
         | 24 -> 12<stageCount>
         | 32 -> 14<stageCount>
         | 48 -> 18<stageCount>
         | 64 -> 20<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)





    //// ********** n/2 Merge sort   *****************


    let getRecordStageLengthForMerge (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 1<stageCount>
         | 6 -> 3<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 4<stageCount>
         | 16 -> 4<stageCount>
         | 24 -> 5<stageCount>
         | 32 -> 6<stageCount>
         | 48 -> 7<stageCount>
         | 64 -> 8<stageCount>
         | 96 -> 10<stageCount>
         | 128 -> 12<stageCount>
         | 192 -> 14<stageCount>
         | 256 -> 19<stageCount>
         | 384 -> 20<stageCount>
         | 512 -> 21<stageCount>
         | 768 -> 22<stageCount>
         | 1024 -> 23<stageCount>
         | 1536 -> 24<stageCount>
         | 2048 -> 25<stageCount>
         | 3072 -> 26<stageCount>
         | 4096 -> 27<stageCount>
         | 6144 -> 28<stageCount>
         | 8192 -> 29<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


    let get0P5StageLengthForMerge (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 1<stageCount>
         | 6 -> 3<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 4<stageCount>
         | 16 -> 4<stageCount>
         | 24 -> 5<stageCount>
         | 32 -> 6<stageCount>
         | 48 -> 7<stageCount>
         | 64 -> 8<stageCount>
         | 96 -> 10<stageCount>
         | 128 -> 12<stageCount>
         | 192 -> 14<stageCount>
         | 256 -> 19<stageCount>
         | 384 -> 20<stageCount>
         | 512 -> 21<stageCount>
         | 768 -> 22<stageCount>
         | 1024 -> 23<stageCount>
         | 1536 -> 24<stageCount>
         | 2048 -> 25<stageCount>
         | 3072 -> 26<stageCount>
         | 4096 -> 27<stageCount>
         | 6144 -> 28<stageCount>
         | 8192 -> 29<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


    let get0P9StageLengthForMerge (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 1<stageCount>
         | 6 -> 3<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 4<stageCount>
         | 16 -> 4<stageCount>
         | 24 -> 5<stageCount>
         | 32 -> 6<stageCount>
         | 48 -> 7<stageCount>
         | 64 -> 8<stageCount>
         | 96 -> 10<stageCount>
         | 128 -> 12<stageCount>
         | 192 -> 14<stageCount>
         | 256 -> 19<stageCount>
         | 384 -> 20<stageCount>
         | 512 -> 21<stageCount>
         | 768 -> 22<stageCount>
         | 1024 -> 23<stageCount>
         | 1536 -> 24<stageCount>
         | 2048 -> 25<stageCount>
         | 3072 -> 26<stageCount>
         | 4096 -> 27<stageCount>
         | 6144 -> 28<stageCount>
         | 8192 -> 29<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)



    let get0P99StageLengthForMerge (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 1<stageCount>
         | 6 -> 3<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 4<stageCount>
         | 16 -> 4<stageCount>
         | 24 -> 5<stageCount>
         | 32 -> 6<stageCount>
         | 48 -> 7<stageCount>
         | 64 -> 8<stageCount>
         | 96 -> 10<stageCount>
         | 128 -> 12<stageCount>
         | 192 -> 14<stageCount>
         | 256 -> 19<stageCount>
         | 384 -> 20<stageCount>
         | 512 -> 21<stageCount>
         | 768 -> 22<stageCount>
         | 1024 -> 23<stageCount>
         | 1536 -> 24<stageCount>
         | 2048 -> 25<stageCount>
         | 3072 -> 26<stageCount>
         | 4096 -> 27<stageCount>
         | 6144 -> 28<stageCount>
         | 8192 -> 29<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


         
    let get0P999StageLengthForMerge (sw:int<sortingWidth>) : int<stageCount> =
        match %sw with
         | 4 -> 1<stageCount>
         | 6 -> 3<stageCount>
         | 8 -> 6<stageCount>
         | 12 -> 4<stageCount>
         | 16 -> 4<stageCount>
         | 24 -> 5<stageCount>
         | 32 -> 6<stageCount>
         | 48 -> 7<stageCount>
         | 64 -> 8<stageCount>
         | 96 -> 10<stageCount>
         | 128 -> 12<stageCount>
         | 192 -> 14<stageCount>
         | 256 -> 19<stageCount>
         | 384 -> 20<stageCount>
         | 512 -> 21<stageCount>
         | 768 -> 22<stageCount>
         | 1024 -> 23<stageCount>
         | 1536 -> 24<stageCount>
         | 2048 -> 25<stageCount>
         | 3072 -> 26<stageCount>
         | 4096 -> 27<stageCount>
         | 6144 -> 28<stageCount>
         | 8192 -> 29<stageCount>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)

