namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.SortingOps

module SorterEvalSpecsRandomMerge =

    let rngType = 
            (runParameters.rngTypeKey, 
            [CommonSorterEval.projectRngType;] |> List.map RngType.toString)

    let sorterEvalType = 
            (runParameters.sorterEvalTypeKey, 
            [ sorterEvalType.V2 ;] |> List.map SorterEvalType.toString)
    
    // SortingWidths
    let smallSortingWidths = SortableTestMergeSpecs.smallSortingWidths
    let mediumSortingWidths = SortableTestMergeSpecs.mediumSortingWidths
    
    // MergeDimensions
    let singleMergeDimension = SortableTestMergeSpecs.mergeDimension2
    let allMergeDimensions = SortableTestMergeSpecs.allMergeDimensions
    let lowMergeDimensions = SortableTestMergeSpecs.lowMergeDimensions
    let highMergeDimensions = SortableTestMergeSpecs.highMergeDimensions

    // DataFormats
    let onlyDataFormat = 
            (runParameters.sortableDataFormatKey, 
            [CommonSorterEval.mergeSortableDataFormat] |> List.map SortableDataFormat.toString)
    
    // MergeSuffixTypes
    let bothMergeSuffixTypes = SortableTestMergeSpecs.bothMergeSuffixTypes
    let vv1SuffixType = SortableTestMergeSpecs.vv1SuffixType

    // SimpleSorterModelTypes
    let allSimpleSorterModelTypes = SorterEvalSpecsRandom.allSimpleSorterModelTypes

    // SorterCounts
    let testSorterCount = SorterEvalSpecsRandom.testSorterCount
    let smallSorterCount = SorterEvalSpecsRandom.smallSorterCount
    let mediumSorterCount = SorterEvalSpecsRandom.mediumSorterCount
    let largeSorterCount = SorterEvalSpecsRandom.largeSorterCount



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
            if (mst.IsNoSuffix && %sw > 128) then return! None
        
            return rp
        }


    module Specs =

        let RandMerge_Single (executorType: sorterEvalExecutorType) : runHostSpec = {
            DatabaseName = SorterEvalDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"RandMerge-Single_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msuf4"
            Spans = [   
                rngType
                sorterEvalType
                (runParameters.sortingWidthKey, [32] |> List.map string)
                allSimpleSorterModelTypes
                (runParameters.mergeDimensionKey, [8;] |> List.map string)
                (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix;] |> List.map MergeSuffixType.toString)
                onlyDataFormat
                (runParameters.sorterCountKey, ["1000";] )
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        let RandMerge_Test (executorType: sorterEvalExecutorType) : runHostSpec = {
            DatabaseName = SorterEvalDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"RandMerge-Test_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [   
                rngType
                sorterEvalType
                smallSortingWidths
                allSimpleSorterModelTypes
                lowMergeDimensions
                bothMergeSuffixTypes
                onlyDataFormat
                testSorterCount
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }


        let RandMerge_Small (executorType: sorterEvalExecutorType) : runHostSpec = {
            DatabaseName = SorterEvalDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"RandMerge-Small_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                sorterEvalType
                smallSortingWidths
                allSimpleSorterModelTypes
                allMergeDimensions
                bothMergeSuffixTypes
                onlyDataFormat
                mediumSorterCount
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 1
        }


        let RandMerge_MediumLd (executorType: sorterEvalExecutorType) : runHostSpec = {
            DatabaseName = SorterEvalDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"RandMerge-MediumLd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                sorterEvalType
                mediumSortingWidths
                allSimpleSorterModelTypes
                lowMergeDimensions
                bothMergeSuffixTypes
                onlyDataFormat
                mediumSorterCount
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }


        let RandMerge_MediumHd (executorType: sorterEvalExecutorType) : runHostSpec = {
            DatabaseName = SorterEvalDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"RandMerge-MediumHd_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                sorterEvalType
                mediumSortingWidths
                allSimpleSorterModelTypes
                highMergeDimensions
                vv1SuffixType
                onlyDataFormat
                smallSorterCount
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }


    type configType =
        | RandMerge_Single
        | RandMerge_Test
        | RandMerge_Small
        | RandMerge_MediumLd
        | RandMerge_MediumHd


    let Configs = Map.ofList 
                    [ 
                        (configType.RandMerge_Single, Specs.RandMerge_Single);
                        (configType.RandMerge_Test, Specs.RandMerge_Test); 
                        (configType.RandMerge_Small, Specs.RandMerge_Small);
                        (configType.RandMerge_MediumLd, Specs.RandMerge_MediumLd);
                        (configType.RandMerge_MediumHd, Specs.RandMerge_MediumHd);
                    ]

    let getConfig (config: configType) (executorType: sorterEvalExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType