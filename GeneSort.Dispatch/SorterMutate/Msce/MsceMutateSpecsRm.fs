namespace GeneSort.Dispatch.V1.SorterMutate.Msce


open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate


module MsceMutateSpecsRm =

    let rngType = 
            (runParameters.rngTypeKey, 
            [CommonSorterMutate.projectRngType;] |> List.map RngType.toString)


    let sorterEvalType = 
            (runParameters.sorterEvalTypeKey, 
            [ sorterEvalType.V1; ] |> List.map SorterEvalType.toString)

    //let sorterEvalSelectionType = 
    //        (runParameters.sorterEvalSelectionType, 
    //        [ sorterEvalSelectionType.TopN 10<sorterCount>;  
    //          sorterEvalSelectionType.Tmb 6<sorterCount>;  
    //          sorterEvalSelectionType.ValueSpan 10<sorterCount>; ] |> List.map SorterEvalSelectionType.toString)

    let sorterEvalSelectionType = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterEvalSelectionType.toString)


    let sorterEvalMeasure = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.0, true);] |> List.map SorterEvalMeasure.toString)
    
    // SortingWidths
    let smallSortingWidths = SortableTestMergeSpecs.smallSortingWidths
    let mediumSortingWidths = SortableTestMergeSpecs.mediumSortingWidths

    // MergeDimensions
    let allMergeDimensions = SortableTestMergeSpecs.allMergeDimensions
    let lowMergeDimensions = SortableTestMergeSpecs.lowMergeDimensions
    let highMergeDimensions = SortableTestMergeSpecs.highMergeDimensions

    // DataFormats
    let onlyDataFormat = 
            (runParameters.sortableDataFormatKey, 
            [CommonSorterEval.mergeSortableDataFormat] |> List.map SortableDataFormat.toString)
    
    // MergeSuffixTypes
    let bothMergeSuffixTypes = SortableTestMergeSpecs.bothMergeSuffixTypes
    let vv1SuffixType = SortableTestMergeSpecs.vv1SuffixType



    // MutationRates
    let mutationRates =
            (runParameters.mutationRateKey, [1.0] |> List.map string)
    let insertionRates =
            (runParameters.insertionRateKey, [0.1;] |> List.map string)
    let deletionRates =
            (runParameters.deletionRateKey, [0.1;] |> List.map string)
    let modificationRates =
            (runParameters.modificationRateKey, [0.0025; 0.005; 0.01; 0.015; 0.02; 0.03; 0.04; 0.05 ] |> List.map string)


    // SorterCounts
    let testChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let smallChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let mediumChildCount = (runParameters.sorterChildCountKey, ["100";] )
    let largeChildCount = (runParameters.sorterChildCountKey, ["100000";] )

    // SimpleSorterModelTypes
    let msceModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)
    let allSimpleSorterModelTypes = SorterEvalSpecsRandom.allSimpleSorterModelTypes



    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)



    let private paramMapFilter (rp: runParameters) : runParameters option = 
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let! mst = rp.GetMergeSuffixType()
        
            let has2factor = (%sw % 2 = 0)
            let isMuf4able = (MathUtils.isAPowerOfTwo %sw)
            let isMuf6able = (%sw % 3 = 0) && (MathUtils.isAPowerOfTwo (%sw / 3))

            // We bind to unit just to enforce the filter
            let! _ = 
                match smt with
                | simpleSorterModelType.Msce -> Some ()
                | simpleSorterModelType.Mssi | simpleSorterModelType.Msrs -> 
                    if has2factor then Some () else None
                | simpleSorterModelType.Msuf4 -> 
                    if (isMuf4able && %sw < 256) then Some () else None
                | simpleSorterModelType.Msuf6 -> 
                    if isMuf6able then Some () else None
                | _ -> None

            // Merge dimension check: If it doesn't divide, return None to stop
            if (%sw % %md <> 0) then return! None
        
            // Suffix check: If it's NoSuffix and width > 128, return None to stop
            if (mst.IsNoSuffix && %sw > 128) then return! None
        
            return rp
        }


    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            DatabaseName = MsceMutateDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalType
                mutationRates
                insertionRates
                deletionRates
                modificationRates
                SortableTestMergeSpecs.testSortingWidths
                allSimpleSorterModelTypes
                SortableTestMergeSpecs.testMergeDimensions
                SortableTestMergeSpecs.noSuffixSuffixType
                onlyDataFormat
                largeChildCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = MsceMutateDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalType
                mutationRates
                insertionRates
                deletionRates
                modificationRates
                smallSortingWidths
                msceModelType
                lowMergeDimensions
                bothMergeSuffixTypes
                onlyDataFormat
                largeChildCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_Medium (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = MsceMutateDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Medium_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalType
                mutationRates
                insertionRates
                deletionRates
                modificationRates
                mediumSortingWidths
                msceModelType
                lowMergeDimensions
                bothMergeSuffixTypes
                onlyDataFormat
                largeChildCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

    type configType =
        | Rand_Test
        | Rand_Small
        | Rand_Medium

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_Medium, Specs.Rand_Medium);
                    ]

    let getConfig (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


