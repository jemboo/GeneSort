namespace GeneSort.Dispatch.V1.SorterMutate.Msce


open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams

module MsceMutateSpecsRm =

    let seedSorterPoolSelectionType = 
            (runParameters.seedSorterPoolSelectionTypeKey, 
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)

    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
          .WithCollectNewSortableTests(Some (false |> UMX.tag<collectNewSortableTests>))
          .WithSortableDataFormat(Some sortableDataFormat.Int8Vector512)
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

            // Merge dimension check: If it doesn't divide, return None to stop
            if (%sw % %md <> 0) then return! None
        
            // Suffix check: If it's NoSuffix and width > 128, return None to stop
            //if (mst.IsNoSuffix && %sw > 128 && %md > 2) then return! None
        
            return rp
        }


    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = MsceMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msce"
            spans = [
                rngTypeLcg
                msceModelType
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                testMergeSortingWidths
                testMergeDimensions
                noSuffixSuffixType
                sorterEvalTypeV1
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                testChildCount
                mutationMod1
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }

        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MsceMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msce"
            spans = [
                rngTypeLcg
                msceModelType
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                smallMergeSortingWidths
                allMergeDimensions
                noSuffixSuffixType
                extraLargeChildCount
                mutationMod1
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }

        let Rand_MediumLd (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MsceMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-MediumLd_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msce"
            spans = [
                rngTypeLcg
                msceModelType
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                mediumMergeSortingWidths
                msceModelType
                lowMergeDimensions
                noSuffixSuffixType
                extraLargeChildCount
                mutationMod1
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        let Rand_MediumHd (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MsceMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-MediumHd_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msce"
            spans = [
                rngTypeLcg
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                mediumMergeSortingWidths
                msceModelType
                mergeDimension6
                noSuffixSuffixType
                largeChildCount
                mutationMod1
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


        let Rand_Large2d (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MsceMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-Large2d_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msce"
            spans = [
                rngTypeLcg
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                largeMergeSortingWidths
                msceModelType
                mergeDimension2
                noSuffixSuffixType
                largeChildCount
                mutationMod1
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


