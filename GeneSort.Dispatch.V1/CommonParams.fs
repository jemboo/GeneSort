namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.Eval.V1.Sgd

module CommonParams =

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



    module IntervalDefinitions =

        /// Helper to fetch registered configuration strings by key name from the SampleRegistry
        let private getSampleStr (key: string) : string =
            match SampleRegistry.samplingConfigsDict.TryGetValue(key) with
            | true, cfg -> IntSampleMethod.toString cfg.Method
            | false, _ -> invalidArg "key" (sprintf "Key '%s' not found in SampleRegistry" key)

        // RunResult report Intervals

        // 2, 4, 6, ...
        let runResultReportInterval2 = 
            (runParameters.snapshotReportIntervalsKey, [getSampleStr "runResultReportInterval2"])

        // 10, 20, 30, ...
        let runResultReportInterval10 = 
            (runParameters.snapshotReportIntervalsKey, [getSampleStr "runResultReportInterval10"])

        // 100, 200, 300 ..
        let runResultReportInterval100 = 
            (runParameters.snapshotReportIntervalsKey, [getSampleStr "runResultReportInterval100"])

        // 500, 1000, 1500 ...
        let runResultReportInterval500 = 
            (runParameters.snapshotReportIntervalsKey, [getSampleStr "runResultReportInterval500"])

        // 1000, 2000, 3000 ...
        let runResultReportInterval1000 = 
            (runParameters.snapshotReportIntervalsKey, [getSampleStr "runResultReportInterval1000"])


        // SummaryReport Intervals

        // 1, 2, 3, 4 ..
        let summaryReport_cSampleC = 
            (runParameters.summaryReportIntervalsKey, [getSampleStr "summaryReport_cSampleC"])

        // 25, 27, 28, 29, 30, 31, 32, 34, 35, 36, 38
        let summaryReport_cSample5C = 
            (runParameters.summaryReportIntervalsKey, [getSampleStr "summaryReport_cSample5C"])

        // 25, 27, 28, 29, 31, 32, 34, 36, 37, 39
        let summaryReport_cSample1K = 
            (runParameters.summaryReportIntervalsKey, [getSampleStr "summaryReport_cSample1K"])

        // 25, 27, 29, 31, 33, 36 ...
        let summaryReport_cSample5K = 
            (runParameters.summaryReportIntervalsKey, [getSampleStr "summaryReport_cSample5K"])


        // SorterPool selection Intervals

        let sorterPoolSelectEmpty = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelectEmpty"])

        // 5, 10
        let sorterPoolSelect5_2 = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelect5_2"])

        // 5, 10 followed by empty state
        let sorterPoolSelects5_2 = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelects5_2"; getSampleStr "sorterPoolSelectEmpty"])

        // 25, 50, 75, 100, 125
        let sorterPoolSelects25_5 = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelects25_5"; getSampleStr "sorterPoolSelectEmpty"])

        // 25, 50, 100, 200, 400
        let sorterPoolSelects25_5i = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelects25_5i"; getSampleStr "sorterPoolSelectEmpty"])

        // 100, 150, 200, 250, 300 ...
        let sorterPoolSelect100_50 = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelect100_50"])

        // 100, 200, 300, 400, ...
        let sorterPoolSelect100_100 = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelect100_100"])

        // 10, 20, 40, 80, 160, ...
        let sorterPoolSelect25_20i = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelect25_20i"])

        // 50, 100, 200, 400, 800, 1600, ...
        let sorterPoolSelect50_10i = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelect50_10i"])

        // 100, 200, 400, 800, 1600, ...
        let sorterPoolSelect100_10i = 
            (runParameters.sorterPoolSelectionIntervalsKeyOld, [getSampleStr "sorterPoolSelect100_10i"])



    // SorterCounts
    let testSorterCount = (runParameters.sorterCountKey, ["1000";] )
    let smallSorterCount = (runParameters.sorterCountKey, ["100";] )
    let mediumSorterCount = (runParameters.sorterCountKey, ["1000";] )
    let largeSorterCount = (runParameters.sorterCountKey, ["10000";] )
    let veryLargeSorterCount = (runParameters.sorterCountKey, ["25000";] )
    let extraLargeSorterCount = (runParameters.sorterCountKey, ["100000";] )

    
    // SorterCounts per pool
    let oneAndTwoSortersPerPool = (runParameters.sorterCountPerPoolKey, ["1"; "2";] )
    let oneToFourSortersPerPool = (runParameters.sorterCountPerPoolKey, ["1"; "2"; "4"] )
    let fourTo32SortersPerPool = (runParameters.sorterCountPerPoolKey, ["4"; "8"; "16"; "32"] )
    let mid3SortersPerPool = (runParameters.sorterCountPerPoolKey, ["64"; "128"; "256"] )

    
    let oneSorterPerPool = (runParameters.sorterCountPerPoolKey, ["1";] )
    let twoSortersPerPool = (runParameters.sorterCountPerPoolKey, ["2";] )
    let fourSortersPerPool = (runParameters.sorterCountPerPoolKey, ["4"] )
    let eightSortersPerPool = (runParameters.sorterCountPerPoolKey, ["18";] )
    let sixteenSortersPerPool = (runParameters.sorterCountPerPoolKey, ["16";] )
    let thirtyTwoSortersPerPool = (runParameters.sorterCountPerPoolKey, ["32";] )
    let sixtyFourSortersPerPool = (runParameters.sorterCountPerPoolKey, ["64";] )
    let oneTwenty8SortersPerPool = (runParameters.sorterCountPerPoolKey, ["128";] )
    let twoFifty6SortersPerPool = (runParameters.sorterCountPerPoolKey, ["256";] )
    let fiveTwelveSortersPerPool = (runParameters.sorterCountPerPoolKey, ["512";] )
    let oneKSortersPerPool = (runParameters.sorterCountPerPoolKey, ["1024";] )
    let twoKSortersPerPool = (runParameters.sorterCountPerPoolKey, ["2048";] )
    let fourKSortersPerPool = (runParameters.sorterCountPerPoolKey, ["4096";] )
    let eightKSortersPerPool = (runParameters.sorterCountPerPoolKey, ["8192";] )

    // sorterCountCycle
    let sorterCountCycle20 = (runParameters.sorterCountCycleKey, ["20";] )
    let sorterCountCycle50 = (runParameters.sorterCountCycleKey, ["50";] )
    let sorterCountCycle100 = (runParameters.sorterCountCycleKey, ["100";] )
    let sorterCountCycle500 = (runParameters.sorterCountCycleKey, ["500";] )

    // sorterCountCycleMultiplier
    let sorterCountCycleMultiplier1 = (runParameters.sorterCountCycleMultiplierKey, ["1";] )
    let sorterCountCycleMultipliers1n2 = (runParameters.sorterCountCycleMultiplierKey, ["1"; "2";] )
    let sorterCountCycleMultipliers1n4 = (runParameters.sorterCountCycleMultiplierKey, ["1"; "4";] )
    let sorterCountCycleMultipliersLow = (runParameters.sorterCountCycleMultiplierKey, ["1.001"; "1.2"; "1.4"; "1.8"] )
    let sorterCountCycleMultiplier4 = (runParameters.sorterCountCycleMultiplierKey, ["4";] )
    let sorterCountCycleMultiplier8 = (runParameters.sorterCountCycleMultiplierKey, ["8";] )
    let sorterCountCycleMultiplier16 = (runParameters.sorterCountCycleMultiplierKey, ["16";] )


    // SorterPoolCounts
    let poolCount1 = (runParameters.sorterPoolCountKey, ["1";] )
    let poolCount2 = (runParameters.sorterPoolCountKey, ["2";] )
    let poolCount4 = (runParameters.sorterPoolCountKey, ["4";] )
    let poolCount8 = (runParameters.sorterPoolCountKey, ["8";] )
    let poolCount16 = (runParameters.sorterPoolCountKey, ["16";] )
    let poolCount32 = (runParameters.sorterPoolCountKey, ["32";] )
    let poolCount64 = (runParameters.sorterPoolCountKey, ["64";] )
    let poolCount128 = (runParameters.sorterPoolCountKey, ["128";] )
    let poolCount256 = (runParameters.sorterPoolCountKey, ["256";] )
    let poolCount512 = (runParameters.sorterPoolCountKey, ["512";] )

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
    let sortingWidth24 = 
            (runParameters.sortingWidthKey, [24] |> List.map string)
    let sortingWidth28 = 
            (runParameters.sortingWidthKey, [28] |> List.map string)
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

    let mRateOrtho =    (runParameters.orthoRateKey, [4.001;] |> List.map string)
    let mRatePara =     (runParameters.paraRateKey, [0.4;] |> List.map string)
    let mRatesPara =     (runParameters.paraRateKey, [0.4; 0.6] |> List.map string)
    let mRateSelfSym =  (runParameters.selfSymRateKey, [2.001; ] |> List.map string)
    let mRatesSelfSym =  (runParameters.selfSymRateKey, [1.5; 2.001; ] |> List.map string)



    // SeedModificationRates

    let seedModificationRate02 =
            (runParameters.seedModificationRateKey, [0.02] |> List.map string)
    let seedModificationRate03 =
            (runParameters.seedModificationRateKey, [0.03] |> List.map string)
    let seedModificationRate06 =
            (runParameters.seedModificationRateKey, [0.06] |> List.map string)
    let seedModificationRate12 =
            (runParameters.seedModificationRateKey, [0.12] |> List.map string)
    
    let seedModificationRateR2 =
            (runParameters.seedModificationRateKey, [0.03; 0.06] |> List.map string)

    let seedModificationRate10 =
            (runParameters.seedModificationRateKey, [0.10] |> List.map string)

    let seedModificationRates =
            (runParameters.seedModificationRateKey, [0.02; 0.03;] |> List.map string)

    let seedModificationRates2 =
            (runParameters.seedModificationRateKey, [0.10; 0.14] |> List.map string)



    // ModificationRates

    let modificationRatesStageR =
            (runParameters.modificationRateKey, [ 0.08; 0.09; 0.10; 0.11; 0.12; 0.13; 0.14; 0.15;] |> List.map string)

    let modificationRatesStageRR =
            (runParameters.modificationRateKey, [ 0.02; 0.03; 0.04; 0.05; 0.07; 0.08; 0.09; 0.10;] |> List.map string)
            
    let modificationRate03 =
            (runParameters.modificationRateKey, [ 0.03; ] |> List.map string)


    let modificationRatesMsce =
        (runParameters.modificationRateKey, [ 0.02; 0.035; 0.045; 0.05; 0.055; 0.065; 0.08 ] |> List.map string)

    let modificationRatesMsuf4 =
            (runParameters.modificationRateKey, [0.015; 0.02; 0.025; 0.03] |> List.map string)

    let modificationRatesMsuf4center =
            (runParameters.modificationRateKey, [0.015; 0.0175; 0.02; 0.0225; 0.025; 0.0275; 0.03; 0.0325;] |> List.map string)

    let modificationRatesStage =
            (runParameters.modificationRateKey, [ 0.04; 0.08; 0.16; 0.24;] |> List.map string)

    let modificationRates15 =
            (runParameters.modificationRateKey, [ 0.15;] |> List.map string)

    let modificationRatesStage2 =
            (runParameters.modificationRateKey, [ 0.17; 0.21; 0.25; 0.29;] |> List.map string)
            
    let modificationRatep01 =
            (runParameters.modificationRateKey, [0.01;] |> List.map string)

    let modificationRatep04 =
            (runParameters.modificationRateKey, [0.04;] |> List.map string)

    let modificationRatep06 =
            (runParameters.modificationRateKey, [0.06;] |> List.map string)

    let modificationRatesMsceCenter =
            (runParameters.modificationRateKey, [0.01; 0.02; 0.03; 0.04; 0.05; 0.06; 0.07; 0.08;] |> List.map string)



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




    // SortableTestFilters
    let _sortableTestFilter_Prefix24_4a = 
            SorterLibId.create (24<sortingWidth>) sorterVariant.Prefix4a
    let sortableTestFilter_Prefix24_4a = 
            (runParameters.sortableTestFilterKey, [_sortableTestFilter_Prefix24_4a] |> List.map SorterLibId.toString)

    let _sortableTestFilter_Prefix24_4b = 
            SorterLibId.create (24<sortingWidth>) sorterVariant.Prefix4b
    let sortableTestFilter_Prefix24_4b = 
            (runParameters.sortableTestFilterKey, [_sortableTestFilter_Prefix24_4b] |> List.map SorterLibId.toString)

    let _sortableTestFilter_Prefix24_3a = 
            SorterLibId.create (24<sortingWidth>) sorterVariant.Prefix3a
    let sortableTestFilter_Prefix24_3a = 
            (runParameters.sortableTestFilterKey, [_sortableTestFilter_Prefix24_3a] |> List.map SorterLibId.toString)

    let _sortableTestFilter_Prefix24_3b = 
            SorterLibId.create (24<sortingWidth>) sorterVariant.Prefix3b
    let sortableTestFilter_Prefix24_3b = 
            (runParameters.sortableTestFilterKey, [_sortableTestFilter_Prefix24_3b] |> List.map SorterLibId.toString)

    let sortableTestFilter_Prefix24_3s = 
            (runParameters.sortableTestFilterKey, 
            [_sortableTestFilter_Prefix24_3a; _sortableTestFilter_Prefix24_3b] |> List.map SorterLibId.toString)

    let sortableTestFilter_Prefix24s = 
            (runParameters.sortableTestFilterKey, 
            [   _sortableTestFilter_Prefix24_4a
                _sortableTestFilter_Prefix24_4b
                _sortableTestFilter_Prefix24_3a
                _sortableTestFilter_Prefix24_3b
            ] |> List.map SorterLibId.toString)

    let _sortableTestFilter_Prefix28_4 = 
            SorterLibId.create (28<sortingWidth>) sorterVariant.Prefix4a
    let sortableTestFilter_Prefix28_4 = 
            (runParameters.sortableTestFilterKey, [_sortableTestFilter_Prefix28_4] |> List.map SorterLibId.toString)


    let _sortableTestFilter_Prefix32_4 = 
            SorterLibId.create (32<sortingWidth>) sorterVariant.Prefix4a
    let sortableTestFilter_Prefix32_4 = 
            (runParameters.sortableTestFilterKey, [_sortableTestFilter_Prefix32_4] |> List.map SorterLibId.toString)






    // Sorted Fractions
    let sortedFractions = 
            (runParameters.sortedFractionKey, [0.65; 0.75; 0.85; 0.90; 0.95; 0.98; 0.99; 0.995] |> List.map string)

    let sortedFractionsHi = 
            (runParameters.sortedFractionKey, [0.99; 0.995] |> List.map string)

    let sortedFraction90 = 
            (runParameters.sortedFractionKey, [0.90] |> List.map string)

    let sortedFraction95 = 
            (runParameters.sortedFractionKey, [0.95] |> List.map string)

    let sortedFraction99 = 
            (runParameters.sortedFractionKey, [0.99] |> List.map string)




    // SorterEvalMeasures

    let _cestM_ScwP1 = ceStMeasure.create 
                                (1.1<stageWeight>) 
                                (true |> UMX.tag<filterUnsorted>)
                                (false |> UMX.tag<filterReflectionSymmetric>)
                                (0.0025 |> UMX.tag<stageCrossingWeight>)
                    |> sorterEvalMeasure.CeSt


    let _cestM_ScwP2 = ceStMeasure.create 
                                (1.1<stageWeight>) 
                                (true |> UMX.tag<filterUnsorted>)
                                (false |> UMX.tag<filterReflectionSymmetric>)
                                (0.005 |> UMX.tag<stageCrossingWeight>)
                    |> sorterEvalMeasure.CeSt


    let _cestM_ScwP3 = ceStMeasure.create 
                                (1.1<stageWeight>) 
                                (true |> UMX.tag<filterUnsorted>)
                                (false |> UMX.tag<filterReflectionSymmetric>)
                                (0.0075 |> UMX.tag<stageCrossingWeight>)
                    |> sorterEvalMeasure.CeSt


    let _cestM_ScwP4 = ceStMeasure.create 
                                (1.1<stageWeight>) 
                                (true |> UMX.tag<filterUnsorted>)
                                (false |> UMX.tag<filterReflectionSymmetric>)
                                (0.01 |> UMX.tag<stageCrossingWeight>)
                    |> sorterEvalMeasure.CeSt

    let _cestM_ScwP5 = ceStMeasure.create 
                                (1.1<stageWeight>) 
                                (true |> UMX.tag<filterUnsorted>)
                                (false |> UMX.tag<filterReflectionSymmetric>)
                                (0.015 |> UMX.tag<stageCrossingWeight>)
                    |> sorterEvalMeasure.CeSt



    let _cestM_ScwM1 = ceStMeasure.create 
                                (1.1<stageWeight>) 
                                (true |> UMX.tag<filterUnsorted>)
                                (false |> UMX.tag<filterReflectionSymmetric>)
                                (-0.005 |> UMX.tag<stageCrossingWeight>)
                    |> sorterEvalMeasure.CeSt

    let _cestM_ScwM2 = ceStMeasure.create 
                                (1.1<stageWeight>) 
                                (true |> UMX.tag<filterUnsorted>)
                                (false |> UMX.tag<filterReflectionSymmetric>)
                                (-0.01 |> UMX.tag<stageCrossingWeight>)
                    |> sorterEvalMeasure.CeSt



    let sorterEvalMeasureInitial_CestM_noScw =
            (runParameters.sorterEvalMeasureInitialKey, 
            [ SorterEvalMeasure.stageBiased;] |> List.map SorterEvalMeasure.toCompactString)
    let sorterEvalMeasureInitial_CestM_Scw =
            (runParameters.sorterEvalMeasureInitialKey, 
            [ _cestM_ScwP2;] |> List.map SorterEvalMeasure.toCompactString)



    let sorterEvalMeasure_CestM_noScw =
            (runParameters.sorterEvalMeasureKey, 
            [ SorterEvalMeasure.stageBiased;] |> List.map SorterEvalMeasure.toCompactString)
    let sorterEvalMeasure_CestM_Scw =
            (runParameters.sorterEvalMeasureKey, 
            [ _cestM_ScwP2;] |> List.map SorterEvalMeasure.toCompactString)


    let sorterEvalMeasure_CestM_Range =
            (runParameters.sorterEvalMeasureKey, 
            [ SorterEvalMeasure.stageBiased; _cestM_ScwP1; _cestM_ScwP2; _cestM_ScwP3; 
             _cestM_ScwP4; _cestM_ScwP5; _cestM_ScwM1; _cestM_ScwM2; ] 
            |> List.map SorterEvalMeasure.toCompactString)




    // SorterPoolMeasures

    let sorterPoolMeasure_zp4_noScw =
            (runParameters.sorterPoolMeasureKey, 
            [ SorterPoolMeasure.noStdev;] |> List.map SorterPoolMeasure.toCompactString)


    let sorterPoolMeasure_z_noScw =
            (runParameters.sorterPoolMeasureKey, 
            [ SorterPoolMeasure.stdev;] |> List.map SorterPoolMeasure.toCompactString)

    let sorterPoolMeasures =
            (runParameters.sorterPoolMeasureKey, 
            [ SorterPoolMeasure.noStdev; SorterPoolMeasure.stdev;] |> List.map SorterPoolMeasure.toCompactString)



    // MutationMods

    let mutationMod1 =
            (runParameters.mutationModKey, [1;] |> List.map string)

    let mutationMod2 =
            (runParameters.mutationModKey, [2;] |> List.map string)

    let mutationMod3 =
            (runParameters.mutationModKey, [3;] |> List.map string)

    let mutationMod4 =
            (runParameters.mutationModKey, [4;] |> List.map string)


    let mutationMods4a =
            (runParameters.mutationModKey, [1; 2; 3; 4] |> List.map string)

    let mutationMods4b =
            (runParameters.mutationModKey, [5; 6; 7; 8] |> List.map string)

    let mutationMods64 =
            (runParameters.mutationModKey, [0 .. 63] |> List.map string)

    let mutationMods128 =
            (runParameters.mutationModKey, [0 .. 127] |> List.map string)

    // SorterPoolExpansionRates

    let sorterPoolExpansionRate1 =
            (runParameters.sorterPoolExpansionRateKey, [1.0;] |> List.map string)

    let sorterPoolExpansionRate2 =
            (runParameters.sorterPoolExpansionRateKey, [2.0;] |> List.map string)

    let sorterPoolExpansionRate4 =
            (runParameters.sorterPoolExpansionRateKey, [4.0;] |> List.map string)
            
    let sorterPoolExpansionRate8 =
            (runParameters.sorterPoolExpansionRateKey, [8.0;] |> List.map string)

    let sorterPoolExpansionRates =
            (runParameters.sorterPoolExpansionRateKey, [1.0; 2.0; 4.0; 8.0;] |> List.map string)


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
        | 28 -> 300
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

