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



   let standardMapVals() : string*string list =
       ("SortingWidth", ["8"; "16"; "32"; "64"]) 
       
   let practicalFullTestVals() : string*string list =
       ("SortingWidth", ["16";]) 

type swMerge =
    | Sw4  | Sw6  | Sw8  | Sw12  | Sw16  | Sw24  | Sw32  | Sw48  | Sw64  | Sw96
    | Sw128 | Sw192  | Sw256  | Sw384  | Sw512  | Sw768  | Sw1024  | Sw1536
    | Sw2048  | Sw3072  | Sw4096  | Sw6144  | Sw8192

module SwMerge =

   let tst4 = 
        [Sw4; Sw8; Sw16; Sw32; Sw64;
         Sw128; Sw256; Sw512; Sw1024;]

   let all = 
        [Sw4; Sw6; Sw8; Sw12; Sw16; Sw24; Sw32; Sw48; Sw64; Sw96;
         Sw128; Sw192; Sw256; Sw384; Sw512; Sw768; Sw1024; Sw1536;
         Sw2048; Sw3072; Sw4096; Sw6144; Sw8192]

   let fromString (s: string) : swMerge =
        match s with
        | "4" -> Sw4  | "6" -> Sw6  | "8" -> Sw8  | "12" -> Sw12
        | "16" -> Sw16  | "24" -> Sw24  | "32" -> Sw32  | "48" -> Sw48
        | "64" -> Sw64  | "96" -> Sw96  | "128" -> Sw128  | "192" -> Sw192
        | "256" -> Sw256 | "384" -> Sw384  | "512" -> Sw512  | "768" -> Sw768
        | "1024" -> Sw1024 | "1536" -> Sw1536  | "2048" -> Sw2048  | "3072" -> Sw3072
        | "4096" -> Sw4096 | "6144" -> Sw6144  | "8192" -> Sw8192
        | _ -> failwithf "Unsupported swMerege string: %s" s


   let toString (swMerege:swMerge) : string =
        match swMerege with
        | Sw4 -> "4"  | Sw6 -> "6"  | Sw8 -> "8"  | Sw12 -> "12"
        | Sw16 -> "16"  | Sw24 -> "24"  | Sw32 -> "32"  | Sw48 -> "48"
        | Sw64 -> "64"  | Sw96 -> "96"  | Sw128 -> "128"  | Sw192 -> "192"
        | Sw256 -> "256" | Sw384 -> "384"  | Sw512 -> "512"  | Sw768 -> "768"
        | Sw1024 -> "1024" | Sw1536 -> "1536"  | Sw2048 -> "2048"  | Sw3072 -> "3072"
        | Sw4096 -> "4096" | Sw6144 -> "6144"  | Sw8192 -> "8192"


   let getSwMergeForSortingWidth (sortingWidth:int<sortingWidth>) : swMerge =
        match %sortingWidth with
        | 4 -> swMerge.Sw4  | 6 -> swMerge.Sw6  | 8 -> swMerge.Sw8  | 12 -> swMerge.Sw12
        | 16 -> swMerge.Sw16  | 24 -> swMerge.Sw24  | 32 -> swMerge.Sw32  | 48 -> swMerge.Sw48
        | 64 -> swMerge.Sw64  | 96 -> swMerge.Sw96  | 128 -> swMerge.Sw128  | 192 -> swMerge.Sw192
        | 256 -> swMerge.Sw256 | 384 -> swMerge.Sw384  | 512 -> swMerge.Sw512  | 768 -> swMerge.Sw768
        | 1024 -> swMerge.Sw1024 | 1536 -> swMerge.Sw1536  | 2048 -> swMerge.Sw2048  | 3072 -> swMerge.Sw3072
        | 4096 -> swMerge.Sw4096 | 6144 -> swMerge.Sw6144  | 8192 -> swMerge.Sw8192
        | _ -> failwith "Unsupported sorting width"

   let toSortingWidth (swMerege:swMerge) : int<sortingWidth> =
        match swMerege with
        | Sw4 -> 4<sortingWidth>
        | Sw6 -> 6<sortingWidth>
        | Sw8 -> 8<sortingWidth>
        | Sw12 -> 12<sortingWidth>
        | Sw16 -> 16<sortingWidth>
        | Sw24 -> 24<sortingWidth>
        | Sw32 -> 32<sortingWidth>
        | Sw48 -> 48<sortingWidth>
        | Sw64 -> 64<sortingWidth>
        | Sw96 -> 96<sortingWidth>
        | Sw128 -> 128<sortingWidth>
        | Sw192 -> 192<sortingWidth>
        | Sw256 -> 256<sortingWidth>
        | Sw384 -> 384<sortingWidth>
        | Sw512 -> 512<sortingWidth>
        | Sw768 -> 768<sortingWidth>
        | Sw1024 -> 1024<sortingWidth>
        | Sw1536 -> 1536<sortingWidth>
        | Sw2048 -> 2048<sortingWidth>
        | Sw3072 -> 3072<sortingWidth>
        | Sw4096 -> 4096<sortingWidth>
        | Sw6144 -> 6144<sortingWidth>
        | Sw8192 -> 8192<sortingWidth>



   let exp4Vals() : string*string list =
       ("SortingWidth", ["16"; "32"; "64"; "128"; "256"]) 



   let exp5Vals() : string*string list =
       ("SortingWidth", ["64"; "128"; "256"; "512"]) 

         