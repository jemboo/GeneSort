namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter

// Stage lengths for different sorting widths and probabilities for full sort
//SortProb	4-Mcse	4-Msrs	4-Mssi	4-Msuf4	6-Mcse	6-Msrs	 6-Mssi	 6-Msuf6  8-Mcse	8-Msrs	8-Mssi	8-Msuf4	12-Mcse	12-Msrs	12-Mssi	12-Msuf6  16-Mcse  16-Msrs	16-Mssi	 16-Msuf4  24-Mcse	24-Msrs	24-Mssi	24-Msuf6
//0.25	    1	    3	    3	    3	     6	     9	      9	      7	        13	     16	     18	     16	     31	     37	     36	     41	        51	      63	  58	   71	     96	     120	 104	 192
//0.5	    2	    4	    4	    4	     8	    12	     12	      9	        17	     21	     22	     21	     37	     46	     43	     52	        60	      77	  67	   88	    111	     145	 120	 231
//0.75	    2	    6	    5	    6	    11	    15	     15	     13	        21	     28	     27	     28	     45	     56	     52	     64	        71	      94	  80	  111	    129	     174	 141	 279
//0.9	    4	    8	    7	    8	    14	    18	     17	     16	        26	     35	     33	     35	     53	     68	     62 	 73	        83	     113	  93	  136	    149	     211	 162	 332
//0.99	    6	    12	    12	    13	    20	    19	     19	     19	        40	     51	     49	     51	     71	     77	     75	     79	       112	     149	  133	  180	    187	     291	 215	 388


module StageLength =

    let fromString (s: string) : int<stageLength> =
        // Ensure the string is not null or empty
        if String.IsNullOrEmpty(s) then
            failwith "Stage count string cannot be null or empty"
        try
            System.Int32.Parse(s) |> UMX.tag<stageLength>
        with 
        | :? System.FormatException as ex ->
            failwithf "Invalid stage count string format: %s. Error: %s" s ex.Message




    //// **********  Full sort   *****************


    let getRecordStageLengthForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


    let get0P5StageLengthForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


         
    let get0P9StageLengthForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)

         
    let get0P99StageLengthForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)

    let get0P999StageLengthForFull (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 3<stageLength>
         | 6 -> 5<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 8<stageLength>
         | 16 -> 9<stageLength>
         | 24 -> 12<stageLength>
         | 32 -> 14<stageLength>
         | 48 -> 18<stageLength>
         | 64 -> 20<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)





    //// ********** n/2 Merge sort   *****************


    let getRecordStageLengthForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


    let get0P5StageLengthForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


    let get0P9StageLengthForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)



    let get0P99StageLengthForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)


         
    let get0P999StageLengthForMerge (sw:int<sortingWidth>) : int<stageLength> =
        match %sw with
         | 4 -> 1<stageLength>
         | 6 -> 3<stageLength>
         | 8 -> 6<stageLength>
         | 12 -> 4<stageLength>
         | 16 -> 4<stageLength>
         | 24 -> 5<stageLength>
         | 32 -> 6<stageLength>
         | 48 -> 7<stageLength>
         | 64 -> 8<stageLength>
         | 96 -> 10<stageLength>
         | 128 -> 12<stageLength>
         | 192 -> 14<stageLength>
         | 256 -> 19<stageLength>
         | 384 -> 20<stageLength>
         | 512 -> 21<stageLength>
         | 768 -> 22<stageLength>
         | 1024 -> 23<stageLength>
         | 1536 -> 24<stageLength>
         | 2048 -> 25<stageLength>
         | 3072 -> 26<stageLength>
         | 4096 -> 27<stageLength>
         | 6144 -> 28<stageLength>
         | 8192 -> 29<stageLength>
         | _ -> failwith (sprintf "sortingWidth %d not handled" %sw)

