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
            [ sorterEvalSelectionType.Tmb 48<sorterCount>; ] |> List.map SorterEvalSelectionType.toString)

    let sorterEvalMeasureInitial = 
            (runParameters.sorterEvalMeasureInitialKey , 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)

    let sorterEvalMeasureEvo = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)

    let generationLast = 
            (runParameters.generationLastKey, [10] |> List.map string)

    let generationFirst = 
            (runParameters.generationFirstKey, [0] |> List.map string)

    let generationQueryFirst = 
            (runParameters.queryWithGenFirst, [false] |> List.map string)

    let distinctSorterHashesBoth = 
            (runParameters.distinctSorterHashesKey, [true; false] |> List.map string)

    let distinctSorterHashesTrue = 
            (runParameters.distinctSorterHashesKey, [true] |> List.map string)


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
            DatabaseName = Msuf4SgdDbs.RandomStandard.Uniform.dbName
            RunName = sprintf @"Rand-Test1_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Sgd analysis for Msuf4"
            Spans = [
                msuf4ModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate
                modificationRateStage
                sortingWidth16
                poolCount30
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
                distinctSorterHashesBoth
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_Small (executorType: sorterSgdExecutorType) : runHostSpec = {
            DatabaseName = Msuf4SgdDbs.RandomStandard.Uniform.dbName
            RunName = sprintf @"Rand-Small_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Sgd analysis for Msuf4"
            Spans = [
                msuf4ModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate
                modificationRateStage
                sortingWidth16
                poolCount30
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
                distinctSorterHashesBoth
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_Pool2 (executorType: sorterSgdExecutorType) : runHostSpec = {
            DatabaseName = Msuf4SgdDbs.RandomStandard.Uniform.dbName
            RunName = sprintf @"Rand-Pool2_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Sgd analysis for Msuf4"
            Spans = [
                msuf4ModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate
                modificationRatesStage
                sortingWidth16
                poolCount30
                oneToFourSortersPerPool
                oneToFourChildCount
                generationFirst
                generationLast
                generationQueryFirst
                distinctSorterHashesBoth
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

    type configType =
        | Rand_Test
        | Rand_Small
        | Rand_Pool2

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_Pool2, Specs.Rand_Pool2);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType
