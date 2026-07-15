namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams

module SorterEvalSpecsPrefix =

    let private mergeEnhancer 
                    (host: IRunHost) 
                    (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters host.Run.RunName)
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

            // Prefix dimension check: If it doesn't divide, return None to stop
            if (%sw % %md <> 0) then return! None
        
            return rp
        }


    module Specs =

        let RandPrefix_Test (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Prefix.dbName
            runName = sprintf @"RandPrefix-Test_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "PrefixSorter eval for Msce/Mssi/Msrs/Msuf4"
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


        let RandPrefix_Small (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Prefix.dbName
            runName = sprintf @"RandPrefix-Small_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "PrefixSorter eval for Msce/Mssi/Msrs/Msuf4"
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


        let RandPrefix_MediumLd (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Prefix.dbName
            runName = sprintf @"RandPrefix-MediumLd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "PrefixSorter eval for Msce/Mssi/Msrs/Msuf4"
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


        let RandPrefix_MediumHd (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Prefix.dbName
            runName = sprintf @"RandPrefix-MediumHd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "PrefixSorter eval for Msce/Mssi/Msrs/Msuf4"
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


        let RandPrefix_LargeLd (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Prefix.dbName
            runName = sprintf @"RandPrefix-LargeLd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "PrefixSorter eval for Msce/Mssi/Msrs/Msuf4"
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
        | RandPrefix_Test
        | RandPrefix_Small
        | RandPrefix_MediumLd
        | RandPrefix_MediumHd
        | RandPrefix_LargeLd


    let Configs = Map.ofList 
                    [ 
                        (configType.RandPrefix_Test, Specs.RandPrefix_Test); 
                        (configType.RandPrefix_Small, Specs.RandPrefix_Small);
                        (configType.RandPrefix_MediumLd, Specs.RandPrefix_MediumLd);
                        (configType.RandPrefix_MediumHd, Specs.RandPrefix_MediumHd);
                        (configType.RandPrefix_LargeLd, Specs.RandPrefix_LargeLd);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterEvalExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType