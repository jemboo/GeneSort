namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SortableTest

module SorterEvalMergeSpecs =

    let mergeDataFolder = "c:\\Projects\\SorterEvalBins\\RandomMerge\\Data"
    let rngTypes = 
            (runParameters.rngTypeKey, [rngType.Lcg;] |> List.map RngType.toString)
    
    let smallSortingWidths = SortableMergeSpecs.smallSortingWidths

    let allSortingWidths = SortableMergeSpecs.allSortingWidths

    let smallMergeDimensions = SortableMergeSpecs.smallMergeDimensions

    let allMergeDimensions = SortableMergeSpecs.allMergeDimensions

    let allDataFormats = SortableMergeSpecs.allDataFormats

    let allMergeFillTypes = SortableMergeSpecs.allMergeFillTypes

    let allSimpleSorterModelTypes = 
            (runParameters.simpleSorterModelTypeKey, SimpleSorterModelType.all() |> List.map SimpleSorterModelType.toString)

    let testSorterCount = (runParameters.sorterCountKey, ["100";] )
    let smallSorterCount = (runParameters.sorterCountKey, ["1000";] )
    let mediumSorterCount = (runParameters.sorterCountKey, ["10000";] )
    let largeSorterCount = (runParameters.sorterCountKey, ["100000";] )


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

        let Small_Dev (executorType: evalExecutorType) : runHostSpec = {
            ProjectName = Common.projectName
            DatabaseName = Common.randomMergeDatabaseName
            RunName = sprintf @"Merge_P1_%s" (EvalExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngTypes
                smallSortingWidths
                allSimpleSorterModelTypes
                smallMergeDimensions
                allMergeFillTypes
                allDataFormats
                testSorterCount
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
        }

    let Configs = Map.ofList [ ("Small_dev", Specs.Small_Dev) ]