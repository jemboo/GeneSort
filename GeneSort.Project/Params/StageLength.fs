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


// Stage lengths for different sorting widths and probabilities for n/2 merge sort
//SortProb	32-Mcse	    32-Msrs	    32-Mssi	    32-Msuf4	48-Mcse	    48-Msrs	    48-Mssi	    48-Msuf6	64-Mcse	    64-Msrs	    64-Mssi	    64-Msuf4	96-Mcse	    96-Msrs	    96-Mssi	    96-Msuf6	128-Mcse	128-Msrs	128-Mssi	128-Msuf4	192-Mcse	192-Msrs	192-Mssi	192-Msuf6	256-Mcse	256-Msrs	256-Mssi	256-Msuf4	384-Mcse	384-Msrs	384-Mssi	384-Msuf6
//0.25	    125	        157	        131	        225	        223	        281	        230	        541	        319	        409	        324	        900	        532	        718	        545	        2298	    772	        1032	    786	        3333	    1254	    1682	    1251	    8203	    1704	    2351	    1743	    11304	    2823	    3867	    2892	    28969
//0.5	    146	        186	        153	        272	        254	        325	        260	        649	        359	        471	        375	        1034	    599	        819	        618	        2722	    866	        1134	    869	        3845	    1376	    1907	    1375	    9585	    1873	    2567	    1925	    12815	    3069	    4156	    3142	    33700
//0.75	    169	        227	        180	        323	        301	        386	        305	        773	        418	        552	        431	        1262	    683	        944	        700	        3191	    981	        1318	    976	        4429	    1550	    2134	    1544	    11109	    2095	    2941	    2240	    15041	    3399	    4718	    3447	    38926
//0.9	    199	        268	        214	        368	        354	        469	        357	        879	        488	        648	        497	        1458	    769	        1125	    773	        3707	    1119	    1521	    1091	    5259	    1715	    2402	    1742	    12563	    2407	    3208	    2456	    16778	    3628	    5068	    3712	    40929
//0.99	    290	        351	        289	        398	        445	        615	        467	        984	        645	        853	        714	        1901	    933	        1482	    981	        4600	    1404	    1824	    1404	    6818	    2220	    2976	    2132	    16480	    2700	    3717	    3304	    21553	    4793	    5873	    4884	    46606






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

