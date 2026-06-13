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


module MssiMutateSpecsRs = 


    let rngType = 
            (runParameters.rngTypeKey, 
            [CommonSorterMutate.projectRngType;] |> List.map RngType.toString)


    let sorterEvalType = 
            (runParameters.sorterEvalTypeKey, 
            [ sorterEvalType.V2 ;] |> List.map SorterEvalType.toString)


    let sorterEvalSelection = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.Tmb 6<sorterCount> ; ] |> List.map SorterEvalSelectionType.toString)

    let sorterEvalMeasure = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.0, true); ] |> List.map SorterEvalMeasure.toString)
    
    // SortingWidths
    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [4;5;6;7;8;9;10;11;12] |> List.map string)
    let mediumSortingWidths = 
            (runParameters.sortingWidthKey, [14;16;18;20;22] |> List.map string)


    // MutationRates
    let orthoRates =
            (runParameters.orthoRateKey, [1.0;] |> List.map string)
    let paraRates =
            (runParameters.paraRateKey, [1.0;] |> List.map string)
    let modificationRates =
            (runParameters.modificationRateKey, [0.0025; 0.005; 0.01; 0.015; 0.02; 0.03; 0.04; 0.05 ] |> List.map string)


    // SorterCounts
    let testChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let smallChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let mediumChildCount = (runParameters.sorterChildCountKey, ["100";] )
    let largeChildCount = (runParameters.sorterChildCountKey, ["10000";] )


    // SorterModelTypes
    let mssiModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Mssi] |> List.map SimpleSorterModelType.toString)


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
            DatabaseName = MssiMutateDbs.RandomStandard.Uniform.dbName
            RunName = sprintf @"Rand-Test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for Mssi"
            Spans = [
                rngType
                sorterEvalSelection
                sorterEvalMeasure
                sorterEvalType
                orthoRates
                paraRates
                modificationRates
                smallSortingWidths
                mssiModelType
                testChildCount
            ]
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }


        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = MssiMutateDbs.RandomStandard.Uniform.dbName
            RunName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for Mssi"
            Spans = [
                rngType
                sorterEvalSelection
                sorterEvalMeasure
                sorterEvalType
                orthoRates
                paraRates
                modificationRates
                smallSortingWidths
                mssiModelType
                largeChildCount
            ]
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_Medium (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = MssiMutateDbs.RandomStandard.Uniform.dbName
            RunName = sprintf @"Rand-Medium_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for Mssi"
            Spans = [
                rngType
                sorterEvalSelection
                sorterEvalMeasure
                sorterEvalType
                orthoRates
                paraRates
                modificationRates
                mediumSortingWidths
                mssiModelType
                largeChildCount
            ]
            Filter = standardSorterModelTypeFilter
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

    let getConfig (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


