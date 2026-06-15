namespace GeneSort.Dispatch.V1.SorterMutate.Mssi

open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams


module MssiMutateSpecsRm =

    let sorterEvalSelectionType = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterEvalSelectionType.toString)


    let sorterEvalMeasure = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.0, true);] |> List.map SorterEvalMeasure.toString)
    

    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)

    

    let private paramMapFilter (rp: runParameters) =
        maybe {
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let has2factor = (%sw % 2 = 0)
            let! _ = if (%sw % %md = 0) then Some rp else None
            return! if has2factor then Some rp else None
        }

    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            DatabaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for merge Mssi"
            Spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRates
                paraRates
                modificationRates
                testMergeSortingWidths
                mssiModelType
                testMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                testChildCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 1
        }

        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for merge Mssi"
            Spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRates
                paraRates
                modificationRates
                smallMergeSortingWidths
                mssiModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_Medium (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = MssiMutateDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Medium_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for merge Mssi"
            Spans = [
                rngTypeLcg
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalTypeV1
                orthoRates
                paraRates
                modificationRates
                mediumMergeSortingWidths
                mssiModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
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


