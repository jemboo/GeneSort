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

    let sorterEvalSelection = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.RankSpan 5000<sorterCount>;] 
            |> List.map SorterEvalSelectionType.toString)


    let sorterEvalMeasureInitial = 
            (runParameters.sorterEvalMeasureInitialKey , 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)

    let sorterEvalMeasureEvo = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)
        
    let generationLast = 
            (runParameters.generationLastKey, [10] |> List.map string)

    let generationCurrent = 
            (runParameters.generationCurrentKey, [0] |> List.map string)
            

    let prefixEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)

        rp.WithDatabaseName(Some host.Run.DatabaseName)
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
                sortingWidth32
                msuf4ModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate03
                modificationRate03
                dataFormatInt8v512
                poolCount1
                oneTwenty8SortersPerPool
                oneChildCount
                generationCurrent
                genReportInterval1
                generationLast
                distinctSorterHashesTrue
                prioritizeNewMutantsTrue
                sortedFraction90
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

    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


