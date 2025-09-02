namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter

type swFull =
    | Sw4  | Sw6  | Sw8  | Sw12  | Sw16  | Sw24  | Sw32  | Sw48  | Sw64 

module SwFull =

   let all = [Sw4; Sw6; Sw8; Sw12; Sw16; Sw24; Sw32; Sw48; Sw64]

   let fromString (s: string) : swFull =
        match s with
        | "4" -> Sw4
        | "6" -> Sw6
        | "8" -> Sw8
        | "12" -> Sw12
        | "16" -> Sw16
        | "24" -> Sw24
        | "32" -> Sw32
        | "48" -> Sw48
        | "64" -> Sw64
        | _ -> failwithf "Unsupported swFull string: %s" s

   let toString (swFull:swFull) : string =
        match swFull with
        | Sw4 -> "4"
        | Sw6 -> "6"
        | Sw8 -> "8"
        | Sw12 -> "12"
        | Sw16 -> "16"
        | Sw24 -> "24"
        | Sw32 -> "32"
        | Sw48 -> "48"
        | Sw64 -> "64"

   let toSortingWidth (swFull:swFull) : int<sortingWidth> =
        match swFull with
        | Sw4 -> 4<sortingWidth>
        | Sw6 -> 6<sortingWidth>
        | Sw8 -> 8<sortingWidth>
        | Sw12 -> 12<sortingWidth>
        | Sw16 -> 16<sortingWidth>
        | Sw24 -> 24<sortingWidth>
        | Sw32 -> 32<sortingWidth>
        | Sw48 -> 48<sortingWidth>
        | Sw64 -> 64<sortingWidth>


   let getSwFullForSortingWidth (sortingWidth:int<sortingWidth>) : swFull =
       match %sortingWidth with
        | 4 -> swFull.Sw4
        | 6 -> swFull.Sw6
        | 8 -> swFull.Sw8
        | 12 -> swFull.Sw12
        | 16 -> swFull.Sw16
        | 24 -> swFull.Sw24
        | 32 -> swFull.Sw32
        | 48 -> swFull.Sw48
        | 64 -> swFull.Sw64
        | _ -> failwith "Unsupported sorting width"


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



   let standardMapVals() : string*string list =
       ("SortingWidth", ["8"; "16"; "32"; "64"]) 
       
   let practicalFullTestVals() : string*string list =
       ("SortingWidth", ["16";]) 



type swMerege =
    | Sw4  | Sw6  | Sw8  | Sw12  | Sw16  | Sw24  | Sw32  | Sw48  | Sw64  | Sw96
    | Sw128 | Sw192  | Sw256  | Sw384  | Sw512  | Sw768  | Sw1024  | Sw1536
    | Sw2048  | Sw3072  | Sw4096  | Sw6144  | Sw8192

module SwMerege =

   let all = 
        [Sw4; Sw6; Sw8; Sw12; Sw16; Sw24; Sw32; Sw48; Sw64; Sw96;
         Sw128; Sw192; Sw256; Sw384; Sw512; Sw768; Sw1024; Sw1536;
         Sw2048; Sw3072; Sw4096; Sw6144; Sw8192]


   let fromString (s: string) : swMerege =
        match s with
        | "4" -> Sw4  | "6" -> Sw6  | "8" -> Sw8  | "12" -> Sw12
        | "16" -> Sw16  | "24" -> Sw24  | "32" -> Sw32  | "48" -> Sw48
        | "64" -> Sw64  | "96" -> Sw96  | "128" -> Sw128  | "192" -> Sw192
        | "256" -> Sw256 | "384" -> Sw384  | "512" -> Sw512  | "768" -> Sw768
        | "1024" -> Sw1024 | "1536" -> Sw1536  | "2048" -> Sw2048  | "3072" -> Sw3072
        | "4096" -> Sw4096 | "6144" -> Sw6144  | "8192" -> Sw8192
        | _ -> failwithf "Unsupported swMerege string: %s" s


   let toString (swMerege:swMerege) : string =
        match swMerege with
        | Sw4 -> "4"  | Sw6 -> "6"  | Sw8 -> "8"  | Sw12 -> "12"
        | Sw16 -> "16"  | Sw24 -> "24"  | Sw32 -> "32"  | Sw48 -> "48"
        | Sw64 -> "64"  | Sw96 -> "96"  | Sw128 -> "128"  | Sw192 -> "192"
        | Sw256 -> "256" | Sw384 -> "384"  | Sw512 -> "512"  | Sw768 -> "768"
        | Sw1024 -> "1024" | Sw1536 -> "1536"  | Sw2048 -> "2048"  | Sw3072 -> "3072"
        | Sw4096 -> "4096" | Sw6144 -> "6144"  | Sw8192 -> "8192"


   let getSwMergeForSortingWidth (sortingWidth:int<sortingWidth>) : swMerege =
        match %sortingWidth with
        | 4 -> swMerege.Sw4  | 6 -> swMerege.Sw6  | 8 -> swMerege.Sw8  | 12 -> swMerege.Sw12
        | 16 -> swMerege.Sw16  | 24 -> swMerege.Sw24  | 32 -> swMerege.Sw32  | 48 -> swMerege.Sw48
        | 64 -> swMerege.Sw64  | 96 -> swMerege.Sw96  | 128 -> swMerege.Sw128  | 192 -> swMerege.Sw192
        | 256 -> swMerege.Sw256 | 384 -> swMerege.Sw384  | 512 -> swMerege.Sw512  | 768 -> swMerege.Sw768
        | 1024 -> swMerege.Sw1024 | 1536 -> swMerege.Sw1536  | 2048 -> swMerege.Sw2048  | 3072 -> swMerege.Sw3072
        | 4096 -> swMerege.Sw4096 | 6144 -> swMerege.Sw6144  | 8192 -> swMerege.Sw8192
        | _ -> failwith "Unsupported sorting width"
         