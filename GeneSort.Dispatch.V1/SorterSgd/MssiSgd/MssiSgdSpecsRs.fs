namespace GeneSort.Dispatch.V1.SorterSgd.Mssi

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd


module MssiSgdSpecsRs = 

    let prioritizeNewMutantsBoth = 
            (runParameters.prioritizeNewMutantsKey, 
            [ true; false ] |> List.map string)

    let sorterEvalSelection = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.Tmb 1500<sorterCount> ; ] |> List.map SorterEvalSelectionType.toString)

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
        
    let generationLast = 
            (runParameters.generationLastKey, [2500] |> List.map string)

    let generationFirst = 
            (runParameters.generationFirstKey, [0] |> List.map string)

    let generationQueryFirst = 
            (runParameters.queryWithGenFirst, [false] |> List.map string)

    let distinctSorterHashesBoth = 
            (runParameters.distinctSorterHashesKey, [true; false] |> List.map string)

    let distinctSorterHashesTrue = 
            (runParameters.distinctSorterHashesKey, [true] |> List.map string)

    let distinctSorterHashesFalse = 
            (runParameters.distinctSorterHashesKey, [false] |> List.map string)


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
            databaseName = MssiSgdDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Test_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Mssi"
            spans = [
                mssiModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                modificationRatesMsuf4
                sortingWidth16
                poolCount10
                oneSorterPerPool
                oneChildCount
                generationFirst
                genReportInterval10
                generationLast
                generationQueryFirst
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


        let Rand_Pool (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = MssiSgdDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Test_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Mssi"
            spans = [
                mssiModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                modificationRatesMsuf4
                sortingWidth16
                poolCount1
                fourKSortersPerPool
                oneChildCount
                generationFirst
                genReportInterval10
                generationLast
                generationQueryFirst
                distinctSorterHashesTrue
                prioritizeNewMutantsBoth
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }


        let Rand_Small (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = MssiSgdDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Small_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Mssi"
            spans = [
                mssiModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                modificationRatesMsuf4
                sortingWidth16
                poolCount10
                oneSorterPerPool
                oneChildCount
                generationFirst
                genReportInterval10
                generationLast
                generationQueryFirst
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_Medium (executorType: sorterSgdExecutorType) : runHostSpec = {
            databaseName = MssiSgdDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Medium_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Mssi"
            spans = [
                mssiModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelection
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                orthoRate
                paraRate
                modificationRatesMsuf4
                sortingWidth16
                poolCount10
                oneSorterPerPool
                oneChildCount
                generationFirst
                genReportInterval10
                generationLast
                generationQueryFirst
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

    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


