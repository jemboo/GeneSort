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


module Msuf4SgdSpecsTestPrefix =

    let sorterEvalSelectionTypeRs6000 = 
            (runParameters.sorterEvalSelectionType, 
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

        let Rand_Test (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = Msuf4SgdDbs.Prefix.dbName
            runName = sprintf @"Rand-test_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for 32pfx4 Msuf4"
            spans = [
                rngTypeLcg
                generationCurrent
                sixteenSortersPerPool
                poolCount40
                oneChildCount
                sorterEvalSelectionTypeRs6000
                sorterEvalMeasureInitial_CestM_noScw
                sorterEvalMeasure_CestM_Range
                sortableTestFilter_Prefix32_4
                msuf4ModelType
                sorterEvalTypeV1
                seedModificationRate03
                orthoRate
                paraRate
                selfSymRate
                modificationRatesMsuf4center
                dataFomatBitv512
                distinctSorterHashesTrue
                prioritizeNewMutantsTrue
                sortedFraction99
                genReportInterval200
                generationLast
            ]
            filter = paramMapFilter
            enhancer = prefixEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 16
        }


    type configType =
        | Rand_Test

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


