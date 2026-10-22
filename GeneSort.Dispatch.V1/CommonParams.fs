namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters

module CommonParams =

    // SimpleSorterModelTypes
    let withMsuf4 = 
            (ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.withMsuf4() 
            |> List.map SimpleSorterModelType.toString)

    let withMsuf6 = 
            (ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.withMsuf6() 
            |> List.map SimpleSorterModelType.toString)

    let msceModelType = 
            (ParamKeys.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)

    let msrsModelType = 
            (ParamKeys.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msrs] |> List.map SimpleSorterModelType.toString)

    let mssiModelType = 
            (ParamKeys.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Mssi] |> List.map SimpleSorterModelType.toString)

    let msuf4ModelType = 
            (ParamKeys.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msuf4] |> List.map SimpleSorterModelType.toString)


    // SorterCounts
    let testSorterCount = (ParamKeys.sorterCountKey, ["1000";] )
    let smallSorterCount = (ParamKeys.sorterCountKey, ["100";] )
    let mediumSorterCount = (ParamKeys.sorterCountKey, ["1000";] )
    let largeSorterCount = (ParamKeys.sorterCountKey, ["10000";] )
    let veryLargeSorterCount = (ParamKeys.sorterCountKey, ["25000";] )
    let extraLargeSorterCount = (ParamKeys.sorterCountKey, ["100000";] )

    
    // SorterCounts per pool
    let oneAndTwoSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["1"; "2";] )
    let oneToFourSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["1"; "2"; "4"] )
    let fourTo32SortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["4"; "8"; "16"; "32"] )
    let mid3SortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["64"; "128"; "256"] )

    
    let oneSorterPerPool = (ParamKeys.sorterCountPerPoolKey, ["1";] )
    let twoSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["2";] )
    let fourSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["4"] )
    let eightSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["18";] )
    let sixteenSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["16";] )
    let thirtyTwoSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["32";] )
    let sixtyFourSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["64";] )
    let oneTwenty8SortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["128";] )
    let twoFifty6SortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["256";] )
    let fiveTwelveSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["512";] )
    let oneKSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["1024";] )
    let twoKSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["2048";] )
    let fourKSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["4096";] )
    let eightKSortersPerPool = (ParamKeys.sorterCountPerPoolKey, ["8192";] )

    // sorterCountCycle
    let sorterCountCycle20 = (ParamKeys.sorterCountCycleKey, ["20";] )
    let sorterCountCycle50 = (ParamKeys.sorterCountCycleKey, ["50";] )
    let sorterCountCycle100 = (ParamKeys.sorterCountCycleKey, ["100";] )
    let sorterCountCycle500 = (ParamKeys.sorterCountCycleKey, ["500";] )

    // sorterCountCycleMultiplier
    let sorterCountCycleMultiplier1 = (ParamKeys.sorterCountCycleMultiplierKey, ["1";] )
    let sorterCountCycleMultipliers1n2 = (ParamKeys.sorterCountCycleMultiplierKey, ["1"; "2";] )
    let sorterCountCycleMultipliers1n4 = (ParamKeys.sorterCountCycleMultiplierKey, ["1"; "4";] )
    let sorterCountCycleMultipliersLow = (ParamKeys.sorterCountCycleMultiplierKey, ["1.001"; "1.2"; "1.4"; "1.8"] )
    let sorterCountCycleMultiplier4 = (ParamKeys.sorterCountCycleMultiplierKey, ["4";] )
    let sorterCountCycleMultiplier8 = (ParamKeys.sorterCountCycleMultiplierKey, ["8";] )
    let sorterCountCycleMultiplier16 = (ParamKeys.sorterCountCycleMultiplierKey, ["16";] )


    // SorterPoolCounts
    let poolCount1 = (ParamKeys.sorterPoolCountKey, ["1";] )
    let poolCount2 = (ParamKeys.sorterPoolCountKey, ["2";] )
    let poolCount4 = (ParamKeys.sorterPoolCountKey, ["4";] )
    let poolCount8 = (ParamKeys.sorterPoolCountKey, ["8";] )
    let poolCount16 = (ParamKeys.sorterPoolCountKey, ["16";] )
    let poolCount32 = (ParamKeys.sorterPoolCountKey, ["32";] )
    let poolCount64 = (ParamKeys.sorterPoolCountKey, ["64";] )
    let poolCount128 = (ParamKeys.sorterPoolCountKey, ["128";] )
    let poolCount256 = (ParamKeys.sorterPoolCountKey, ["256";] )
    let poolCount512 = (ParamKeys.sorterPoolCountKey, ["512";] )

    // SorterChildCounts
    let oneChildCount = (ParamKeys.sorterChildCountKey, ["1";] )
    let twoChildCount = (ParamKeys.sorterChildCountKey, ["2";] )
    let oneAndTwoChildCount = (ParamKeys.sorterChildCountKey, ["1"; "2";])
    let oneToFourChildCount = (ParamKeys.sorterChildCountKey, ["1"; "2"; "4";])
    let testChildCount = (ParamKeys.sorterChildCountKey, ["1000";] )
    let smallChildCount = (ParamKeys.sorterChildCountKey, ["10";] )
    let mediumChildCount = (ParamKeys.sorterChildCountKey, ["100";] )
    let largeChildCount = (ParamKeys.sorterChildCountKey, ["1000";] )
    let extraLargeChildCount = (ParamKeys.sorterChildCountKey, ["10000";] )

    let _sorterEvalTypeV1 = sorterEvalType.V1
    let _sorterEvalTypeV2 = sorterEvalType.V2

    let sorterEvalTypeV1 = 
            (ParamKeys.sorterEvalTypeKey, 
            [ sorterEvalType.V1 ;] |> List.map SorterEvalType.toString)

    let sorterEvalTypeV2 = 
            (ParamKeys.sorterEvalTypeKey, 
            [ sorterEvalType.V2 ;] |> List.map SorterEvalType.toString)


    let _rngTypeLcg = rngType.Lcg

    // MergeDimensions
    let testMergeDimensions = 
        (ParamKeys.mergeDimensionKey, [8;] |> List.map string)
    let allMergeDimensions = 
            (ParamKeys.mergeDimensionKey, [2; 3; 4; 6; 8] |> List.map string)
    let mergeDimension2 = 
            (ParamKeys.mergeDimensionKey, [2;] |> List.map string)
    let mergeDimension3 = 
            (ParamKeys.mergeDimensionKey, [3;] |> List.map string)
    let mergeDimension4 = 
            (ParamKeys.mergeDimensionKey, [4;] |> List.map string)
    let mergeDimension6 = 
            (ParamKeys.mergeDimensionKey, [6;] |> List.map string)
    let mergeDimension8 = 
            (ParamKeys.mergeDimensionKey, [8;] |> List.map string)
    let lowMergeDimensions = 
            (ParamKeys.mergeDimensionKey, [2; 3; 4;] |> List.map string)
    let highMergeDimensions = 
            (ParamKeys.mergeDimensionKey, [6; 8] |> List.map string)
    

    // SortingWidths
    let testSortingWidths = 
            (ParamKeys.sortingWidthKey, [4;5;6;7;8;9;10;11;12] |> List.map string)
    let sortingWidth16 = 
            (ParamKeys.sortingWidthKey, [16] |> List.map string)
    let sortingWidth24 = 
            (ParamKeys.sortingWidthKey, [24] |> List.map string)
    let sortingWidth28 = 
            (ParamKeys.sortingWidthKey, [28] |> List.map string)
    let sortingWidth32 = 
            (ParamKeys.sortingWidthKey, [32] |> List.map string)
    let smallSortingWidths = 
            (ParamKeys.sortingWidthKey, [4;5;6;7;8;9;10;11;12] |> List.map string)
    let mediumSortingWidths = 
            (ParamKeys.sortingWidthKey, [14;16;18;20;22] |> List.map string)


    // MergeSortingWidths
    let testMergeSortingWidths = 
            (ParamKeys.sortingWidthKey, [32;] |> List.map string)

    let smallMergeSortingWidths = 
            (ParamKeys.sortingWidthKey, [16; 18; 24; 32; 36; 48; 64] |> List.map string)  
            
    let mediumMergeSortingWidths = 
            (ParamKeys.sortingWidthKey,  [96; 128;]  |> List.map string)

    let sortingWidth96 = 
            (ParamKeys.sortingWidthKey,  [96;]  |> List.map string)

    let largeMergeSortingWidths = 
            (ParamKeys.sortingWidthKey,  [192; 256; 512]  |> List.map string)

    let smallP2MergeSortingWidths = 
            (ParamKeys.sortingWidthKey, [16; 32; 64;] |> List.map string)

    let mediumP2MergeSortingWidths = 
            (ParamKeys.sortingWidthKey, [128;] |> List.map string)

    let largeP2MergeSortingWidths = 
            (ParamKeys.sortingWidthKey, [256; 512;] |> List.map string)


    // RngType
    let rngTypeLcg = 
            (ParamKeys.rngTypeKey, [_rngTypeLcg] |> List.map RngType.toString)


    // DataFormats
    let dataFormatInt8v512 = 
            (ParamKeys.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            
    let dataFomatBitv512 = 
            (ParamKeys.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)

    let noSuffixSuffixType = 
            (ParamKeys.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)



    // MutatorParams

    let mutatorSpansMsce (srtWdth: int<sortingWidth>) (mutVariant:mutatorVariant) (rType: rngType) = 
            (ParamKeys.mutatorParamsKey, 
                [   
                    MutatorParams.msceParamsR5 srtWdth rType mutVariant
                    //MutatorParams.msceParamsR10 srtWdth rType
                ] 
            |> List.map MutatorParams.toString)

    let mutatorSpansMssi (srtingWdth: int<sortingWidth>) (mutVariant:mutatorVariant) (rType: rngType) = 
            (ParamKeys.mutatorParamsKey, 
                [   
                    MutatorParams.mssiParamsRL srtingWdth rType mutVariant
                    //MutatorParams.mssiParamsRM srtingWdth rType
                    //MutatorParams.mssiParamsRH srtingWdth rType
                    //MutatorParams.mssiParamsRVH srtingWdth rType
                ] 
            |> List.map MutatorParams.toString)


    let mutatorSpansMsrs (srtingWdth: int<sortingWidth>) (mutVariant:mutatorVariant) (rType: rngType) = 
            (ParamKeys.mutatorParamsKey, 
                [   
                    MutatorParams.msrsParamsRL srtingWdth rType mutVariant
                    //MutatorParams.msrsParamsRM srtingWdth rType
                    //MutatorParams.msrsParamsRH srtingWdth rType
                ] 
            |> List.map MutatorParams.toString)


    let mutatorSpansMsuf4 (srtingWdth: int<sortingWidth>) (mutVariant:mutatorVariant) (rType: rngType) = 
            (ParamKeys.mutatorParamsKey, 
                [   
                    MutatorParams.msuf4ParamsRL srtingWdth rType mutVariant
                    //MutatorParams.msuf4ParamsRC srtingWdth rType
                    //MutatorParams.msuf4ParamsRH srtingWdth rType
                ] 
            |> List.map MutatorParams.toString)


    // MutationRates

    let mutationRates =
            (ParamKeys.mutationRateKey, [1.0] |> List.map string)
    let insertionRates =
            (ParamKeys.insertionRateKey, [0.1;] |> List.map string)
    let deletionRates =
            (ParamKeys.deletionRateKey, [0.1;] |> List.map string)

    let mRateOrtho =    (ParamKeys.orthoRateKey, [4.001;] |> List.map string)
    let mRatePara =     (ParamKeys.paraRateKey, [0.4;] |> List.map string)
    let mRatesPara =     (ParamKeys.paraRateKey, [0.4; 0.6] |> List.map string)
    let mRateSelfSym =  (ParamKeys.selfSymRateKey, [2.001; ] |> List.map string)
    let mRatesSelfSym =  (ParamKeys.selfSymRateKey, [1.5; 2.001; ] |> List.map string)



    // SeedModificationRates

    let seedModificationRate02 =
            (ParamKeys.seedModificationRateKey, [0.02] |> List.map string)
    let seedModificationRate03 =
            (ParamKeys.seedModificationRateKey, [0.03] |> List.map string)
    let seedModificationRate06 =
            (ParamKeys.seedModificationRateKey, [0.06] |> List.map string)
    let seedModificationRate12 =
            (ParamKeys.seedModificationRateKey, [0.12] |> List.map string)
    
    let seedModificationRateR2 =
            (ParamKeys.seedModificationRateKey, [0.03; 0.06] |> List.map string)

    let seedModificationRate10 =
            (ParamKeys.seedModificationRateKey, [0.10] |> List.map string)

    let seedModificationRates =
            (ParamKeys.seedModificationRateKey, [0.02; 0.03;] |> List.map string)

    let seedModificationRates2 =
            (ParamKeys.seedModificationRateKey, [0.10; 0.14] |> List.map string)



    // ModificationRates

    let modificationRatesStageR =
            (ParamKeys.modificationRateKey, [ 0.08; 0.09; 0.10; 0.11; 0.12; 0.13; 0.14; 0.15;] |> List.map string)

    let modificationRatesStageRR =
            (ParamKeys.modificationRateKey, [ 0.02; 0.03; 0.04; 0.05; 0.07; 0.08; 0.09; 0.10;] |> List.map string)
            
    let modificationRate03 =
            (ParamKeys.modificationRateKey, [ 0.03; ] |> List.map string)

    let modificationRate90 =
            (ParamKeys.modificationRateKey, [ 0.90; ] |> List.map string)


    let modificationRatesMsce =
        (ParamKeys.modificationRateKey, [ 0.02; 0.035; 0.045; 0.05; 0.055; 0.065; 0.08 ] |> List.map string)

    let modificationRatesMsuf4 =
            (ParamKeys.modificationRateKey, [0.015; 0.02; 0.025; 0.03] |> List.map string)

    let modificationRatesMsuf4center =
            (ParamKeys.modificationRateKey, [0.015; 0.0175; 0.02; 0.0225; 0.025; 0.0275; 0.03; 0.0325;] |> List.map string)

    let modificationRatesStage =
            (ParamKeys.modificationRateKey, [ 0.04; 0.08; 0.16; 0.24;] |> List.map string)

    let modificationRates15 =
            (ParamKeys.modificationRateKey, [ 0.15;] |> List.map string)

    let modificationRatesStage2 =
            (ParamKeys.modificationRateKey, [ 0.17; 0.21; 0.25; 0.29;] |> List.map string)
            
    let modificationRatep01 =
            (ParamKeys.modificationRateKey, [0.01;] |> List.map string)

    let modificationRatep04 =
            (ParamKeys.modificationRateKey, [0.04;] |> List.map string)

    let modificationRatep06 =
            (ParamKeys.modificationRateKey, [0.06;] |> List.map string)

    let modificationRatesMsceCenter =
            (ParamKeys.modificationRateKey, [0.01; 0.02; 0.03; 0.04; 0.05; 0.06; 0.07; 0.08;] |> List.map string)



    // prioritizeNewMutants

    let prioritizeNewMutantsBoth = 
            (ParamKeys.prioritizeNewMutantsKey, 
            [ true; false ] |> List.map string)


    let prioritizeNewMutantsTrue = 
            (ParamKeys.prioritizeNewMutantsKey, 
            [ true; ] |> List.map string)

    let prioritizeNewMutantsFalse = 
            (ParamKeys.prioritizeNewMutantsKey, 
            [ false ] |> List.map string)



    // distinctSorterHashes
    let distinctSorterHashesBoth = 
            (ParamKeys.distinctSorterHashesKey, [true; false] |> List.map string)

    let distinctSorterHashesTrue = 
            (ParamKeys.distinctSorterHashesKey, [true] |> List.map string)

    let distinctSorterHashesFalse = 
            (ParamKeys.distinctSorterHashesKey, [false] |> List.map string)




    // PrefixLib
    let _prefixLib_Prefix24_4a = 
            prefixLibId.create (24<sortingWidth>) (4<stageLength>) prefixLibVariant.PrefixA
    let prefixLib_Prefix24_4a = 
            (ParamKeys.prefixLibIdKey, [_prefixLib_Prefix24_4a] |> List.map PrefixLibId.toString)

    let _prefixLib_Prefix24_4b = 
            prefixLibId.create (24<sortingWidth>) (4<stageLength>)  prefixLibVariant.PrefixB
    let prefixLib_Prefix24_4b = 
            (ParamKeys.prefixLibIdKey, [_prefixLib_Prefix24_4b] |> List.map PrefixLibId.toString)

    let _prefixLib_Prefix24_3a = 
            prefixLibId.create (24<sortingWidth>) (3<stageLength>) prefixLibVariant.PrefixA
    let prefixLib_Prefix24_3a = 
            (ParamKeys.prefixLibIdKey, [_prefixLib_Prefix24_3a] |> List.map PrefixLibId.toString)

    let _prefixLib_Prefix24_3b = 
            prefixLibId.create (24<sortingWidth>) (3<stageLength>) prefixLibVariant.PrefixB
    let prefixLib_Prefix24_3b = 
            (ParamKeys.prefixLibIdKey, [_prefixLib_Prefix24_3b] |> List.map PrefixLibId.toString)

    let prefixLib_Prefix24_3s = 
            (ParamKeys.prefixLibIdKey, 
            [_prefixLib_Prefix24_3a; 
             _prefixLib_Prefix24_3b
            ] |> List.map PrefixLibId.toString)

    let prefixLib_Prefix24s = 
            (ParamKeys.prefixLibIdKey, 
            [   _prefixLib_Prefix24_4a
                _prefixLib_Prefix24_4b
                _prefixLib_Prefix24_3a
                _prefixLib_Prefix24_3b
            ] |> List.map PrefixLibId.toString)

    let _prefixLib_Prefix28_4a = 
            prefixLibId.create (28<sortingWidth>) (4<stageLength>) prefixLibVariant.PrefixA
    let prefixLib_Prefix28_4 = 
            (ParamKeys.prefixLibIdKey, [_prefixLib_Prefix28_4a] |> List.map PrefixLibId.toString)


    let _prefixLib_Prefix32_4a = 
            prefixLibId.create (32<sortingWidth>) (4<stageLength>) prefixLibVariant.PrefixA
    let prefixLib_Prefix32_4 = 
            (ParamKeys.prefixLibIdKey, [_prefixLib_Prefix32_4a] |> List.map PrefixLibId.toString)



    // MergeLib


    let _mergeLib_Merge16_2a = 
            mergeLibId.create (16<sortingWidth>) (2<mergeDimension>) sorterLibVariant.VariantA

    let _mergeLib_Merge32_2a = 
            mergeLibId.create (32<sortingWidth>) (2<mergeDimension>) sorterLibVariant.VariantA

    let _mergeLib_Merge32_2b = 
            mergeLibId.create (32<sortingWidth>) (2<mergeDimension>) sorterLibVariant.VariantB

    let _mergeLib_Merge32_4a = 
            mergeLibId.create (32<sortingWidth>) (4<mergeDimension>) sorterLibVariant.VariantA

    let _mergeLib_Merge32_8a = 
            mergeLibId.create (32<sortingWidth>) (8<mergeDimension>) sorterLibVariant.VariantA

    let _mergeLib_Merge64_4a = 
            mergeLibId.create (64<sortingWidth>) (4<mergeDimension>) sorterLibVariant.VariantA

    let _mergeLib_Merge64_4b = 
            mergeLibId.create (64<sortingWidth>) (4<mergeDimension>) sorterLibVariant.VariantB

    let _mergeLib_Merge64_8a = 
            mergeLibId.create (64<sortingWidth>) (8<mergeDimension>) sorterLibVariant.VariantA


    let mergeLib_Merge16_2a = 
            (ParamKeys.mergeLibIdKey, 
            [   _mergeLib_Merge16_2a
            ] |> List.map MergeLibId.toString)


    let mergeLib_Merge32_2a = 
            (ParamKeys.mergeLibIdKey, 
            [   _mergeLib_Merge32_2a
            ] |> List.map MergeLibId.toString)

    let mergeLib_Merge32s = 
            (ParamKeys.mergeLibIdKey, 
            [   _mergeLib_Merge32_2a
                _mergeLib_Merge32_2b
                _mergeLib_Merge32_4a
                _mergeLib_Merge32_8a
            ] |> List.map MergeLibId.toString)

    let mergeLib_Merge64_4a =
            (ParamKeys.mergeLibIdKey, 
            [   _mergeLib_Merge64_4a
            ] |> List.map MergeLibId.toString)

    // Sorted Fractions
    let sortedFractions = 
            (ParamKeys.sortedFractionKey, [0.65; 0.75; 0.85; 0.90; 0.95; 0.98; 0.99; 0.995] |> List.map string)

    let sortedFractionsHi = 
            (ParamKeys.sortedFractionKey, [0.99; 0.995] |> List.map string)

    let sortedFraction90 = 
            (ParamKeys.sortedFractionKey, [0.90] |> List.map string)

    let sortedFraction95 = 
            (ParamKeys.sortedFractionKey, [0.95] |> List.map string)

    let sortedFraction99 = 
            (ParamKeys.sortedFractionKey, [0.99] |> List.map string)




    // SorterEvalMeasures

    let _cestM_ScwP3 =
        ceStMeasure.create {
            StageWeight = 1.1<stageWeight>
            FilterUnsorted = true |> UMX.tag<filterUnsorted>
            FilterReflectionSymmetric = false |> UMX.tag<filterReflectionSymmetric>
            StageCrossingWeight = 0.5 |> UMX.tag<stageCrossingWeight>
        }
        |> sorterEvalMeasure.CeSt

    let _cestM_ScwP2 =
        ceStMeasure.create {
            StageWeight = 1.1<stageWeight>
            FilterUnsorted = true |> UMX.tag<filterUnsorted>
            FilterReflectionSymmetric = false |> UMX.tag<filterReflectionSymmetric>
            StageCrossingWeight = 0.3 |> UMX.tag<stageCrossingWeight>
        }
        |> sorterEvalMeasure.CeSt


    let _cestM_ScwP1 =
        ceStMeasure.create {
            StageWeight = 1.1<stageWeight>
            FilterUnsorted = true |> UMX.tag<filterUnsorted>
            FilterReflectionSymmetric = false |> UMX.tag<filterReflectionSymmetric>
            StageCrossingWeight = 0.1 |> UMX.tag<stageCrossingWeight>
        }
        |> sorterEvalMeasure.CeSt


    let _cestM_ScwN1 =
        ceStMeasure.create {
            StageWeight = 1.1<stageWeight>
            FilterUnsorted = true |> UMX.tag<filterUnsorted>
            FilterReflectionSymmetric = false |> UMX.tag<filterReflectionSymmetric>
            StageCrossingWeight = -0.05 |> UMX.tag<stageCrossingWeight>
        }
        |> sorterEvalMeasure.CeSt





    let sorterEvalMeasureInitial_CestM_noScw =
            (ParamKeys.sorterEvalMeasureInitialKey, 
            [ SorterEvalMeasure.stageBiasedFilterUnsorted;] |> List.map SorterEvalMeasure.toCompactString)
    let sorterEvalMeasureInitial_CestM_Scw =
            (ParamKeys.sorterEvalMeasureInitialKey, 
            [ _cestM_ScwP2;] |> List.map SorterEvalMeasure.toCompactString)



    let sorterEvalMeasure_CestM_noScw =
            (ParamKeys.sorterEvalMeasureKey, 
            [ SorterEvalMeasure.stageBiasedFilterUnsorted;] |> List.map SorterEvalMeasure.toCompactString)
    let sorterEvalMeasure_CestM_Scw =
            (ParamKeys.sorterEvalMeasureKey, 
            [ _cestM_ScwP2;] |> List.map SorterEvalMeasure.toCompactString)


    let sorterEvalMeasure_StageCrossing_Range =
            (ParamKeys.sorterEvalMeasureKey, 
            [ SorterEvalMeasure.stageBiasedFilterUnsorted; _cestM_ScwP2; _cestM_ScwP1; _cestM_ScwN1; _cestM_ScwP3; ] 
            |> List.map SorterEvalMeasure.toCompactString)



    // SorterPoolMeasures

    let sorterPoolMeasure_zp4_noScw =
            (ParamKeys.sorterPoolMeasureKey, 
            [ SorterPoolMeasure.noStdev;] |> List.map SorterPoolMeasure.toCompactString)


    let sorterPoolMeasure_z_noScw =
            (ParamKeys.sorterPoolMeasureKey, 
            [ SorterPoolMeasure.stdev;] |> List.map SorterPoolMeasure.toCompactString)

    let sorterPoolMeasures =
            (ParamKeys.sorterPoolMeasureKey, 
            [ SorterPoolMeasure.noStdev; SorterPoolMeasure.stdev;] |> List.map SorterPoolMeasure.toCompactString)



    // MutationMods

    let mutationMod1 =
            (ParamKeys.mutationModKey, [1;] |> List.map string)

    let mutationMod2 =
            (ParamKeys.mutationModKey, [2;] |> List.map string)

    let mutationMod3 =
            (ParamKeys.mutationModKey, [3;] |> List.map string)

    let mutationMod4 =
            (ParamKeys.mutationModKey, [4;] |> List.map string)


    let mutationMods4a =
            (ParamKeys.mutationModKey, [1; 2; 3; 4] |> List.map string)

    let mutationMods4b =
            (ParamKeys.mutationModKey, [5; 6; 7; 8] |> List.map string)

    let mutationMods64 =
            (ParamKeys.mutationModKey, [0 .. 63] |> List.map string)

    let mutationMods128 =
            (ParamKeys.mutationModKey, [0 .. 127] |> List.map string)

    // SorterPoolExpansionRates

    let sorterPoolExpansionRate1 =
            (ParamKeys.sorterPoolExpansionRateKey, [1.0;] |> List.map string)

    let sorterPoolExpansionRate2 =
            (ParamKeys.sorterPoolExpansionRateKey, [2.0;] |> List.map string)

    let sorterPoolExpansionRate4 =
            (ParamKeys.sorterPoolExpansionRateKey, [4.0;] |> List.map string)

    let sorterPoolExpansionRate8 =
            (ParamKeys.sorterPoolExpansionRateKey, [8.0;] |> List.map string)

    let sorterPoolExpansionRates =
            (ParamKeys.sorterPoolExpansionRateKey, [1.0; 2.0; 4.0; 8.0;] |> List.map string)


    let getStageLength = SeedSorterModelGeneration.getStageLength

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
        | 16 -> match smt with | simpleSorterModelType.Msuf4 -> 100 | _ -> 60
        | 18 -> 80
        | 20 -> 100
        | 22 -> 125
        | 24 -> 150
        | 32 -> match smt with | simpleSorterModelType.Msuf4 -> 200 | _ -> 150
        | 36 -> 150
        | 48 -> 200
        | 64 -> match smt with | simpleSorterModelType.Msuf4 -> 1000 | _ -> 300
        | 96 -> 800
        | 128 -> match smt with | simpleSorterModelType.Msuf4 -> 1500 | _ -> 600
        | 192 -> 2000
        | 256 -> match smt with | simpleSorterModelType.Msuf4 -> 2000 | _ -> 1000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag

