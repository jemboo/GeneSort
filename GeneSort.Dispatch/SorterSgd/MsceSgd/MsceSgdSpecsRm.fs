namespace GeneSort.Dispatch.V1.SorterSgd.Msce


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
open GeneSort.Dispatch.V1.SorterSgd

module MsceSgdSpecsRm =

    let sorterEvalSelectionType = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.Tmb 10<sorterCount>;] |> List.map SorterEvalSelectionType.toString)

    let sorterEvalMeasureInitial = 
            (runParameters.sorterEvalMeasureInitialKey , 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)

    let sorterEvalMeasureEvo = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.1, true); ] |> List.map SorterEvalMeasure.toString)
    
    let generationLast = 
            (runParameters.generationLastKey, [100] |> List.map string)

    let generationFirst = 
            (runParameters.generationFirstKey, [0] |> List.map string)

    let generationQueryFirst = 
            (runParameters.queryWithGenFirst, [false] |> List.map string)



    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)



    let private paramMapFilter (rp: runParameters) : runParameters option = 
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let! mst = rp.GetMergeSuffixType()
        
            let has2factor = (%sw % 2 = 0)
            let isMuf4able = (MathUtils.isAPowerOfTwo %sw)
            let isMuf6able = (%sw % 3 = 0) && (MathUtils.isAPowerOfTwo (%sw / 3))

            // We bind to unit just to enforce the filter
            let! _ = 
                match smt with
                | simpleSorterModelType.Msce -> Some ()
                | simpleSorterModelType.Mssi | simpleSorterModelType.Msrs -> 
                    if has2factor then Some () else None
                | simpleSorterModelType.Msuf4 -> 
                    if (isMuf4able && %sw < 256) then Some () else None
                | simpleSorterModelType.Msuf6 -> 
                    if isMuf6able then Some () else None
                | _ -> None

            // Merge dimension check: If it doesn't divide, return None to stop
            if (%sw % %md <> 0) then return! None
        
            // Suffix check: If it's NoSuffix and width > 128, return None to stop
            //if (mst.IsNoSuffix && %sw > 128 && %md > 2) then return! None
        
            return rp
        }


    module Specs =

        let Rand_Test (executorType: sorterSgdExecutorType)  : runHostSpec = {
            DatabaseName = MsceSgdDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-test_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Sgd analysis for merge Msce"
            Spans = [
                msceModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                sortingWidth16
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
                mergeDimension2
                noSuffixSuffixType
                dataFormatInt8v512
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_Small (executorType: sorterSgdExecutorType) : runHostSpec = {
            DatabaseName = MsceSgdDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Small_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Sgd analysis for merge Msce"
            Spans = [
                msceModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                sortingWidth16
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
                testMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_MediumLd (executorType: sorterSgdExecutorType) : runHostSpec = {
            DatabaseName = MsceSgdDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Medium_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Sgd analysis for merge Msce"
            Spans = [
                msceModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                sortingWidth16
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
                testMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        let Rand_MediumHd (executorType: sorterSgdExecutorType) : runHostSpec = {
            DatabaseName = MsceSgdDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Medium_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Sgd analysis for merge Msce"
            Spans = [
                msceModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                sortingWidth16
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
                testMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }


        let Rand_Large2d (executorType: sorterSgdExecutorType) : runHostSpec = {
            DatabaseName = MsceSgdDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Medium_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Sgd analysis for merge Msce"
            Spans = [
                msceModelType
                rngTypeLcg
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasureInitial
                sorterEvalMeasureEvo
                mutationRates
                insertionRates
                deletionRates
                modificationRatesMsce
                sortingWidth16
                testPoolCount
                oneSorterPerPool
                oneChildCount
                generationFirst
                generationLast
                generationQueryFirst
                testMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

    type configType =
        | Rand_Test
        | Rand_Small
        | Rand_MediumLd
        | Rand_MediumHd
        | Rand_Large2d

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_MediumLd, Specs.Rand_MediumLd);
                        (configType.Rand_MediumHd, Specs.Rand_MediumHd);
                        (configType.Rand_Large2d, Specs.Rand_Large2d);
                    ]


    let getRunHostSpec (config: configType) (executorType: sorterSgdExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


