namespace GeneSort.Dispatch.V1.SorterMutate.Mssi

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Model.Sorting.V1


module MssiMutateSpecsRm =

    let sorterEvalSelectionType = 
            (runParameters.seedSorterPoolSelectionTypeKey, 
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterEvalSelectionType.toString)


    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        let mrgLibId = rp.GetMergeLibId().Value
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithSortingWidth(Some mrgLibId.SortingWidth)
          .WithCollectNewSortableTests(Some (false |> UMX.tag))
          .WithExcludeSelfCe(Some (true |> UMX.tag))
          .WithId (Some qp.Value.Id)

    

    let private paramMapFilter (rp: runParameters) : runParameters option = 
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! mrgLibId = rp.GetMergeLibId()
        
            let has2factor = (%mrgLibId.SortingWidth % 2 = 0)
            let isMuf4able = (MathUtils.isAPowerOfTwo %mrgLibId.SortingWidth)
            let isMuf6able = (%mrgLibId.SortingWidth % 3 = 0) && (MathUtils.isAPowerOfTwo (%mrgLibId.SortingWidth / 3))

            // We bind to unit just to enforce the filter
            let! _ = 
                match smt with
                | simpleSorterModelType.Msce -> Some ()
                | simpleSorterModelType.Mssi | simpleSorterModelType.Msrs -> 
                    if has2factor then Some () else None
                | simpleSorterModelType.Msuf4 -> 
                    if isMuf4able then Some () else None
                | simpleSorterModelType.Msuf6 -> 
                    if isMuf6able then Some () else None

            // Merge dimension check: If it doesn't divide, return None to stop
            if (%mrgLibId.SortingWidth % %mrgLibId.MergeDimension <> 0) then return! None
        
            return rp
        }

    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                mRateOrtho
                mRatePara
                modificationRatesMsuf4
                mergeLib_Merge32s
                mssiModelType
                dataFormatInt8v512
                testChildCount
                (runParameters.mutationModKey, [0;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }

        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                mRateOrtho
                mRatePara
                modificationRatesMsuf4
                smallMergeSortingWidths
                mssiModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                extraLargeChildCount
                (runParameters.mutationModKey, [0;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_MediumLd (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-MediumLd_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                mRateOrtho
                mRatePara
                modificationRatesMsuf4
                mediumMergeSortingWidths
                mssiModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                extraLargeChildCount
                (runParameters.mutationModKey, [0;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        let Rand_MediumHd (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-MediumHd_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                mRateOrtho
                mRatePara
                modificationRatesMsuf4
                mediumMergeSortingWidths
                mssiModelType
                mergeDimension6
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
                (runParameters.mutationModKey, [0;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }

        let Rand_Large2d (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-Large2d_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                mRateOrtho
                mRatePara
                modificationRatesMsuf4
                largeMergeSortingWidths
                mssiModelType
                mergeDimension2
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
                (runParameters.mutationModKey, [0;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


    type configType =
        | Rand_Test
        | Rand_Small
        | Rand_MediumLd
        | Rand_MediumHd
        | Rand_Large2d

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_MediumLd, Specs.Rand_MediumLd);
                        (configType.Rand_MediumHd, Specs.Rand_MediumHd);
                        (configType.Rand_Large2d, Specs.Rand_Large2d);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


