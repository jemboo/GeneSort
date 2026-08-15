namespace GeneSort.Dispatch.V1.SorterSgd.Msrs

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Project.V1
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd
open GeneSort.Core
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.SortingLib.Sorter


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
            (runParameters.generationLastKey, [2001] |> List.map string)

    let generationLast = 
            (runParameters.generationLastKey, [5000] |> List.map string)

    let generationCurrent = 
            (runParameters.generationCurrentKey, [0] |> List.map string)
            

    let prefixEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let stf = SorterLibId.create (24<sortingWidth>) sorterVariant.Prefix3a

        let spp = rp.GetSorterCountPerPool().Value |> UMX.untag
        let pc = rp.GetSorterPoolCount().Value |> UMX.untag
        let sorterEvalSelectionType = sorterEvalSelectionType.GuidOrder ((spp * pc) |> UMX.tag<sorterCount>)

        let newRp = rp.WithRngType(Some rngType.Lcg)
                      .WithCollectNewSortableTests(false |> UMX.tag<collectNewSortableTests> |> Some)
                      .WithExcludeSelfCe(true |> UMX.tag<excludeSelfCe> |> Some)
                      .WithSortableTestFilter(Some stf)
                      .WithSortingWidth(Some stf.sortingWidth)
                      .WithSorterChildCount(Some 1<sorterChildCount>)
                      .WithSimpleSorterModelType(Some simpleSorterModelType.Msrs)
                      .WithSorterEvalType(Some sorterEvalType.V1)
                      .WithSeedModificationRate(Some 0.02<seedModificationRate>)
                      .WithModificationRate(Some 0.06<modificationRate>)
                      .WithOrthoRate(Some 4.001<orthoRate>)
                      .WithParaRate(Some 0.4<paraRate>)
                      .WithSelfSymRate(Some 2.001<selfSymRate>)
                      .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
                      .WithDistinctSorterHashes(Some true)
                      .WithPrioritizeNewMutants(Some true)
                      .WithSortedFraction(Some 0.99<sortedFraction>)
                      .WithSorterEvalMeasureInitial(Some SorterEvalMeasure.stageBiased)
                      .WithSorterEvalMeasure(Some SorterEvalMeasure.stageBiased)
                      .WithSorterEvalSelectionType(Some sorterEvalSelectionType)
                      .WithSorterPoolExpansionRate(Some 2<sorterPoolExpansionRate>)

        let qp = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.Run host.Run.RunName)

        newRp.WithDatabaseName(Some host.Run.DatabaseName)
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
                generationCurrent
                sixteenSortersPerPool
                poolCount16
                IntervalDefinitions.runResultReportInterval2
                IntervalDefinitions.summaryReport_cSampleC
                IntervalDefinitions.sorterPoolSelects25_5i
                generationLastTest
                sorterCountCycle100
                sorterCountCycleMultiplier1
                mutationMod4
                sorterPoolMeasures
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
                generationCurrent
                thirtyTwoSortersPerPool
                poolCount32
                IntervalDefinitions.runResultReportInterval500
                IntervalDefinitions.summaryReport_cSample5C
                IntervalDefinitions.sorterPoolSelects25_5i
                generationLastLight
                sorterCountCycle100
                sorterCountCycleMultiplier1
                mutationMods128
                sorterPoolMeasures
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
                mRateOrtho
                mRatePara
                mRateSelfSym
                dataFomatBitv512
                distinctSorterHashesTrue
                prioritizeNewMutantsTrue
                sortedFraction99
                IntervalDefinitions.runResultReportInterval1000
                IntervalDefinitions.runResultReportInterval100
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


