namespace GeneSort.Dispatch.V1.SorterSgd.Msuf4

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd


module Msuf4SgdSpecsRm =

    let sorterEvalSelectionType = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.ValueSpan 30<sorterCount>;] 
            |> List.map SorterEvalSelectionType.toString)


    let sorterEvalMeasureInitial = 
            (runParameters.sorterEvalMeasureInitialKey , 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)

    let sorterEvalMeasureEvo = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)
        
    let generationLast = 
            (runParameters.generationLastKey, [100] |> List.map string)

    let generationFirst = 
            (runParameters.generationFirstKey, [0] |> List.map string)

    let generationQueryFirst = 
            (runParameters.queryWithGenFirst, [false] |> List.map string)

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

        let Rand_Test (executorType: sorterSgdExecutorType)  : runHostSpec = {
            DatabaseName = Msuf4SgdDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-test_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for merge Msuf4"
            Spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                sorterEvalTypeV1
                orthoRates
                paraRates
                selfSymRates
                seedModificationRateZero
                modificationRatesStage
                testMergeSortingWidths
                msuf4ModelType
                testMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 1
        }

        let Rand_Small (executorType: sorterSgdExecutorType) : runHostSpec = {
            DatabaseName = Msuf4SgdDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Small_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for merge Msuf4"
            Spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                sorterEvalTypeV1
                orthoRates
                paraRates
                selfSymRates
                seedModificationRateZero
                modificationRatesStage
                smallMergeSortingWidths
                msuf4ModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_Medium (executorType: sorterSgdExecutorType) : runHostSpec = {
            DatabaseName = Msuf4SgdDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Medium_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for merge Msuf4"
            Spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                sorterEvalTypeV1
                orthoRates
                paraRates
                selfSymRates
                seedModificationRateZero
                modificationRatesStage
                mediumMergeSortingWidths
                msuf4ModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
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

    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


