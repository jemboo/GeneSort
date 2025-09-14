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




    //// **********  Full sort   *****************


   let getRecordCeLengthForFull (sw:int<sortingWidth>) : int<ceLength> =
       match %sw with
        | 4 -> 5<ceLength>
        | 6 -> 12<ceLength>
        | 8 -> 19<ceLength>
        | 12 -> 39<ceLength>
        | 16 -> 60<ceLength>
        | 24 -> 120<ceLength>
        | 32 -> 185<ceLength>
        | 48 -> 346<ceLength>
        | 64 -> 521<ceLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)



   let get0P5CeLengthForFull (sw:int<sortingWidth>) : int<ceLength> =
       match %sw with
        | 4 -> 5<ceLength>
        | 6 -> 12<ceLength>
        | 8 -> 19<ceLength>
        | 12 -> 39<ceLength>
        | 16 -> 60<ceLength>
        | 24 -> 120<ceLength>
        | 32 -> 185<ceLength>
        | 48 -> 346<ceLength>
        | 64 -> 521<ceLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)



   let get0P9CeLengthForFull (sw:int<sortingWidth>) : int<ceLength> =
       match %sw with
        | 4 -> 5<ceLength>
        | 6 -> 12<ceLength>
        | 8 -> 19<ceLength>
        | 12 -> 39<ceLength>
        | 16 -> 60<ceLength>
        | 24 -> 120<ceLength>
        | 32 -> 185<ceLength>
        | 48 -> 346<ceLength>
        | 64 -> 521<ceLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)



   let get0P99CeLengthForFull (sw:int<sortingWidth>) : int<ceLength> =
       match %sw with
        | 4 -> 5<ceLength>
        | 6 -> 12<ceLength>
        | 8 -> 19<ceLength>
        | 12 -> 39<ceLength>
        | 16 -> 60<ceLength>
        | 24 -> 120<ceLength>
        | 32 -> 185<ceLength>
        | 48 -> 346<ceLength>
        | 64 -> 521<ceLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


   let get0P999CeLengthForFull (sw:int<sortingWidth>) : int<ceLength> =
       match %sw with
        | 4 -> 5<ceLength>
        | 6 -> 12<ceLength>
        | 8 -> 19<ceLength>
        | 12 -> 39<ceLength>
        | 16 -> 60<ceLength>
        | 24 -> 120<ceLength>
        | 32 -> 185<ceLength>
        | 48 -> 346<ceLength>
        | 64 -> 521<ceLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)




    //// ********** n/2 Merge sort   *****************


   let getRecordCeLengthForMerge (sw:int<sortingWidth>) : int<ceLength> =
        match %sw with
        | 4 -> 3<ceLength>
        | 6 -> 9<ceLength>
        | 8 -> 16<ceLength>
        | 12 -> 34<ceLength>
        | 16 -> 54<ceLength>
        | 24 -> 110<ceLength>
        | 32 -> 174<ceLength>
        | 48 -> 326<ceLength>
        | 64 -> 498<ceLength>
        | 96 -> 5000<ceLength>
        | 128 -> 5000<ceLength>
        | 192 -> 2500<ceLength>
        | 256 -> 2500<ceLength>
        | 384 -> 1000<ceLength>
        | 512 -> 1000<ceLength>
        | 768 -> 5000<ceLength>
        | 1024 -> 5000<ceLength>
        | 1536 -> 2000<ceLength>
        | 2048 -> 2000<ceLength>
        | 3072 -> 1000<ceLength>
        | 4096 -> 1000<ceLength>
        | 6144 -> 5000<ceLength>
        | 8192 -> 5000<ceLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)
                 
