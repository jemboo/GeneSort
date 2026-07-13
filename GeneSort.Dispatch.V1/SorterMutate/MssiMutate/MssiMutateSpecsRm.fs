namespace GeneSort.Dispatch.V1.SorterMutate.Mssi

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


module MssiMutateSpecsRm =

    let sorterEvalSelectionType = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterEvalSelectionType.toString)


    let sorterEvalMeasure = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.0, true);] |> List.map SorterEvalMeasure.toString)
    

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
            let has2factor = (%sw % 2 = 0)
            let! _ = if (%sw % %md = 0) then Some rp else None
            return! if has2factor then Some rp else None
        }

    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                modificationRatesMsuf4
                testMergeSortingWidths
                mssiModelType
                testMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                testChildCount
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
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                modificationRatesMsuf4
                smallMergeSortingWidths
                mssiModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                extraLargeChildCount
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
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                modificationRatesMsuf4
                mediumMergeSortingWidths
                mssiModelType
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
            databaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-MediumHd_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                modificationRatesMsuf4
                mediumMergeSortingWidths
                mssiModelType
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
            databaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            runName = sprintf @"Rand-Large2d_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                modificationRatesMsuf4
                largeMergeSortingWidths
                mssiModelType
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


