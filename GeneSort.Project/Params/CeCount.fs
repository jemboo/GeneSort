namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter


module CeCount =

   let fromString (s: string) : int<ceCount> =
        // Ensure the string is not null or empty
        if String.IsNullOrEmpty(s) then
            failwith "ceCount string cannot be null or empty"
        try
            System.Int32.Parse(s) |> UMX.tag<ceCount>
        with 
        | :? System.FormatException as ex ->
            failwithf "Invalid ceCount string format: %s. Error: %s" s ex.Message


   let getRecordCeCountForFull (swFull:swFull) : int<ceCount> =
       match swFull with
        | swFull.Sw4 -> 5<ceCount>
        | swFull.Sw6 -> 12<ceCount>
        | swFull.Sw8 -> 19<ceCount>
        | swFull.Sw12 -> 39<ceCount>
        | swFull.Sw16 -> 60<ceCount>
        | swFull.Sw24 -> 120<ceCount>
        | swFull.Sw32 -> 185<ceCount>
        | swFull.Sw48 -> 346<ceCount>
        | swFull.Sw64 -> 521<ceCount>


   let getRecordCeCountForMerge (swMerege:swMerege) : int<ceCount> =
        match swMerege with
        | swMerege.Sw4 -> 3<ceCount>
        | swMerege.Sw6 -> 9<ceCount>
        | swMerege.Sw8 -> 16<ceCount>
        | swMerege.Sw12 -> 34<ceCount>
        | swMerege.Sw16 -> 54<ceCount>
        | swMerege.Sw24 -> 110<ceCount>
        | swMerege.Sw32 -> 174<ceCount>
        | swMerege.Sw48 -> 326<ceCount>
        | swMerege.Sw64 -> 498<ceCount>
        | swMerege.Sw96 -> 5000<ceCount>
        | swMerege.Sw128 -> 5000<ceCount>
        | swMerege.Sw192 -> 2500<ceCount>
        | swMerege.Sw256 -> 2500<ceCount>
        | swMerege.Sw384 -> 1000<ceCount>
        | swMerege.Sw512 -> 1000<ceCount>
        | swMerege.Sw768 -> 5000<ceCount>
        | swMerege.Sw1024 -> 5000<ceCount>
        | swMerege.Sw1536 -> 2000<ceCount>
        | swMerege.Sw2048 -> 2000<ceCount>
        | swMerege.Sw3072 -> 1000<ceCount>
        | swMerege.Sw4096 -> 1000<ceCount>
        | swMerege.Sw6144 -> 5000<ceCount>
        | swMerege.Sw8192 -> 5000<ceCount>
                 

   let getP900CeCountForFull (swFull:swFull) : int<ceCount> =
       match swFull with
        | swFull.Sw4 -> 80<ceCount>
        | swFull.Sw6 -> 120<ceCount>
        | swFull.Sw8 -> 160<ceCount>
        | swFull.Sw12 -> 400<ceCount>
        | swFull.Sw16 -> 800<ceCount>
        | swFull.Sw24 -> 1900<ceCount>
        | swFull.Sw32 -> 10000<ceCount>
        | swFull.Sw48 -> 10000<ceCount>
        | swFull.Sw64 -> 10000<ceCount>


   let getP900CeCountForMerge (swMerege:swMerege) : int<ceCount> =
       match swMerege with
        | swMerege.Sw4 -> 60<ceCount>
        | swMerege.Sw6 -> 80<ceCount>
        | swMerege.Sw8 -> 120<ceCount>
        | swMerege.Sw12 -> 300<ceCount>
        | swMerege.Sw16 -> 600<ceCount>
        | swMerege.Sw24 -> 1600<ceCount>
        | swMerege.Sw32 -> 8000<ceCount>
        | swMerege.Sw48 -> 10000<ceCount>
        | swMerege.Sw64 -> 10000<ceCount>
        | swMerege.Sw96 -> 50000<ceCount>
        | swMerege.Sw128 -> 50000<ceCount>
        | swMerege.Sw192 -> 25000<ceCount>
        | swMerege.Sw256 -> 25000<ceCount>
        | swMerege.Sw384 -> 10000<ceCount>
        | swMerege.Sw512 -> 10000<ceCount>
        | swMerege.Sw768 -> 50000<ceCount>
        | swMerege.Sw1024 -> 50000<ceCount>
        | swMerege.Sw1536 -> 20000<ceCount>
        | swMerege.Sw2048 -> 20000<ceCount>
        | swMerege.Sw3072 -> 10000<ceCount>
        | swMerege.Sw4096 -> 10000<ceCount>
        | swMerege.Sw6144 -> 50000<ceCount>
        | swMerege.Sw8192 -> 50000<ceCount>


   let getP999CeCountForFull (swFull:swFull) : int<ceCount> =
       match swFull with
        | swFull.Sw4 -> 300<ceCount>
        | swFull.Sw6 -> 600<ceCount>
        | swFull.Sw8 -> 700<ceCount>
        | swFull.Sw12 -> 1000<ceCount>
        | swFull.Sw16 -> 1600<ceCount>
        | swFull.Sw24 -> 3000<ceCount>
        | swFull.Sw32 -> 10000<ceCount>
        | swFull.Sw48 -> 100000<ceCount>
        | swFull.Sw64 -> 100000<ceCount>


   let getP999CeCountForMerge (swMerege:swMerege) : int<ceCount> =
       match swMerege with
        | swMerege.Sw4 -> 200<ceCount>
        | swMerege.Sw6 -> 400<ceCount>
        | swMerege.Sw8 -> 600<ceCount>
        | swMerege.Sw12 -> 700<ceCount>
        | swMerege.Sw16 -> 1200<ceCount>
        | swMerege.Sw24 -> 2000<ceCount>
        | swMerege.Sw32 -> 8000<ceCount>
        | swMerege.Sw48 -> 8000<ceCount>
        | swMerege.Sw64 -> 8000<ceCount>
        | swMerege.Sw96 -> 8000<ceCount>
        | swMerege.Sw128 -> 8000<ceCount>
        | swMerege.Sw192 -> 8000<ceCount>
        | swMerege.Sw256 -> 8000<ceCount>
        | swMerege.Sw384 -> 8000<ceCount>
        | swMerege.Sw512 -> 8000<ceCount>
        | swMerege.Sw768 -> 8000<ceCount>
        | swMerege.Sw1024 -> 8000<ceCount>
        | swMerege.Sw1536 -> 8000<ceCount>
        | swMerege.Sw2048 -> 8000<ceCount>
        | swMerege.Sw3072 -> 8000<ceCount>
        | swMerege.Sw4096 -> 8000<ceCount>
        | swMerege.Sw6144 -> 8000<ceCount>
        | swMerege.Sw8192 -> 8000<ceCount>



