namespace GeneSort.Dispatch.V1.SorterSgd.Msuf4

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd


module Msuf4SgdSpecsRs = 

    let sorterEvalSelection = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.Tmb 3000<sorterCount>; ] |> List.map SorterEvalSelectionType.toString)

    let sorterEvalMeasureInitial = 
            (runParameters.sorterEvalMeasureInitialKey , 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)

    let sorterEvalMeasureEvo = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)

    let sorterEvalMeasureEvos = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (0.8, true);
              sorterEvalMeasure.CeSt (2.0, true); ] |> List.map SorterEvalMeasure.toString)

    let sorterEvalMeasureEvoUsc = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeStUc (1.1, 20.0); ] |> List.map SorterEvalMeasure.toString)

    let generationLast = 
            (runParameters.generationLastKey, [500] |> List.map string)

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

        let Rand_Test (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = Msuf4SgdDbs.Standard.dbName
            runName = sprintf @"Rand-Test1_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Sgd analysis for Msuf4"
            spans = [
                msuf4ModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRatesMsuf4
                sortingWidth16
                poolCount30
                oneSorterPerPool
                oneChildCount
                generationLast
                generationCurrent
                distinctSorterHashesBoth
                prioritizeNewMutantsBoth
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }

        let Rand_Small (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = Msuf4SgdDbs.Standard.dbName
            runName = sprintf @"Rand-Small_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Sgd analysis for Msuf4"
            spans = [
                msuf4ModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRatesMsuf4
                sortingWidth16
                poolCount30
                oneSorterPerPool
                oneChildCount
                generationCurrent
                generationLast
                distinctSorterHashesBoth
                prioritizeNewMutantsBoth
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_Pool (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = Msuf4SgdDbs.Standard.dbName
            runName = sprintf @"Rand-Pool_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Sgd analysis for Msuf4"
            spans = [
                msuf4ModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate10
                modificationRate03
                sortingWidth16
                poolCount1
                fourKSortersPerPool
                oneChildCount
                generationCurrent
                generationLast
                distinctSorterHashesBoth
                prioritizeNewMutantsBoth
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }


        let Rand_Pool3 (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = Msuf4SgdDbs.Standard.dbName
            runName = sprintf @"Rand-Pool3_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Sgd analysis for Msuf4"
            spans = [
                msuf4ModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvoUsc
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRates15
                sortingWidth16
                poolCount10
                oneToFourSortersPerPool
                twoChildCount
                generationCurrent
                generationLast
                distinctSorterHashesTrue
                prioritizeNewMutantsBoth
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

    type configType =
        | Rand_Test
        | Rand_Small
        | Rand_Pool
        | Rand_Pool3

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_Pool, Specs.Rand_Pool);
                        (configType.Rand_Pool3, Specs.Rand_Pool3);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType
