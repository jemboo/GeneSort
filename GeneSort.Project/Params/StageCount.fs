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


    let getRecordStageCountForMerge (swMerege:swMerege) : int<stageCount> =
        match swMerege with
         | swMerege.Sw4 -> 1<stageCount>
         | swMerege.Sw6 -> 3<stageCount>
         | swMerege.Sw8 -> 6<stageCount>
         | swMerege.Sw12 -> 4<stageCount>
         | swMerege.Sw16 -> 4<stageCount>
         | swMerege.Sw24 -> 5<stageCount>
         | swMerege.Sw32 -> 6<stageCount>
         | swMerege.Sw48 -> 7<stageCount>
         | swMerege.Sw64 -> 8<stageCount>
         | swMerege.Sw96 -> 10<stageCount>
         | swMerege.Sw128 -> 12<stageCount>
         | swMerege.Sw192 -> 14<stageCount>
         | swMerege.Sw256 -> 19<stageCount>
         | swMerege.Sw384 -> 20<stageCount>
         | swMerege.Sw512 -> 21<stageCount>
         | swMerege.Sw768 -> 22<stageCount>
         | swMerege.Sw1024 -> 23<stageCount>
         | swMerege.Sw1536 -> 24<stageCount>
         | swMerege.Sw2048 -> 25<stageCount>
         | swMerege.Sw3072 -> 26<stageCount>
         | swMerege.Sw4096 -> 27<stageCount>
         | swMerege.Sw6144 -> 28<stageCount>
         | swMerege.Sw8192 -> 29<stageCount>


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
         | swFull.Sw16 -> 190<stageCount>
         | swFull.Sw24 -> 200<stageCount>
         | swFull.Sw32 -> 300<stageCount>
         | swFull.Sw48 -> 600<stageCount>
         | swFull.Sw64 -> 1000<stageCount>


    let getP900StageCountForMerge (swMerege:swMerege) : int<stageCount> =
         match swMerege with
         | swMerege.Sw4 -> 5<stageCount>
         | swMerege.Sw6 -> 10<stageCount>
         | swMerege.Sw8 -> 20<stageCount>
         | swMerege.Sw12 -> 30<stageCount>
         | swMerege.Sw16 -> 50<stageCount>
         | swMerege.Sw24 -> 70<stageCount>
         | swMerege.Sw32 -> 100<stageCount>
         | swMerege.Sw48 -> 200<stageCount>
         | swMerege.Sw64 -> 300<stageCount>
         | swMerege.Sw96 -> 400<stageCount>
         | swMerege.Sw128 -> 500<stageCount>
         | swMerege.Sw192 -> 600<stageCount>
         | swMerege.Sw256 -> 700<stageCount>
         | swMerege.Sw384 -> 800<stageCount>
         | swMerege.Sw512 -> 900<stageCount>
         | swMerege.Sw768 -> 1000<stageCount>
         | swMerege.Sw1024 -> 1200<stageCount>
         | swMerege.Sw1536 -> 1400<stageCount>
         | swMerege.Sw2048 -> 1600<stageCount>
         | swMerege.Sw3072 -> 1800<stageCount>
         | swMerege.Sw4096 -> 2000<stageCount>
         | swMerege.Sw6144 -> 2500<stageCount>
         | swMerege.Sw8192 -> 3000<stageCount>


    let getP999StageCountForMerge (swMerege:swMerege) : int<stageCount> =
         match swMerege with
         | swMerege.Sw4 -> 10<stageCount>
         | swMerege.Sw6 -> 20<stageCount>
         | swMerege.Sw8 -> 40<stageCount>
         | swMerege.Sw12 -> 60<stageCount>
         | swMerege.Sw16 -> 100<stageCount>
         | swMerege.Sw24 -> 140<stageCount>
         | swMerege.Sw32 -> 200<stageCount>
         | swMerege.Sw48 -> 400<stageCount>
         | swMerege.Sw64 -> 600<stageCount>
         | swMerege.Sw96 -> 800<stageCount>
         | swMerege.Sw128 -> 1000<stageCount>
         | swMerege.Sw192 -> 1200<stageCount>
         | swMerege.Sw256 -> 1400<stageCount>
         | swMerege.Sw384 -> 1600<stageCount>
         | swMerege.Sw512 -> 1800<stageCount>
         | swMerege.Sw768 -> 2000<stageCount>
         | swMerege.Sw1024 -> 2500<stageCount>
         | swMerege.Sw1536 -> 3000<stageCount>
         | swMerege.Sw2048 -> 3500<stageCount>
         | swMerege.Sw3072 -> 4000<stageCount>
         | swMerege.Sw4096 -> 5000<stageCount>
         | swMerege.Sw6144 -> 6000<stageCount>
         | swMerege.Sw8192 -> 7000<stageCount>
         