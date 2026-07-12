namespace GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Sorting
open GeneSort.SortingOps
open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter

module CommonParams =

    let CollectSortableTests = true
    let ExcludeSelfCe = true |> UMX.tag<excludeSelfCe>

    // SimpleSorterModelTypes
    let allSimpleSorterModelTypes = 
            (runParameters.simpleSorterModelTypeKey, SimpleSorterModelType.all() 
            |> List.map SimpleSorterModelType.toString)

    let msceModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)

    let msrsModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msrs] |> List.map SimpleSorterModelType.toString)

    let mssiModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Mssi] |> List.map SimpleSorterModelType.toString)

    let msuf4ModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msuf4] |> List.map SimpleSorterModelType.toString)


    // SorterCounts
    let testSorterCount = (runParameters.sorterCountKey, ["1000";] )
    let smallSorterCount = (runParameters.sorterCountKey, ["100";] )
    let mediumSorterCount = (runParameters.sorterCountKey, ["1000";] )
    let largeSorterCount = (runParameters.sorterCountKey, ["10000";] )
    let extraLargeSorterCount = (runParameters.sorterCountKey, ["100000";] )

    
    // SorterCounts per pool
    let oneSorterPerPool = (runParameters.sorterCountPerPoolKey, ["1";] )
    let fourSortersPerPool = (runParameters.sorterCountPerPoolKey, ["4"] )
    let oneAndTwoSortersPerPool = (runParameters.sorterCountPerPoolKey, ["1"; "2";] )
    let oneToFourSortersPerPool = (runParameters.sorterCountPerPoolKey, ["1"; "2"; "4"] )
    let fourTo32SortersPerPool = (runParameters.sorterCountPerPoolKey, ["4"; "8"; "16"; "32"] )
    let oneTwenty8SortersPerPool = (runParameters.sorterCountPerPoolKey, ["128";] )
    let fiveTwelveSortersPerPool = (runParameters.sorterCountPerPoolKey, ["512";] )
    let oneKSortersPerPool = (runParameters.sorterCountPerPoolKey, ["1024";] )
    let twoKSortersPerPool = (runParameters.sorterCountPerPoolKey, ["2048";] )
    let fourKSortersPerPool = (runParameters.sorterCountPerPoolKey, ["4096";] )
    let eightKSortersPerPool = (runParameters.sorterCountPerPoolKey, ["8192";] )


    // SorterPoolCounts
    let poolCount1 = (runParameters.sorterPoolCountKey, ["1";] )
    let poolCount2 = (runParameters.sorterPoolCountKey, ["2";] )
    let poolCount5 = (runParameters.sorterPoolCountKey, ["5";] )
    let poolCount10 = (runParameters.sorterPoolCountKey, ["10";] )
    let poolCount30 = (runParameters.sorterPoolCountKey, ["30";] )
    let poolCount100 = (runParameters.sorterPoolCountKey, ["100";] )
    let poolCount1000 = (runParameters.sorterPoolCountKey, ["1000";] )

    // SorterChildCounts
    let oneChildCount = (runParameters.sorterChildCountKey, ["1";] )
    let twoChildCount = (runParameters.sorterChildCountKey, ["2";] )
    let oneAndTwoChildCount = (runParameters.sorterChildCountKey, ["1"; "2";])
    let oneToFourChildCount = (runParameters.sorterChildCountKey, ["1"; "2"; "4";])
    let testChildCount = (runParameters.sorterChildCountKey, ["1000";] )
    let smallChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let mediumChildCount = (runParameters.sorterChildCountKey, ["100";] )
    let largeChildCount = (runParameters.sorterChildCountKey, ["1000";] )
    let extraLargeChildCount = (runParameters.sorterChildCountKey, ["10000";] )

    let _sorterEvalTypeV1 = sorterEvalType.V1
    let _sorterEvalTypeV2 = sorterEvalType.V2

    let sorterEvalTypeV1 = 
            (runParameters.sorterEvalTypeKey, 
            [ sorterEvalType.V1 ;] |> List.map SorterEvalType.toString)

    let sorterEvalTypeV2 = 
            (runParameters.sorterEvalTypeKey, 
            [ sorterEvalType.V2 ;] |> List.map SorterEvalType.toString)


    let _rngTypeLcg = rngType.Lcg

    // MergeDimensions
    let testMergeDimensions = 
        (runParameters.mergeDimensionKey, [8;] |> List.map string)
    let allMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4; 6; 8] |> List.map string)
    let mergeDimension2 = 
            (runParameters.mergeDimensionKey, [2;] |> List.map string)
    let mergeDimension3 = 
            (runParameters.mergeDimensionKey, [3;] |> List.map string)
    let mergeDimension4 = 
            (runParameters.mergeDimensionKey, [4;] |> List.map string)
    let mergeDimension6 = 
            (runParameters.mergeDimensionKey, [6;] |> List.map string)
    let mergeDimension8 = 
            (runParameters.mergeDimensionKey, [8;] |> List.map string)
    let lowMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4;] |> List.map string)
    let highMergeDimensions = 
            (runParameters.mergeDimensionKey, [6; 8] |> List.map string)
    

    // SortingWidths
    let testSortingWidths = 
            (runParameters.sortingWidthKey, [4;5;6;7;8;9;10;11;12] |> List.map string)
    let sortingWidth16 = 
            (runParameters.sortingWidthKey, [16] |> List.map string)
    let sortingWidth32 = 
            (runParameters.sortingWidthKey, [32] |> List.map string)
    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [4;5;6;7;8;9;10;11;12] |> List.map string)
    let mediumSortingWidths = 
            (runParameters.sortingWidthKey, [14;16;18;20;22] |> List.map string)


    // MergeSortingWidths
    let testMergeSortingWidths = 
            (runParameters.sortingWidthKey, [16; 24; 32;] |> List.map string)

    let smallMergeSortingWidths = 
            (runParameters.sortingWidthKey, [16; 18; 24; 32; 36; 48; 64] |> List.map string)  
            
    let mediumMergeSortingWidths = 
            (runParameters.sortingWidthKey,  [96; 128;]  |> List.map string)

    let sortingWidth96 = 
            (runParameters.sortingWidthKey,  [96;]  |> List.map string)

    let largeMergeSortingWidths = 
            (runParameters.sortingWidthKey,  [192; 256; 512]  |> List.map string)

    let smallP2MergeSortingWidths = 
            (runParameters.sortingWidthKey, [16; 32; 64;] |> List.map string)

    let mediumP2MergeSortingWidths = 
            (runParameters.sortingWidthKey, [128;] |> List.map string)

    let largeP2MergeSortingWidths = 
            (runParameters.sortingWidthKey, [256; 512;] |> List.map string)


    // RngType
    let rngTypeLcg = 
            (runParameters.rngTypeKey, [_rngTypeLcg] |> List.map RngType.toString)


    // DataFormats
    let dataFormatInt8v512 = 
            (runParameters.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            
    let dataFomatBitv512 = 
            (runParameters.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)

    let noSuffixSuffixType = 
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)


    // MutationRates
    let mutationRates =
            (runParameters.mutationRateKey, [1.0] |> List.map string)
    let insertionRates =
            (runParameters.insertionRateKey, [0.1;] |> List.map string)
    let deletionRates =
            (runParameters.deletionRateKey, [0.1;] |> List.map string)

    let orthoRate =    (runParameters.orthoRateKey, [4.001;] |> List.map string)
    let paraRate =     (runParameters.paraRateKey, [0.4;] |> List.map string)
    let paraRates =     (runParameters.paraRateKey, [0.4; 0.6] |> List.map string)
    let selfSymRate =  (runParameters.selfSymRateKey, [2.001; ] |> List.map string)
    let selfSymRates =  (runParameters.selfSymRateKey, [1.5; 2.001; ] |> List.map string)


    let seedModificationRate06 =
            (runParameters.seedModificationRateKey, [0.06] |> List.map string)
    
    let seedModificationRateR2 =
            (runParameters.seedModificationRateKey, [0.03; 0.06] |> List.map string)
    let seedModificationRate12 =
            (runParameters.seedModificationRateKey, [0.12] |> List.map string)

    let modificationRatesStageR =
            (runParameters.modificationRateKey, [ 0.08; 0.09; 0.10; 0.11; 0.12; 0.13; 0.14; 0.15;] |> List.map string)

    let modificationRatesStageRR =
            (runParameters.modificationRateKey, [ 0.02; 0.03; 0.04; 0.05; 0.07; 0.08; 0.09; 0.10;] |> List.map string)
            
    let modificationRatesStageR3 =
            (runParameters.modificationRateKey, [ 0.03; 0.04;] |> List.map string)

    let seedModificationRate10 =
            (runParameters.seedModificationRateKey, [0.10] |> List.map string)

    let seedModificationRates =
            (runParameters.seedModificationRateKey, [0.08; 0.12; 0.16] |> List.map string)

    let seedModificationRates2 =
            (runParameters.seedModificationRateKey, [0.10; 0.14] |> List.map string)

    //let modificationRatesMsce =
    //        (runParameters.modificationRateKey, [ 0.00125; 0.005; 0.02; 0.08 ] |> List.map string)

    let modificationRatesMsce =
        (runParameters.modificationRateKey, [ 0.02; 0.035; 0.045; 0.05; 0.055; 0.065; 0.08 ] |> List.map string)

    let modificationRateStage =
            (runParameters.modificationRateKey, [0.19;] |> List.map string)

    let modificationRatesStage =
            (runParameters.modificationRateKey, [ 0.04; 0.08; 0.16; 0.24;] |> List.map string)

    let modificationRates15 =
            (runParameters.modificationRateKey, [ 0.15;] |> List.map string)

    let modificationRatesStage2 =
            (runParameters.modificationRateKey, [ 0.17; 0.21; 0.25; 0.29;] |> List.map string)


    // prioritizeNewMutants
    let prioritizeNewMutantsBoth = 
            (runParameters.prioritizeNewMutantsKey, 
            [ true; false ] |> List.map string)


    let prioritizeNewMutantsTrue = 
            (runParameters.prioritizeNewMutantsKey, 
            [ true; ] |> List.map string)

    let prioritizeNewMutantsFalse = 
            (runParameters.prioritizeNewMutantsKey, 
            [ false ] |> List.map string)



    // distinctSorterHashes
    let distinctSorterHashesBoth = 
            (runParameters.distinctSorterHashesKey, [true; false] |> List.map string)

    let distinctSorterHashesTrue = 
            (runParameters.distinctSorterHashesKey, [true] |> List.map string)

    let distinctSorterHashesFalse = 
            (runParameters.distinctSorterHashesKey, [false] |> List.map string)


    // SorterLib Ids
    let _sorterLibId24_4 = 
            SorterLibId.create (24<sortingWidth>) sorterVariant.Prefix4
    let sorterLibId24_4 = 
            (runParameters.sorterLibIdKey, [_sorterLibId24_4] |> List.map SorterLibId.toString)

    let _sorterLibId28_4 = 
            SorterLibId.create (28<sortingWidth>) sorterVariant.Prefix4
    let sorterLibId28_4 = 
            (runParameters.sorterLibIdKey, [_sorterLibId28_4] |> List.map SorterLibId.toString)

    let _sorterLibId32_4 = 
            SorterLibId.create (32<sortingWidth>) sorterVariant.Prefix4
    let sorterLibId32_4 = 
            (runParameters.sorterLibIdKey, [_sorterLibId32_4] |> List.map SorterLibId.toString)


    // Sorted Fractions
    let sortedFractions = 
            (runParameters.sortedFractionKey, [0.5; 0.75; 0.90; 0.95; 0.99] |> List.map string)

    let sortedFraction90 = 
            (runParameters.sortedFractionKey, [0.90] |> List.map string)

    let sortedFraction95 = 
            (runParameters.sortedFractionKey, [0.95] |> List.map string)

    let sortedFraction99 = 
            (runParameters.sortedFractionKey, [0.99] |> List.map string)




    let getStageLength 
                (smt: simpleSorterModelType) 
                (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 4 -> 15
        | 5 -> 25
        | 6 -> 40 
        | 7 -> 50 
        | 8 -> 60
        | 9 -> 70
        | 10 -> 80
        | 11 -> 90
        | 12 -> 100
        | 14 -> 120
        | 16 -> match smt with | Msuf4 -> 300 | _ -> 150
        | 18 -> 180
        | 20 -> 200
        | 22 -> 250
        | 24 -> 300
        | 32 -> match smt with | Msuf4 -> 600 | _ -> 300
        | 36 -> 350
        | 48 -> 400
        | 64 -> match smt with | Msuf4 -> 2000 | _ -> 600
        | 96 -> 800
        | 128 -> match smt with | Msuf4 -> 4000 | _ -> 1200
        | 192 -> 2000
        | 256 -> match smt with | Msuf4 -> 6000 | _ -> 3000
        | 512 -> match smt with | Msuf4 -> 8000 | _ -> 8000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag


    let getStageLengthShort
                (smt: simpleSorterModelType) 
                (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 4 -> 5
        | 5 -> 5
        | 6 -> 10 
        | 7 -> 10 
        | 8 -> 20
        | 9 -> 20
        | 10 -> 30
        | 11 -> 40
        | 12 -> 50
        | 14 -> 60
        | 16 -> match smt with | Msuf4 -> 100 | _ -> 60
        | 18 -> 80
        | 20 -> 100
        | 22 -> 125
        | 24 -> 150
        | 32 -> match smt with | Msuf4 -> 200 | _ -> 150
        | 36 -> 150
        | 48 -> 200
        | 64 -> match smt with | Msuf4 -> 1000 | _ -> 300
        | 96 -> 800
        | 128 -> match smt with | Msuf4 -> 1500 | _ -> 600
        | 192 -> 2000
        | 256 -> match smt with | Msuf4 -> 2000 | _ -> 1000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag

