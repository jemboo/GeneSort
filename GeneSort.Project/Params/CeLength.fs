namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Sorter


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


   let getRecordCeCountForMerge (swMerege:swMerge) : int<ceLength> =
        match swMerege with
        | swMerge.Sw4 -> 3<ceLength>
        | swMerge.Sw6 -> 9<ceLength>
        | swMerge.Sw8 -> 16<ceLength>
        | swMerge.Sw12 -> 34<ceLength>
        | swMerge.Sw16 -> 54<ceLength>
        | swMerge.Sw24 -> 110<ceLength>
        | swMerge.Sw32 -> 174<ceLength>
        | swMerge.Sw48 -> 326<ceLength>
        | swMerge.Sw64 -> 498<ceLength>
        | swMerge.Sw96 -> 5000<ceLength>
        | swMerge.Sw128 -> 5000<ceLength>
        | swMerge.Sw192 -> 2500<ceLength>
        | swMerge.Sw256 -> 2500<ceLength>
        | swMerge.Sw384 -> 1000<ceLength>
        | swMerge.Sw512 -> 1000<ceLength>
        | swMerge.Sw768 -> 5000<ceLength>
        | swMerge.Sw1024 -> 5000<ceLength>
        | swMerge.Sw1536 -> 2000<ceLength>
        | swMerge.Sw2048 -> 2000<ceLength>
        | swMerge.Sw3072 -> 1000<ceLength>
        | swMerge.Sw4096 -> 1000<ceLength>
        | swMerge.Sw6144 -> 5000<ceLength>
        | swMerge.Sw8192 -> 5000<ceLength>
                 

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


   let getP900CeCountForMerge (swMerege:swMerge) : int<ceLength> =
       match swMerege with
        | swMerge.Sw4 -> 60<ceLength>
        | swMerge.Sw6 -> 80<ceLength>
        | swMerge.Sw8 -> 120<ceLength>
        | swMerge.Sw12 -> 300<ceLength>
        | swMerge.Sw16 -> 600<ceLength>
        | swMerge.Sw24 -> 1600<ceLength>
        | swMerge.Sw32 -> 8000<ceLength>
        | swMerge.Sw48 -> 10000<ceLength>
        | swMerge.Sw64 -> 10000<ceLength>
        | swMerge.Sw96 -> 50000<ceLength>
        | swMerge.Sw128 -> 50000<ceLength>
        | swMerge.Sw192 -> 25000<ceLength>
        | swMerge.Sw256 -> 25000<ceLength>
        | swMerge.Sw384 -> 10000<ceLength>
        | swMerge.Sw512 -> 10000<ceLength>
        | swMerge.Sw768 -> 50000<ceLength>
        | swMerge.Sw1024 -> 50000<ceLength>
        | swMerge.Sw1536 -> 20000<ceLength>
        | swMerge.Sw2048 -> 20000<ceLength>
        | swMerge.Sw3072 -> 10000<ceLength>
        | swMerge.Sw4096 -> 10000<ceLength>
        | swMerge.Sw6144 -> 50000<ceLength>
        | swMerge.Sw8192 -> 50000<ceLength>


   let getP999CeCountForFull (swFull:swFull) : int<ceLength> =
       match swFull with
        | swFull.Sw4 -> 300<ceLength>
        | swFull.Sw6 -> 600<ceLength>
        | swFull.Sw8 -> 700<ceLength>
        | swFull.Sw12 -> 1000<ceLength>
        | swFull.Sw16 -> 1600<ceLength>
        | swFull.Sw24 -> 3000<ceLength>
        | swFull.Sw32 -> 10000<ceLength>
        | swFull.Sw48 -> 100000<ceLength>
        | swFull.Sw64 -> 100000<ceLength>


   let getP999CeCountForMerge (swMerege:swMerge) : int<ceLength> =
       match swMerege with
        | swMerge.Sw4 -> 200<ceLength>
        | swMerge.Sw6 -> 400<ceLength>
        | swMerge.Sw8 -> 600<ceLength>
        | swMerge.Sw12 -> 700<ceLength>
        | swMerge.Sw16 -> 1800<ceLength>
        | swMerge.Sw24 -> 2000<ceLength>
        | swMerge.Sw32 -> 12000<ceLength>
        | swMerge.Sw48 -> 8000<ceLength>
        | swMerge.Sw64 -> 24000<ceLength>
        | swMerge.Sw96 -> 8000<ceLength>
        | swMerge.Sw128 -> 100000<ceLength>
        | swMerge.Sw192 -> 8000<ceLength>
        | swMerge.Sw256 -> 300000<ceLength>
        | swMerge.Sw384 -> 8000<ceLength>
        | swMerge.Sw512 -> 8000<ceLength>
        | swMerge.Sw768 -> 8000<ceLength>
        | swMerge.Sw1024 -> 8000<ceLength>
        | swMerge.Sw1536 -> 8000<ceLength>
        | swMerge.Sw2048 -> 8000<ceLength>
        | swMerge.Sw3072 -> 8000<ceLength>
        | swMerge.Sw4096 -> 8000<ceLength>
        | swMerge.Sw6144 -> 8000<ceLength>
        | swMerge.Sw8192 -> 8000<ceLength>



