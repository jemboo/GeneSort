namespace GeneSort.Dispatch.V1.SorterSgd.Msuf4

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd


module Msuf4SgdSpecsRm =

    let sorterEvalSelection = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.RankSpan 5000<sorterCount>;] 
            |> List.map SorterEvalSelectionType.toString)
        
    let generationLast = 
            (runParameters.generationLastKey, [500] |> List.map string)

    let generationCurrent = 
            (runParameters.generationCurrentKey, [0] |> List.map string)
            

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
            databaseName = Msuf4SgdDbs.Merge.dbName
            runName = sprintf @"Rand-testA_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                msuf4ModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_noScw
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate03
                modificationRate03
                sortingWidth32                
                mergeDimension8
                noSuffixSuffixType
                dataFormatInt8v512
                poolCount1
                oneTwenty8SortersPerPool
                oneChildCount
                generationCurrent
                genReportInterval1
                generationLast
                sorterCountCycle20
                sorterCountCycleMultiplier1n2
                distinctSorterHashesTrue
                prioritizeNewMutantsTrue
                sortedFraction90
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }

        let Rand_Small (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = Msuf4SgdDbs.Merge.dbName
            runName = sprintf @"Rand-Small_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelection
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_noScw
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
                poolCount10
                oneSorterPerPool
                oneChildCount
                generationCurrent
                genReportInterval10
                generationLast
                sortedFraction90
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_Medium (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = Msuf4SgdDbs.Merge.dbName
            runName = sprintf @"Rand-Medium_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelection
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_noScw
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
                poolCount10
                oneSorterPerPool
                oneChildCount
                generationCurrent
                genReportInterval10
                generationLast
                sortedFraction90
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
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


