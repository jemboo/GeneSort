namespace GeneSort.Dispatch.V1.SorterSgd.Msrs

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Project.V1
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd


module MsrsSgdSpecsTestPrefix =

    let sorterEvalSelectionTypeGuid12K = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.GuidOrder 12000<sorterCount>;] 
            |> List.map SorterEvalSelectionType.toString)

    let sorterEvalSelectionTypeGuid1K = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.GuidOrder 1000<sorterCount>;] 
            |> List.map SorterEvalSelectionType.toString)
        
    let generationLastTest = 
            (runParameters.generationLastKey, [11] |> List.map string)

    let generationLastLight = 
            (runParameters.generationLastKey, [1001] |> List.map string)

    let generationLast = 
            (runParameters.generationLastKey, [5000] |> List.map string)

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

        let Test (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = MsrsSgdDbs.Prefix.dbName
            runName = sprintf @"Rand-testA_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for 24pfx Msrs"
            spans = [
                rngTypeLcg
                generationCurrent
                sixteenSortersPerPool
                poolCount16
                oneChildCount
                sorterEvalSelectionTypeGuid1K
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_noScw
                sortableTestFilter_Prefix24_3a
                msrsModelType
                sorterEvalTypeV1
                seedModificationRate02
                modificationRatep04
                orthoRate
                paraRate
                selfSymRate
                dataFomatBitv512
                distinctSorterHashesTrue
                prioritizeNewMutantsTrue
                sortedFraction99
                runResultReportInterval10
                summaryReport_cSampleC
                sorterPoolSelect5_2
                generationLastTest
                sorterCountCycle100
                sorterCountCycleMultiplier1
                mutationMod4
                sorterPoolExpansionRate1
            ]
            filter = paramMapFilter
            enhancer = prefixEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let Light (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = MsrsSgdDbs.Prefix.dbName
            runName = sprintf @"Rand-testA_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for 24pfx Msrs"
            spans = [
                rngTypeLcg
                generationCurrent
                sixteenSortersPerPool
                poolCount16
                oneChildCount
                sorterEvalSelectionTypeGuid1K
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_noScw
                sortableTestFilter_Prefix24_3a
                msrsModelType
                sorterEvalTypeV1
                seedModificationRate02
                modificationRatep04
                orthoRate
                paraRate
                selfSymRate
                dataFomatBitv512
                distinctSorterHashesTrue
                prioritizeNewMutantsTrue
                sortedFraction99
                runResultReportInterval500
                summaryReport_cSampleC
                sorterPoolSelects25_5
                generationLastLight
                sorterCountCycle100
                sorterCountCycleMultiplier1
                mutationMods64
                sorterPoolExpansionRates
            ]
            filter = paramMapFilter
            enhancer = prefixEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }

        let T4_P3 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = MsrsSgdDbs.Prefix.dbName
            runName = sprintf @"T4_P3_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for 24pfx Msrs"
            spans = [
                rngTypeLcg
                generationCurrent
                fiveTwelveSortersPerPool
                poolCount16
                oneChildCount
                sorterEvalSelectionTypeGuid12K
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_noScw
                sortableTestFilter_Prefix24_3a
                msrsModelType
                sorterEvalTypeV1
                seedModificationRate02
                modificationRatep06
                orthoRate
                paraRate
                selfSymRate
                dataFomatBitv512
                distinctSorterHashesTrue
                prioritizeNewMutantsTrue
                sortedFraction99
                runResultReportInterval1000
                runResultReportInterval100
                generationLast
                sorterCountCycle500
                sorterCountCycleMultipliersLow
                mutationMod2
                sorterPoolExpansionRates
            ]
            filter = paramMapFilter
            enhancer = prefixEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }


    type configType =
        | Light
        | T4_P3
        | Test

    let Configs = Map.ofList 
                    [ 
                        (configType.Test, Specs.Test);
                        (configType.Light, Specs.Light);
                        (configType.T4_P3, Specs.T4_P3);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


