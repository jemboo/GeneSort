namespace GeneSort.Dispatch.V1.SorterMutate.Mssi

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


module MssiMutateSpecsRs = 

    let sorterEvalSelectionType = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterEvalSelectionType.toString)
    
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
            let gt4 = (%sw > 4)
            return! if (has2factor && gt4)  then Some rp else None
        }

    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = MssiMutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                orthoRate
                paraRate
                modificationRatesMsuf4
                testSortingWidths
                mssiModelType
                testChildCount
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MssiMutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                orthoRate
                paraRate
                modificationRatesMsuf4
                smallSortingWidths
                mssiModelType
                extraLargeChildCount
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_Medium (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = MssiMutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Medium_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Mssi"
            spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_noScw
                sorterEvalTypeV1
                orthoRate
                paraRate
                modificationRatesMsuf4
                mediumSortingWidths
                mssiModelType
                extraLargeChildCount
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

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_Medium, Specs.Rand_Medium);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


