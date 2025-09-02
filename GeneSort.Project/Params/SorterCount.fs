namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter

module SorterCount = 
    let fromString (s: string) : int<sorterCount> =
        // Ensure the string is not null or empty
        if String.IsNullOrEmpty(s) then
            failwith "Sorter count string cannot be null or empty"
        try
            System.Int32.Parse(s) |> UMX.tag<sorterCount>
        with 
        | :? System.FormatException as ex ->
            failwithf "Invalid sorter count string format: %s. Error: %s" s ex.Message

    let getSorterCountForSwFull (swFull:swFull) : int<sorterCount> =
        match swFull with
        | swFull.Sw4 -> 1000<sorterCount>
        | swFull.Sw6 -> 1000<sorterCount>
        | swFull.Sw8 -> 1000<sorterCount>
        | swFull.Sw12 -> 1000<sorterCount>
        | swFull.Sw16 -> 50<sorterCount>
        | swFull.Sw24 -> 10<sorterCount>
        | swFull.Sw32 -> 10<sorterCount>
        | swFull.Sw48 -> 2<sorterCount>
        | swFull.Sw64 -> 1<sorterCount>

    let getSorterCountForSwMerge (swMerege:swMerege) : int<sorterCount> =
        match swMerege with
        | swMerege.Sw4 -> 1000<sorterCount>
        | swMerege.Sw6 -> 1000<sorterCount>
        | swMerege.Sw8 -> 1000<sorterCount>
        | swMerege.Sw12 -> 1000<sorterCount>
        | swMerege.Sw16 -> 1000<sorterCount>
        | swMerege.Sw24 -> 1000<sorterCount>
        | swMerege.Sw32 -> 1000<sorterCount>
        | swMerege.Sw48 -> 1000<sorterCount>
        | swMerege.Sw64 -> 1000<sorterCount>
        | swMerege.Sw96 -> 500<sorterCount>
        | swMerege.Sw128 -> 500<sorterCount>
        | swMerege.Sw192 -> 250<sorterCount>
        | swMerege.Sw256 -> 250<sorterCount>
        | swMerege.Sw384 -> 100<sorterCount>
        | swMerege.Sw512 -> 100<sorterCount>
        | swMerege.Sw768 -> 50<sorterCount>
        | swMerege.Sw1024 -> 50<sorterCount>
        | swMerege.Sw1536 -> 25<sorterCount>
        | swMerege.Sw2048 -> 25<sorterCount>
        | swMerege.Sw3072 -> 10<sorterCount>
        | swMerege.Sw4096 -> 5<sorterCount>
        | swMerege.Sw6144 -> 2<sorterCount>
        | swMerege.Sw8192 -> 1<sorterCount>