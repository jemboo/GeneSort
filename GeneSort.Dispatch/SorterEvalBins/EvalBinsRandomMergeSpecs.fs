namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SortableTest

module EvalBinsRandomMergeSpecs =

    let mergeDataFolder = "c:\\Projects\\EvalBins\\RandomMerge\\Data"

    let rngType = 
            (runParameters.rngTypeKey, [rngType.Lcg;] |> List.map RngType.toString)
    
    // SortingWidths
    let smallSortingWidths = SortableTetsMergeSpecs.smallSortingWidths
    let mediumSortingWidths = SortableTetsMergeSpecs.mediumSortingWidths
    
    // MergeDimensions
    let allMergeDimensions = SortableTetsMergeSpecs.allMergeDimensions
    let lowMergeDimensions = SortableTetsMergeSpecs.lowMergeDimensions
    let highMergeDimensions = SortableTetsMergeSpecs.highMergeDimensions

    // DataFormats
    let onlyDataFormat = SortableTetsMergeSpecs.onlyDataFormat
    
    // MergeSuffixTypes
    let bothMergeSuffixTypes = SortableTetsMergeSpecs.bothMergeSuffixTypes
    let vv1SuffixType = SortableTetsMergeSpecs.vv1SuffixType

    // SimpleSorterModelTypes
    let allSimpleSorterModelTypes = EvalBinsRandomStandardSpecs.allSimpleSorterModelTypes

    // SorterCounts
    let testSorterCount = EvalBinsRandomStandardSpecs.testSorterCount
    let smallSorterCount = EvalBinsRandomStandardSpecs.smallSorterCount
    let mediumSorterCount = EvalBinsRandomStandardSpecs.mediumSorterCount
    let largeSorterCount = EvalBinsRandomStandardSpecs.largeSorterCount



    let private mergeEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters host.Run.RunName)
                 |> Option.get
        rp.WithProjectName(Some host.Run.ProjectName)
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
                    if isMuf4able then Some () else None
                | simpleSorterModelType.Msuf6 -> 
                    if isMuf6able then Some () else None
                | _ -> None

            // Merge dimension check: If it doesn't divide, return None to stop
            if (%sw % %md <> 0) then return! None
        
            // Suffix check: If it's NoSuffix and width > 64, return None to stop
            if (mst.IsNoSuffix && %sw > 64) then return! None
        
            return rp
        }


    module Specs =

        let EvalBins_Merge_single (executorType: evalBinsExecutorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.randomMergeDatabaseName
            RunName = sprintf @"EvalBins_Merge_single_%s" (EvalBinsExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msuf4"
            Spans = [   
                rngType
                (runParameters.sortingWidthKey, [64] |> List.map string)
                allSimpleSorterModelTypes
                (runParameters.mergeDimensionKey, [2;] |> List.map string)
                (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix;] |> List.map MergeSuffixType.toString)
                onlyDataFormat
                (runParameters.sorterCountKey, ["1000";] )
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        let EvalBins_Merge_test (executorType: evalBinsExecutorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.randomMergeDatabaseName
            RunName = sprintf @"EvalBins_Merge_test_%s" (EvalBinsExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [   
                rngType
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


        let EvalBins_Merge_small (executorType: evalBinsExecutorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.randomMergeDatabaseName
            RunName = sprintf @"EvalBins_Merge_small_%s" (EvalBinsExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                smallSortingWidths
                allSimpleSorterModelTypes
                allMergeDimensions
                bothMergeSuffixTypes
                onlyDataFormat
                smallSorterCount
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }


        let EvalBins_Merge_medium_Ld (executorType: evalBinsExecutorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.randomMergeDatabaseName
            RunName = sprintf @"EvalBins_Merge_medium_Ld_%s" (EvalBinsExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                mediumSortingWidths
                allSimpleSorterModelTypes
                lowMergeDimensions
                bothMergeSuffixTypes
                onlyDataFormat
                smallSorterCount
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }


        let EvalBins_Merge_medium_Hd (executorType: evalBinsExecutorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.randomMergeDatabaseName
            RunName = sprintf @"EvalBins_Merge_medium_Hd_%s" (EvalBinsExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
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
        | EvalBins_Merge_single
        | EvalBins_Merge_Test
        | EvalBins_Merge_Small
        | EvalBins_Merge_Medium_Ld
        | EvalBins_Merge_Medium_Hd


    let Configs = Map.ofList 
                    [ 
                        (configType.EvalBins_Merge_single, Specs.EvalBins_Merge_single);
                        (configType.EvalBins_Merge_Test, Specs.EvalBins_Merge_test); 
                        (configType.EvalBins_Merge_Small, Specs.EvalBins_Merge_small);
                        (configType.EvalBins_Merge_Medium_Ld, Specs.EvalBins_Merge_medium_Ld);
                        (configType.EvalBins_Merge_Medium_Hd, Specs.EvalBins_Merge_medium_Hd);
                    ]

    let getConfig (config: configType) (executorType: evalBinsExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType