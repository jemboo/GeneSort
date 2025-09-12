namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter


module StageCount =

    let fromString (s: string) : int<stageCount> =
        // Ensure the string is not null or empty
        if String.IsNullOrEmpty(s) then
            failwith "Stage count string cannot be null or empty"
        try
            System.Int32.Parse(s) |> UMX.tag<stageCount>
        with 
        | :? System.FormatException as ex ->
            failwithf "Invalid stage count string format: %s. Error: %s" s ex.Message


    let getRecordStageCountForFull (swFull:swFull) : int<stageCount> =
        match swFull with
         | swFull.Sw4 -> 3<stageCount>
         | swFull.Sw6 -> 5<stageCount>
         | swFull.Sw8 -> 6<stageCount>
         | swFull.Sw12 -> 8<stageCount>
         | swFull.Sw16 -> 9<stageCount>
         | swFull.Sw24 -> 12<stageCount>
         | swFull.Sw32 -> 14<stageCount>
         | swFull.Sw48 -> 18<stageCount>
         | swFull.Sw64 -> 20<stageCount>


    let getRecordStageCountForMerge (swMerege:swMerge) : int<stageCount> =
        match swMerege with
         | swMerge.Sw4 -> 1<stageCount>
         | swMerge.Sw6 -> 3<stageCount>
         | swMerge.Sw8 -> 6<stageCount>
         | swMerge.Sw12 -> 4<stageCount>
         | swMerge.Sw16 -> 4<stageCount>
         | swMerge.Sw24 -> 5<stageCount>
         | swMerge.Sw32 -> 6<stageCount>
         | swMerge.Sw48 -> 7<stageCount>
         | swMerge.Sw64 -> 8<stageCount>
         | swMerge.Sw96 -> 10<stageCount>
         | swMerge.Sw128 -> 12<stageCount>
         | swMerge.Sw192 -> 14<stageCount>
         | swMerge.Sw256 -> 19<stageCount>
         | swMerge.Sw384 -> 20<stageCount>
         | swMerge.Sw512 -> 21<stageCount>
         | swMerge.Sw768 -> 22<stageCount>
         | swMerge.Sw1024 -> 23<stageCount>
         | swMerge.Sw1536 -> 24<stageCount>
         | swMerge.Sw2048 -> 25<stageCount>
         | swMerge.Sw3072 -> 26<stageCount>
         | swMerge.Sw4096 -> 27<stageCount>
         | swMerge.Sw6144 -> 28<stageCount>
         | swMerge.Sw8192 -> 29<stageCount>


    let getP900StageCountForFull (swFull:swFull) : int<stageCount> =
         match swFull with
         | swFull.Sw4 -> 10<stageCount>
         | swFull.Sw6 -> 25<stageCount>
         | swFull.Sw8 -> 35<stageCount>
         | swFull.Sw12 -> 60<stageCount>
         | swFull.Sw16 -> 95<stageCount>
         | swFull.Sw24 -> 140<stageCount>
         | swFull.Sw32 -> 200<stageCount>
         | swFull.Sw48 -> 400<stageCount>
         | swFull.Sw64 -> 800<stageCount>


    let getP999StageCountForFull (swFull:swFull) : int<stageCount> =
         match swFull with
         | swFull.Sw4 -> 20<stageCount>
         | swFull.Sw6 -> 50<stageCount>
         | swFull.Sw8 -> 70<stageCount>
         | swFull.Sw12 -> 120<stageCount>
         | swFull.Sw16 -> 200<stageCount>
         | swFull.Sw24 -> 200<stageCount>
         | swFull.Sw32 -> 300<stageCount>
         | swFull.Sw48 -> 600<stageCount>
         | swFull.Sw64 -> 1000<stageCount>


    let getP900StageCountForMerge (swMerege:swMerge) : int<stageCount> =
         match swMerege with
         | swMerge.Sw4 -> 5<stageCount>
         | swMerge.Sw6 -> 10<stageCount>
         | swMerge.Sw8 -> 20<stageCount>
         | swMerge.Sw12 -> 30<stageCount>
         | swMerge.Sw16 -> 100<stageCount>
         | swMerge.Sw24 -> 70<stageCount>
         | swMerge.Sw32 -> 200<stageCount>
         | swMerge.Sw48 -> 200<stageCount>
         | swMerge.Sw64 -> 400<stageCount>
         | swMerge.Sw96 -> 400<stageCount>
         | swMerge.Sw128 -> 500<stageCount>
         | swMerge.Sw192 -> 600<stageCount>
         | swMerge.Sw256 -> 700<stageCount>
         | swMerge.Sw384 -> 800<stageCount>
         | swMerge.Sw512 -> 900<stageCount>
         | swMerge.Sw768 -> 1000<stageCount>
         | swMerge.Sw1024 -> 1200<stageCount>
         | swMerge.Sw1536 -> 1400<stageCount>
         | swMerge.Sw2048 -> 1600<stageCount>
         | swMerge.Sw3072 -> 1800<stageCount>
         | swMerge.Sw4096 -> 2000<stageCount>
         | swMerge.Sw6144 -> 2500<stageCount>
         | swMerge.Sw8192 -> 3000<stageCount>


    let getP999StageCountForMerge (swMerege:swMerge) : int<stageCount> =
         match swMerege with
         | swMerge.Sw4 -> 10<stageCount>
         | swMerge.Sw6 -> 20<stageCount>
         | swMerge.Sw8 -> 60<stageCount>
         | swMerge.Sw12 -> 80<stageCount>
         | swMerge.Sw16 -> 300<stageCount>
         | swMerge.Sw24 -> 140<stageCount>
         | swMerge.Sw32 -> 800<stageCount>
         | swMerge.Sw48 -> 400<stageCount>
         | swMerge.Sw64 -> 3000<stageCount>
         | swMerge.Sw96 -> 800<stageCount>
         | swMerge.Sw128 -> 10000<stageCount>
         | swMerge.Sw192 -> 1200<stageCount>
         | swMerge.Sw256 -> 30000<stageCount>
         | swMerge.Sw384 -> 1600<stageCount>
         | swMerge.Sw512 -> 1800<stageCount>
         | swMerge.Sw768 -> 2000<stageCount>
         | swMerge.Sw1024 -> 2500<stageCount>
         | swMerge.Sw1536 -> 3000<stageCount>
         | swMerge.Sw2048 -> 3500<stageCount>
         | swMerge.Sw3072 -> 4000<stageCount>
         | swMerge.Sw4096 -> 5000<stageCount>
         | swMerge.Sw6144 -> 6000<stageCount>
         | swMerge.Sw8192 -> 7000<stageCount>
         



    let getP999StageCountForMergeSw4 (swMerege:swMerge) : int<stageCount> =
         match swMerege with
         | swMerge.Sw4 -> 10<stageCount>
         | swMerge.Sw6 -> 20<stageCount>
         | swMerge.Sw8 -> 60<stageCount>
         | swMerge.Sw12 -> 80<stageCount>
         | swMerge.Sw16 -> 300<stageCount>
         | swMerge.Sw24 -> 140<stageCount>
         | swMerge.Sw32 -> 200<stageCount>
         | swMerge.Sw48 -> 400<stageCount>
         | swMerge.Sw64 -> 500<stageCount>
         | swMerge.Sw96 -> 800<stageCount>
         | swMerge.Sw128 -> 2000<stageCount>
         | swMerge.Sw192 -> 1200<stageCount>
         | swMerge.Sw256 -> 9000<stageCount>
         | swMerge.Sw384 -> 1600<stageCount>
         | swMerge.Sw512 -> 35000<stageCount>
         | swMerge.Sw768 -> 2000<stageCount>
         | swMerge.Sw1024 -> 2500<stageCount>
         | swMerge.Sw1536 -> 3000<stageCount>
         | swMerge.Sw2048 -> 3500<stageCount>
         | swMerge.Sw3072 -> 4000<stageCount>
         | swMerge.Sw4096 -> 5000<stageCount>
         | swMerge.Sw6144 -> 6000<stageCount>
         | swMerge.Sw8192 -> 7000<stageCount>