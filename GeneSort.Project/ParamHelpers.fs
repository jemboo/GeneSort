namespace GeneSort.Project
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter

type IntegerRange = { Min: int; Max: int }

type sortingSuccess = 
    | Record
    | P900
    | P999

type swFull =
    | Sw4  | Sw6  | Sw8  | Sw12  | Sw16  | Sw24  | Sw32  | Sw48  | Sw64 

type swMerege =
    | Sw4  | Sw6  | Sw8  | Sw12  | Sw16  | Sw24  | Sw32  | Sw48  | Sw64  | Sw96
    | Sw128 | Sw192  | Sw256  | Sw384  | Sw512  | Sw768  | Sw1024  | Sw1536
    | Sw2048  | Sw3072  | Sw4096  | Sw6144  | Sw8192


module ParamHelpers =

   let getRange (index:int) (extent:int) :IntegerRange =
        {
            IntegerRange.Min = index * extent
            IntegerRange.Max = (index + 1) * extent - 1  
        }

   let getSortingWidths () : string*string list =
       ("SortingWidth", ["8"; "16"; "32"; "64"]) 
       
   
   let getSorterModels () : string*string list =
       ("SorterModel", ["Mcse"; "Mssi"; "Msrs"; "Msuf4"])

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


   let getSwMeregeForSortingWidth (sortingWidth:int<sortingWidth>) : swMerege =
        match %sortingWidth with
        | 4 -> swMerege.Sw4  | 6 -> swMerege.Sw6  | 8 -> swMerege.Sw8  | 12 -> swMerege.Sw12
        | 16 -> swMerege.Sw16  | 24 -> swMerege.Sw24  | 32 -> swMerege.Sw32  | 48 -> swMerege.Sw48
        | 64 -> swMerege.Sw64  | 96 -> swMerege.Sw96  | 128 -> swMerege.Sw128  | 192 -> swMerege.Sw192
        | 256 -> swMerege.Sw256 | 384 -> swMerege.Sw384  | 512 -> swMerege.Sw512  | 768 -> swMerege.Sw768
        | 1024 -> swMerege.Sw1024 | 1536 -> swMerege.Sw1536  | 2048 -> swMerege.Sw2048  | 3072 -> swMerege.Sw3072
        | 4096 -> swMerege.Sw4096 | 6144 -> swMerege.Sw6144  | 8192 -> swMerege.Sw8192
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




   let getCeCountForFullSortingSuccess (sortingSuccess:sortingSuccess) 
                (sortingWidth:int<sortingWidth>) : int<ceCount> =
       match sortingSuccess with
        | sortingSuccess.Record -> 
            let swFull = getSwFullForSortingWidth sortingWidth
            getRecordCeCountForFull swFull
        | sortingSuccess.P900 -> 
            let swFull = getSwFullForSortingWidth sortingWidth
            getP900CeCountForFull swFull
        | sortingSuccess.P999 -> 
            let swFull = getSwFullForSortingWidth sortingWidth
            getP999CeCountForFull swFull



   let getCeCountForMergeSortingSuccess (sortingSuccess:sortingSuccess) 
            (sortingWidth:int<sortingWidth>) : int<ceCount> = 
       match sortingSuccess with
        | sortingSuccess.Record -> 
            let swMerege = getSwMeregeForSortingWidth sortingWidth
            getRecordCeCountForMerge swMerege
        | sortingSuccess.P900 -> 
            let swMerege = getSwMeregeForSortingWidth sortingWidth
            getP900CeCountForMerge swMerege
        | sortingSuccess.P999 -> 
            let swMerege = getSwMeregeForSortingWidth sortingWidth
            getP999CeCountForMerge swMerege