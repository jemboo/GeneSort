namespace GeneSort.Dispatch.V1.SorterMutate.Msuf4

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams


module Msuf4MutateSpecsRs = 


    let sorterEvalSelection = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.Tmb 6<sorterCount> ; ] |> List.map SorterEvalSelectionType.toString)

    let sorterEvalMeasure = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.0, true); ] |> List.map SorterEvalMeasure.toString)
    

    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)

    
    let private paramMapFilter (rp: runParameters) =
        maybe {
            let! sw = rp.GetSortingWidth()
            let! isGt4 = if (%sw > 4) then Some () else None
            let isMuf4able = (MathUtils.isAPowerOfTwo %sw)
            return! if isMuf4able then Some rp else None
        }


    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = Msuf4MutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelection
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRateStage
                testSortingWidths
                msuf4ModelType
                testChildCount
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = Msuf4MutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelection
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRateStage
                smallSortingWidths
                msuf4ModelType
                extraLargeChildCount
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let Rand_Medium (executorType: sorterMutateExecutorType) : runHostSpec = {
            databaseName = Msuf4MutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Medium_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msuf4"
            spans = [
                rngTypeLcg
                sorterEvalSelection
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRate
                paraRate
                selfSymRate
                seedModificationRate12
                modificationRateStage
                mediumSortingWidths
                msuf4ModelType
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


