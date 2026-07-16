namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams

module SorterEvalSpecsRm =

    let private mergeEnhancer 
                    (host: IRunHost) 
                    (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)
                 |> Option.get
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Id)


    let private paramMapFilter (rp: runParameters) : runParameters option = 
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
        
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
                    if isMuf4able then Some () else None
                | simpleSorterModelType.Msuf6 -> 
                    if isMuf6able then Some () else None
                | _ -> None

            // Merge dimension check: If it doesn't divide, return None to stop
            if (%sw % %md <> 0) then return! None
        
            return rp
        }


    module Specs =

        let Rand_MergeTest_Test (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-Test_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [   
                rngTypeLcg
                dataFormatInt8v512
                msuf4ModelType
                noSuffixSuffixType
                sorterEvalTypeV2
                sortingWidth32
                mergeDimension8
                extraLargeSorterCount
            ]
            filter = paramMapFilter
            enhancer = mergeEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let Rand_MergeTest_Small (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-Small_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                dataFormatInt8v512
                allSimpleSorterModelTypes
                noSuffixSuffixType
                sorterEvalTypeV2
                smallMergeSortingWidths
                allMergeDimensions
                extraLargeSorterCount
            ]
            filter = paramMapFilter
            enhancer = mergeEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }


        let Rand_MergeTest_MediumLd (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-MediumLd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                dataFormatInt8v512
                allSimpleSorterModelTypes
                noSuffixSuffixType
                sorterEvalTypeV2
                mediumMergeSortingWidths
                lowMergeDimensions
                largeSorterCount
            ]
            filter = paramMapFilter
            enhancer = mergeEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


        let Rand_MergeTest_MediumHd (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-MediumHd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                dataFormatInt8v512
                noSuffixSuffixType
                allSimpleSorterModelTypes
                sorterEvalTypeV2
                sortingWidth96
                mergeDimension6
                largeSorterCount
            ]
            filter = paramMapFilter
            enhancer = mergeEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


        let Rand_MergeTest_LargeLd (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Merge.dbName
            runName = sprintf @"Rand_MergeTest-LargeLd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "MergeSorter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [
                rngTypeLcg
                dataFormatInt8v512
                noSuffixSuffixType
                allSimpleSorterModelTypes
                sorterEvalTypeV2
                largeMergeSortingWidths
                mergeDimension2
                largeSorterCount
            ]
            filter = paramMapFilter
            enhancer = mergeEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 2
        }


    type configType =
        | Rand_MergeTest_Test
        | Rand_MergeTest_Small
        | Rand_MergeTest_MediumLd
        | Rand_MergeTest_MediumHd
        | Rand_MergeTest_LargeLd


    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_MergeTest_Test, Specs.Rand_MergeTest_Test); 
                        (configType.Rand_MergeTest_Small, Specs.Rand_MergeTest_Small);
                        (configType.Rand_MergeTest_MediumLd, Specs.Rand_MergeTest_MediumLd);
                        (configType.Rand_MergeTest_MediumHd, Specs.Rand_MergeTest_MediumHd);
                        (configType.Rand_MergeTest_LargeLd, Specs.Rand_MergeTest_LargeLd);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterEvalExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType