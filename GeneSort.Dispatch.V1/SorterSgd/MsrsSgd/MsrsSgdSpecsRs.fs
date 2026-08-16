namespace GeneSort.Dispatch.V1.SorterSgd.Msrs

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd

module MsrsSgdSpecsRs = 

    let sorterEvalSelection = 
            (runParameters.seedPoolSorterEvalSelectionType, 
            [ sorterEvalSelectionType.Tmb 1500<sorterCount> ; ] |> List.map SorterEvalSelectionType.toString)

    let generationLast = 
            (runParameters.generationLastKey, [2500] |> List.map string)

    let generationCurrent = 
            (runParameters.generationCurrentKey, [20] |> List.map string)


    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)

    
    let private paramMapFilter (rp: runParameters) =
        maybe {
            let! sw = rp.GetSortingWidth()
            let has2factor = (%sw % 2 = 0)
            return! if has2factor then Some rp else None
        }

    module Specs =

        let Rand_Test (executorType: sorterSgdExecutorTypeOld)  : runHostSpec = {
            databaseName = MsrsSgdDbs.Standard.Uniform.dbName
            runName = sprintf @"Rand-Test_%s" (SorterSgdExecutorTypeOld.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msrs"
            spans = [
                msrsModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_noScw
                mRateOrtho
                mRatePara
                mRateSelfSym
                modificationRatesMsuf4
                sortingWidth16
                poolCount8
                oneSorterPerPool
                oneChildCount
                IntervalDefinitions.runResultReportInterval100
                generationLast
                generationCurrent                
                sorterCountCycle20
                sorterCountCycleMultipliers1n2
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        let Rand_Pool (executorType: sorterSgdExecutorTypeOld)  : runHostSpec = {
            databaseName = MsrsSgdDbs.Standard.Uniform.dbName
            runName = sprintf @"Rand-Pool_%s" (SorterSgdExecutorTypeOld.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msrs"
            spans = [
                msrsModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_noScw
                mRateOrtho
                mRatePara
                mRateSelfSym
                modificationRatesStage
                sortingWidth16
                poolCount1
                fourKSortersPerPool
                oneChildCount
                generationCurrent
                IntervalDefinitions.runResultReportInterval100
                generationLast
                distinctSorterHashesTrue
                prioritizeNewMutantsBoth
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }


        let Rand_Small (executorType: sorterSgdExecutorTypeOld) : runHostSpec = {
            databaseName = MsrsSgdDbs.Standard.Uniform.dbName
            runName = sprintf @"Rand-Small_%s" (SorterSgdExecutorTypeOld.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msrs"
            spans = [
                msrsModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasureInitial_CestM_noScw
                mRateOrtho
                mRatePara
                mRateSelfSym
                modificationRatesMsuf4
                sortingWidth16
                poolCount8
                oneSorterPerPool
                oneChildCount
                generationCurrent
                IntervalDefinitions.runResultReportInterval100
                generationLast
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        let Rand_Medium (executorType: sorterSgdExecutorTypeOld) : runHostSpec = {
            databaseName = MsrsSgdDbs.Standard.Uniform.dbName
            runName = sprintf @"Rand-Medium_%s" (SorterSgdExecutorTypeOld.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msrs"
            spans = [
                msrsModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasureInitial_CestM_noScw
                mRateOrtho
                mRatePara
                mRateSelfSym
                modificationRatesMsuf4
                sortingWidth16
                poolCount8
                oneSorterPerPool
                oneChildCount
                generationCurrent
                IntervalDefinitions.runResultReportInterval100
                generationLast
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
        | Rand_Pool

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_Medium, Specs.Rand_Medium);
                        (configType.Rand_Pool, Specs.Rand_Pool);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorTypeOld) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType
