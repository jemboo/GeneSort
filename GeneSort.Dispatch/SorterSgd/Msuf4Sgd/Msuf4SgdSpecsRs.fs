namespace GeneSort.Dispatch.V1.SorterSgd.Msuf4

open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams


module Msuf4SgdSpecsRs = 


    let sorterEvalSelection = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.Tmb 6<sorterCount> ; ] |> List.map SorterEvalSelectionType.toString)

    let sorterEvalMeasure = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.0, true); ] |> List.map SorterEvalMeasure.toString)
    
        
    let generationCount = 
            (runParameters.generationLastKey, [10] |> List.map string)

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
            DatabaseName = Msuf4SgdDbs.RandomStandard.Uniform.dbName
            RunName = sprintf @"Rand-Test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for Msuf4"
            Spans = [
                rngTypeLcg
                sorterEvalSelection
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRates
                paraRates
                selfSymRates
                noSeedModificationRates
                modificationRatesStage
                testSortingWidths
                msuf4ModelType
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }


        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = Msuf4SgdDbs.RandomStandard.Uniform.dbName
            RunName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for Msuf4"
            Spans = [
                rngTypeLcg
                sorterEvalSelection
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRates
                paraRates
                selfSymRates
                noSeedModificationRates
                modificationRatesStage
                smallSortingWidths
                msuf4ModelType
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_Medium (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = Msuf4SgdDbs.RandomStandard.Uniform.dbName
            RunName = sprintf @"Rand-Medium_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for Msuf4"
            Spans = [
                rngTypeLcg
                sorterEvalSelection
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRates
                paraRates
                selfSymRates
                noSeedModificationRates
                modificationRatesStage
                mediumSortingWidths
                msuf4ModelType
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
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


