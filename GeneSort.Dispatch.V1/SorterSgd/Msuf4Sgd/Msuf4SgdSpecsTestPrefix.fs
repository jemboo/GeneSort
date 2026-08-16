namespace GeneSort.Dispatch.V1.SorterSgd.Msuf4

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Project.V1
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd


module Msuf4SgdSpecsTestPrefix =

    let sorterEvalSelectionTypeRs6000 = 
            (runParameters.seedPoolSorterEvalSelectionType, 
            [ sorterEvalSelectionType.RankSpan 6000<sorterCount>;] 
            |> List.map SorterEvalSelectionType.toString)
        
    let generationLast = 
            (runParameters.generationLastKey, [10000] |> List.map string)

    let generationCurrent = 
            (runParameters.generationCurrentKey, [0] |> List.map string)
            

    let prefixEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)
        
        let stf = rp.GetSortableTestFilter().Value
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithSortingWidth(Some stf.sortingWidth)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)


    let private paramMapFilter (rp: runParameters) =
        Some rp


    module Specs =

        let Rand_Test (executorType: sorterSgdExecutorTypeOld)  : runHostSpec = {
            databaseName = Msuf4SgdDbs.Prefix.dbName
            runName = sprintf @"Rand-test_%s" (SorterSgdExecutorTypeOld.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for 24pfx4 Msuf4"
            spans = [
                rngTypeLcg
                generationCurrent
                thirtyTwoSortersPerPool
                poolCount4
                oneChildCount
                sorterEvalSelectionTypeRs6000
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_noScw
                sortableTestFilter_Prefix24_4a
                msuf4ModelType
                sorterEvalTypeV1
                seedModificationRate03
                mRateOrtho
                mRatePara
                mRateSelfSym
                modificationRatesMsuf4center
                dataFomatBitv512
                distinctSorterHashesTrue
                prioritizeNewMutantsTrue
                sortedFraction99
                IntervalDefinitions.runResultReportInterval100
                generationLast
                sorterCountCycle500
                sorterCountCycleMultipliersLow
            ]
            filter = paramMapFilter
            enhancer = prefixEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


    type configType =
        | Rand_Test

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorTypeOld) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


