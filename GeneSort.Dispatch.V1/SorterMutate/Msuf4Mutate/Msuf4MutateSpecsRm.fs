namespace GeneSort.Dispatch.V1.SorterMutate.Msuf4

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams


module Msuf4MutateSpecsRm =

    let sorterEvalSelectionType = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.ValueSpan 5<sorterCount>;] 
            |> List.map SorterEvalSelectionType.toString)


    let sorterEvalMeasure = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.0, true);] 
            |> List.map SorterEvalMeasure.toString)
   

    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)


    let private paramMapFilter (rp: runParameters) =
        maybe {
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let isMuf4able = (MathUtils.isAPowerOfTwo %sw)

            let! _ = if (%sw % %md = 0) then Some rp else None
            return! if isMuf4able then Some rp else None
        }


    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = Msuf4MutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRatesMsuf4
                sortingWidth32
                msuf4ModelType
                mergeDimension8
                noSuffixSuffixType
                dataFormatInt8v512
                extraLargeChildCount
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }

        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = Msuf4MutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRatesMsuf4
                smallMergeSortingWidths
                msuf4ModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_MediumLd (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = Msuf4MutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-MediumLd_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRatesMsuf4
                mediumMergeSortingWidths
                msuf4ModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                extraLargeChildCount
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        let Rand_MediumHd (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = Msuf4MutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-MediumHd_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRatesMsuf4
                mediumMergeSortingWidths
                msuf4ModelType
                mergeDimension6
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


        let Rand_Large2d (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = Msuf4MutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-Large2d_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRatesMsuf4
                largeMergeSortingWidths
                msuf4ModelType
                mergeDimension2
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
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


