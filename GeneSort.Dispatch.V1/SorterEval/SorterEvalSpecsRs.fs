namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams


module SorterEvalSpecsRs =

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

        let Rand_Test (executorType: sorterEvalExecutorType)  : runHostSpec = {
            databaseName = SorterEvalDbs.Standard.dbName
            runName = sprintf @"Rand-Test16_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalTypeV2
                sortingWidth16
                allSimpleSorterModelTypes
                testSorterCount
            ]
            filter = standardSorterModelTypeFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }

        let Rand_Small (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Standard.dbName
            runName = sprintf @"Rand-Small_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalTypeV2
                smallSortingWidths
                allSimpleSorterModelTypes
                extraLargeSorterCount
            ]
            filter = standardSorterModelTypeFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_Medium (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Standard.dbName
            runName = sprintf @"Rand-Medium_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "Standard sorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalTypeV2
                mediumSortingWidths
                allSimpleSorterModelTypes
                extraLargeSorterCount
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

    let getRunHostSpec (config: configType) (executorType: sorterEvalExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType
