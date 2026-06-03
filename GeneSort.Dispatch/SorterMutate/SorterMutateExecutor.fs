namespace GeneSort.Dispatch.V1.SorterMutate

open System
open System.Threading
open FsToolkit.ErrorHandling
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1




module SorterMutateExecutor =

    let makeStandardTests (rp:runParameters) : Async<Result<Sortable.sortableTest, string>> =
        async {
            let paramsOpt = option {
                let! sortingWidth = rp.GetSortingWidth()
                let sortableTestId = Guid.NewGuid() |> UMX.tag<sortableTestId>
                return (sortingWidth, sortableTestId)
            }
            match paramsOpt with
            | Some (sortingWidth, sortableTestId) ->
                let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
                return Ok ( SortableTestModel.makeSortableTest 
                                    sortableTestId
                                    testModel 
                                    CommonSorterEval.standardSortableDataFormat)
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }


    let makeMergeTests (rp: runParameters) : Async<Result<Sortable.sortableTest, string>> =
        async {
            let paramsOpt = option {
                let repl = 0 |> UMX.tag<replNumber>   
                let! sw = rp.GetSortingWidth()
                let! md = rp.GetMergeDimension()
                let! mst = rp.GetMergeSuffixType()
                let! sdf = rp.GetSortableDataFormat()
                return (repl, sw, md, mst, sdf)
            }

            match paramsOpt with
            | Some (repl, sw, md, mst, sdf) ->
                return! SortableTestDb.getMergeSorterTestSet 
                                        repl sw md mst sdf  
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }


    let makeStandardEvalGroupSelection (rp:runParameters) : Async<Result<evalGroupSelection, string>> =
        asyncResult {

            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (sorterEvalType: sorterEvalType) =
                        rp.GetSorterEvalType()
                        |> Result.ofOption "Missing sorter eval type in run parameters"


            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getStandardSorterEvals 
                                            sortingWidth 
                                            simpleSorterModelType
                                            sorterEvalType

            let! (sorterParentCount: int<sorterCount>) = 
                        rp.GetSorterParentCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let! (sorterChildCount: int<sorterCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let ranker = SorterEval.byEqualTwoWeighted

            let egs = TmbSorterEvalGroups.fromEvaluations
                                        ranker
                                        (%sorterParentCount / 3)
                                        parentSorterSetEval.SorterEvals
            return evalGroupSelection.Tmb egs
        }



    let makeMergeEvalGroupSelection (rp:runParameters) : Async<Result<evalGroupSelection, string>> =
        asyncResult {

            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (mergeDimension: int<mergeDimension>) = 
                        rp.GetMergeDimension() 
                        |> Result.ofOption "Missing mergeDimension in run parameters"

            let! (mergeSuffixType: mergeSuffixType) = 
                        rp.GetMergeSuffixType() 
                        |> Result.ofOption "Missing mergeSuffixType in run parameters"

            let! (sorterEvalType: sorterEvalType) =
                        rp.GetSorterEvalType()
                        |> Result.ofOption "Missing sorter eval type in run parameters"

            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getMergeSorterEvals 
                                        sortingWidth 
                                        simpleSorterModelType 
                                        mergeDimension 
                                        mergeSuffixType
                                        sorterEvalType


            let! (parentSorterCount: int<sorterCount>) = 
                        rp.GetSorterParentCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"


            let ranker = SorterEval.byEqualTwoWeighted

            let egs = TmbSorterEvalGroups.fromEvaluations
                                        ranker
                                        (%parentSorterCount / 3)
                                        parentSorterSetEval.SorterEvals
            return evalGroupSelection.Tmb egs
        }



    let getParentSorterModelMutator (rp:runParameters) : Async<Result<simpleSorterModelMutator, string>> =
        asyncResult {

            let! (simpleSorterModelType: simpleSorterModelType) =  
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (mutationRate: float<mutationRate>) =  
                        rp.GetMutationRate()
                        |> Result.ofOption "Missing mutationRate in run parameters"

            let! (insertionRate: float<insertionRate>) =  
                        rp.GetInsertionRate()
                        |> Result.ofOption "Missing insertionRate in run parameters"

            let! (deletionRate: float<deletionRate>) =  
                        rp.GetDeletionRate()
                        |> Result.ofOption "Missing deletionRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"


            let rngFactory = rngType |> RngFactory.create


            return SimpleSorterModelMutator.getSimpleSorterModelMutator 
                        simpleSorterModelType
                        rngFactory
                        CommonSorterMutate.ExcludeSelfCe
                        modificationRate
                        mutationRate
                        insertionRate
                        deletionRate
        }


    let getParentSorterModelSetStandard                         
                        (sortingWidth: int<sortingWidth>)
                        (simpleSorterModelType: simpleSorterModelType)
                        (parentSorterCount: int<sorterCount>)
                        (rngType: rngType): Async<Result<sorterModelSet, string>> =
        asyncResult {

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen rngType sortingWidth simpleSorterModelType




            let parentSmsId = Guid.Empty |> UMX.tag<sorterModelSetId>

            //let! (parentSorterModelMutator: simpleSorterModelMutator) = getParentSorterModelMutator rp
            //let! (parentSorterCount: int<sorterCount>) = 
            //            rp.GetSorterParentCount()
            //            |> Result.ofOption "Missing parent sorter count in run parameters"

            return sorterModelSet.empty
        }


    let getMutantSorterSet (rp: runParameters) : Async<Result<sorterModelSet, string>> =
        asyncResult {
            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"
            let! (parentSorterCount: int<sorterCount>) = 
                        rp.GetSorterParentCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"
            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (mutationRate: float<mutationRate>) =  
                        rp.GetMutationRate()
                        |> Result.ofOption "Missing mutationRate in run parameters"

            let! (insertionRate: float<insertionRate>) =  
                        rp.GetInsertionRate()
                        |> Result.ofOption "Missing insertionRate in run parameters"

            let! (deletionRate: float<deletionRate>) =  
                        rp.GetDeletionRate()
                        |> Result.ofOption "Missing deletionRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"


            let rngFactory = rngType |> RngFactory.create





            let! (parentSorterModelSet :sorterModelSet) = 
                    getParentSorterModelSetStandard 
                            sortingWidth simpleSorterModelType parentSorterCount rngType






            let (parentSorterModelMutator :simpleSorterModelMutator) = 
                                SimpleSorterModelMutator.getSimpleSorterModelMutator 
                                                simpleSorterModelType
                                                rngFactory
                                                CommonSorterMutate.ExcludeSelfCe
                                                modificationRate
                                                mutationRate
                                                insertionRate
                                                deletionRate

            let (mutantSorterSet: sorterModelSet) = sorterModelSet.empty

            return mutantSorterSet
        }

    let _evaluateMutants 
            (makeEvalGroupSelection: runParameters -> Async<Result<evalGroupSelection, string>> )
            (makeSortableTests: runParameters -> Async<Result<sortableTest, string>>)
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                // 1. Initial Check & Splitting Sorter Model Creation
                do! checkCancellation cts.Token
                let! (sortingWidth: int<sortingWidth>) = 
                            rp.GetSortingWidth() 
                            |> Result.ofOption "Missing sorting width in run parameters"
    
                let! (simpleSorterModelType: simpleSorterModelType) =  
                            rp.GetSimpleSorterModelType() 
                            |> Result.ofOption "Missing simple sorter model type in run parameters"

                let! (rngType: rngType) =  
                            rp.GetRngType()
                            |> Result.ofOption "Missing RNG type in run parameters"

                let! (mutationRate: float<mutationRate>) =  
                            rp.GetMutationRate()
                            |> Result.ofOption "Missing mutationRate in run parameters"

                let! (insertionRate: float<insertionRate>) =  
                            rp.GetInsertionRate()
                            |> Result.ofOption "Missing insertionRate in run parameters"

                let! (deletionRate: float<deletionRate>) =  
                            rp.GetDeletionRate()
                            |> Result.ofOption "Missing deletionRate in run parameters"

                let! (modificationRate: float<modificationRate>) =  
                            rp.GetModificationRate()
                            |> Result.ofOption "Missing modificationRate in run parameters"







                let! (parentSorterModelMutator: simpleSorterModelMutator) = 
                                        getParentSorterModelMutator rp

                let! (childSorterCount: int<sorterCount>) = 
                            rp.GetSorterChildCount()
                            |> Result.ofOption "Missing child sorter count in run parameters"


                return rp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Fatal Error in %s: %s" (rp |> RunParameters.getIdString) e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)



    let makeFullReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                let! qpBins = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                              |> Result.ofOption "Failed to create QueryParams for Bins."
                let! outB = host.RunDb.loadAsync qpBins
                let! bins = outB |> OutputData.asSorterEvalBins |> Async.singleton

                let reportName = sprintf "FullReport" |> UMX.tag<textReportName>
                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = bins |> SorterEvalBins.makeFullDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        }


    let makeBinsReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Bins Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                let! qpBins = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                                    |> Result.ofOption "Failed to create QueryParams for Bins."
                let! outB = host.RunDb.loadAsync qpBins 
                let! bins = outB |> OutputData.asSorterEvalBins |> Async.singleton

                let reportName = sprintf "BinsReport" |> UMX.tag<textReportName>
                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                    |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = bins |> SorterEvalBins.makeSummaryDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        }

    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _evaluateMutants 
                    makeStandardEvalGroupSelection
                    makeStandardTests
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _evaluateMutants 
                    makeMergeEvalGroupSelection
                    makeMergeTests
                    host rp allowOverwrite cts progress }

    let binsReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeBinsReport
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeFullReport
                    host rp allowOverwrite cts progress }

    let getExecutor (executorType: sorterMutateExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterMutateExecutorType.GenStandard -> standardExecutor
        | sorterMutateExecutorType.GenMerge -> mergeExecutor
        | sorterMutateExecutorType.FullReport -> fullReportExecutor
        | sorterMutateExecutorType.BinsReport -> binsReportExecutor





