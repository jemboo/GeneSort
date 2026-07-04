namespace GeneSort.Dispatch.V1.SorterMutate.Msce

open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams


module MsceMutateSpecsRs = 

    let sorterEvalSelectionType = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterEvalSelectionType.toString)

    let sorterEvalMeasure = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.0, true); ] |> List.map SorterEvalMeasure.toString)
    

    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)

    
    let private standardSorterModelTypeFilter (rp: runParameters) =
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let has2factor = (%sw % 2 = 0)
            let isPowerOf2 = (%sw &&& (%sw - 1) = 0)
            let isGt4 = (%sw > 4)
            let validMsce = (smt = simpleSorterModelType.Msce)
            let validMssi = (smt = simpleSorterModelType.Mssi) && has2factor
            let validMsrs = (smt = simpleSorterModelType.Msrs) && has2factor
            let validMsuf4 = (smt = simpleSorterModelType.Msuf4) && isPowerOf2 && isGt4
            return! if validMsce || validMssi || validMsrs || validMsuf4 then Some rp else None
        }


    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = MsceMutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msce"
            spans = [
                msceModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasure
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                testSortingWidths
                testChildCount
            ]
            filter = standardSorterModelTypeFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MsceMutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msce"
            spans = [
                msceModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasure
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                smallSortingWidths
                extraLargeChildCount
            ]
            filter = standardSorterModelTypeFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_Medium (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MsceMutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Medium_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msce"
            spans = [
                msceModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasure
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                mediumSortingWidths
                extraLargeChildCount
            ]
            filter = standardSorterModelTypeFilter
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

    let getRunHostSpec (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


