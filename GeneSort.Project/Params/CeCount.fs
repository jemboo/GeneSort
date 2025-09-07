namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter


module CeLength =

   let fromString (s: string) : int<ceLength> =
        // Ensure the string is not null or empty
        if String.IsNullOrEmpty(s) then
            failwith "ceLength string cannot be null or empty"
        try
            System.Int32.Parse(s) |> UMX.tag<ceLength>
        with 
        | :? System.FormatException as ex ->
            failwithf "Invalid ceLength string format: %s. Error: %s" s ex.Message


   let getRecordCeCountForFull (swFull:swFull) : int<ceLength> =
       match swFull with
        | swFull.Sw4 -> 5<ceLength>
        | swFull.Sw6 -> 12<ceLength>
        | swFull.Sw8 -> 19<ceLength>
        | swFull.Sw12 -> 39<ceLength>
        | swFull.Sw16 -> 60<ceLength>
        | swFull.Sw24 -> 120<ceLength>
        | swFull.Sw32 -> 185<ceLength>
        | swFull.Sw48 -> 346<ceLength>
        | swFull.Sw64 -> 521<ceLength>


   let getRecordCeCountForMerge (swMerege:swMerege) : int<ceLength> =
        match swMerege with
        | swMerege.Sw4 -> 3<ceLength>
        | swMerege.Sw6 -> 9<ceLength>
        | swMerege.Sw8 -> 16<ceLength>
        | swMerege.Sw12 -> 34<ceLength>
        | swMerege.Sw16 -> 54<ceLength>
        | swMerege.Sw24 -> 110<ceLength>
        | swMerege.Sw32 -> 174<ceLength>
        | swMerege.Sw48 -> 326<ceLength>
        | swMerege.Sw64 -> 498<ceLength>
        | swMerege.Sw96 -> 5000<ceLength>
        | swMerege.Sw128 -> 5000<ceLength>
        | swMerege.Sw192 -> 2500<ceLength>
        | swMerege.Sw256 -> 2500<ceLength>
        | swMerege.Sw384 -> 1000<ceLength>
        | swMerege.Sw512 -> 1000<ceLength>
        | swMerege.Sw768 -> 5000<ceLength>
        | swMerege.Sw1024 -> 5000<ceLength>
        | swMerege.Sw1536 -> 2000<ceLength>
        | swMerege.Sw2048 -> 2000<ceLength>
        | swMerege.Sw3072 -> 1000<ceLength>
        | swMerege.Sw4096 -> 1000<ceLength>
        | swMerege.Sw6144 -> 5000<ceLength>
        | swMerege.Sw8192 -> 5000<ceLength>
                 

   let getP900CeCountForFull (swFull:swFull) : int<ceLength> =
       match swFull with
        | swFull.Sw4 -> 80<ceLength>
        | swFull.Sw6 -> 120<ceLength>
        | swFull.Sw8 -> 160<ceLength>
        | swFull.Sw12 -> 400<ceLength>
        | swFull.Sw16 -> 800<ceLength>
        | swFull.Sw24 -> 1900<ceLength>
        | swFull.Sw32 -> 10000<ceLength>
        | swFull.Sw48 -> 10000<ceLength>
        | swFull.Sw64 -> 10000<ceLength>


   let getP900CeCountForMerge (swMerege:swMerege) : int<ceLength> =
       match swMerege with
        | swMerege.Sw4 -> 60<ceLength>
        | swMerege.Sw6 -> 80<ceLength>
        | swMerege.Sw8 -> 120<ceLength>
        | swMerege.Sw12 -> 300<ceLength>
        | swMerege.Sw16 -> 600<ceLength>
        | swMerege.Sw24 -> 1600<ceLength>
        | swMerege.Sw32 -> 8000<ceLength>
        | swMerege.Sw48 -> 10000<ceLength>
        | swMerege.Sw64 -> 10000<ceLength>
        | swMerege.Sw96 -> 50000<ceLength>
        | swMerege.Sw128 -> 50000<ceLength>
        | swMerege.Sw192 -> 25000<ceLength>
        | swMerege.Sw256 -> 25000<ceLength>
        | swMerege.Sw384 -> 10000<ceLength>
        | swMerege.Sw512 -> 10000<ceLength>
        | swMerege.Sw768 -> 50000<ceLength>
        | swMerege.Sw1024 -> 50000<ceLength>
        | swMerege.Sw1536 -> 20000<ceLength>
        | swMerege.Sw2048 -> 20000<ceLength>
        | swMerege.Sw3072 -> 10000<ceLength>
        | swMerege.Sw4096 -> 10000<ceLength>
        | swMerege.Sw6144 -> 50000<ceLength>
        | swMerege.Sw8192 -> 50000<ceLength>


   let getP999CeCountForFull (swFull:swFull) : int<ceLength> =
       match swFull with
        | swFull.Sw4 -> 300<ceLength>
        | swFull.Sw6 -> 600<ceLength>
        | swFull.Sw8 -> 700<ceLength>
        | swFull.Sw12 -> 1000<ceLength>
        | swFull.Sw16 -> 2400<ceLength>
        | swFull.Sw24 -> 3000<ceLength>
        | swFull.Sw32 -> 10000<ceLength>
        | swFull.Sw48 -> 100000<ceLength>
        | swFull.Sw64 -> 100000<ceLength>


   let getP999CeCountForMerge (swMerege:swMerege) : int<ceLength> =
       match swMerege with
        | swMerege.Sw4 -> 200<ceLength>
        | swMerege.Sw6 -> 400<ceLength>
        | swMerege.Sw8 -> 600<ceLength>
        | swMerege.Sw12 -> 700<ceLength>
        | swMerege.Sw16 -> 1200<ceLength>
        | swMerege.Sw24 -> 2000<ceLength>
        | swMerege.Sw32 -> 8000<ceLength>
        | swMerege.Sw48 -> 8000<ceLength>
        | swMerege.Sw64 -> 8000<ceLength>
        | swMerege.Sw96 -> 8000<ceLength>
        | swMerege.Sw128 -> 8000<ceLength>
        | swMerege.Sw192 -> 8000<ceLength>
        | swMerege.Sw256 -> 8000<ceLength>
        | swMerege.Sw384 -> 8000<ceLength>
        | swMerege.Sw512 -> 8000<ceLength>
        | swMerege.Sw768 -> 8000<ceLength>
        | swMerege.Sw1024 -> 8000<ceLength>
        | swMerege.Sw1536 -> 8000<ceLength>
        | swMerege.Sw2048 -> 8000<ceLength>
        | swMerege.Sw3072 -> 8000<ceLength>
        | swMerege.Sw4096 -> 8000<ceLength>
        | swMerege.Sw6144 -> 8000<ceLength>
        | swMerege.Sw8192 -> 8000<ceLength>



