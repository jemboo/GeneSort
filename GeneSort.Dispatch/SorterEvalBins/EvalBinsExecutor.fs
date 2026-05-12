namespace GeneSort.Dispatch.V1.SorterEvalBins

open System
open System.Threading
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
open GeneSort.Model.Sorting.Simple.V1
open Yab


module EvalBinsExecutor =


    let makeStandardTests (rp:runParameters) : Sortable.sortableTest option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! qpTests = ProjectDb.makeQueryParamsFromRunParams rp (outputDataType.SortableTest "")
            let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
            return SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel standardSortableDataFormat
        }


    //let makeMergeTests (rp:runParameters) : Sortable.sortableTest option =
    //    maybe {
    //        let! sortingWidth = rp.GetSortingWidth()
    //        let! sdt = rp.GetSortableDataFormat()
    //        let! md = rp.GetMergeDimension()
    //        let! mst = rp.GetMergeSuffixType()
    //        let! qpTests = ProjectDb.makeQueryParamsFromRunParams rp (outputDataType.SortableTest "")
    //        let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
    //        return SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel mergeSortableDataFormat
    //    }

    let makeMergeTests (rp:runParameters) : Sortable.sortableTest option =
        maybe {
                // 1. Safe extraction
                let! (sortingWidth, mergeDim, mergeSufixType, sortableDataFormat) = 
                    maybe {
                        let! width = rp.GetSortingWidth()
                        let! mergeDim = rp.GetMergeDimension()
                        let! suffixFill = rp.GetMergeSuffixType()
                        let! dataFormat = rp.GetSortableDataFormat()
                        return (width, mergeDim, suffixFill, dataFormat)
                    }

                // 3. Create SortableTestModel
                let sortableTestModel = msasM.create sortingWidth mergeDim mergeSufixType |> sortableTestModel.MsasMi
            
                let! qpForSortableTest = ProjectDb.makeQueryParamsFromRunParams rp (outputDataType.SortableTest "") 
                return SortableTestModel.makeSortableTest 
                                            (%qpForSortableTest.Id |> UMX.tag) 
                                            sortableTestModel 
                                            sortableDataFormat
        }


    let makeStandardSorterModelSet (rp:runParameters) : sorterModelSet option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! simpleSorterModelType = rp.GetSimpleSorterModelType()
            let stageLength = Yab.getStandardStageLength simpleSorterModelType sortingWidth

            let! repl = rp.GetRepl()
            let! sorterCount = rp.GetSorterCount()
            let  rngFactory = projectRngType |> RngFactory.create
            let firstIdx = (%repl * %sorterCount) |> UMX.tag<sorterCount>
            let sorterModelGen = SimpleSorterModelGen.makeUniform 
                                        rngFactory 
                                        sortingWidth 
                                        stageLength 
                                        simpleSorterModelType
                                        ExcludeSelfCe
                                 |> sorterModelGen.Simple

            let! qpModelSet = ProjectDb.makeQueryParamsFromRunParams rp (outputDataType.SorterModelSet "")
            return SorterModelGen.makeSorterModelSet 
                            (%qpModelSet.Id |> UMX.tag) firstIdx sorterCount sorterModelGen
        }


    let makeMergeSorterModelSet (rp:runParameters) : sorterModelSet option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! simpleSorterModelType = rp.GetSimpleSorterModelType()
            let stageLength = Yab.getMergeStageLength simpleSorterModelType sortingWidth
            let! repl = rp.GetRepl()
            let! sorterCount = rp.GetSorterCount()
            let  rngFactory = projectRngType |> RngFactory.create
            let firstIdx = (%repl * %sorterCount) |> UMX.tag<sorterCount>
            let sorterModelGen = SimpleSorterModelGen.makeUniform 
                                        rngFactory
                                        sortingWidth 
                                        stageLength 
                                        simpleSorterModelType
                                        ExcludeSelfCe
                                 |> sorterModelGen.Simple

            let! qpModelSet = ProjectDb.makeQueryParamsFromRunParams rp (outputDataType.SorterModelSet "")
            return SorterModelGen.makeSorterModelSet 
                            (%qpModelSet.Id |> UMX.tag) firstIdx sorterCount sorterModelGen
        }



    let _makeSorterEvalBins 
            (makeSorterModelSet: runParameters -> sorterModelSet option)
            (makeTests: runParameters -> Sortable.sortableTest option)
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
    
        // Local reporting helper 
        let log msg = OpsUtils.report progress (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                // 1. Initial Check & Sorter Model Creation
                let! (_: unit) = checkCancellation cts.Token
                log "Creating Sorter Model Set..."
            
                let! modelSet = 
                    makeSorterModelSet rp 
                    |> Result.ofOption "Failed: SorterModelSet could not be initialized."
            
                let fullSorterSet = SorterModelSet.makeSorterSet (%modelSet.Id |> UMX.tag) modelSet
                log (sprintf "Success: Created SorterSet %A" modelSet.Id)

                // 2. Test Generation
                let! (_: unit) = checkCancellation cts.Token
                log "Generating Sortable Tests..."
            
                let! tests = 
                    makeTests rp 
                    |> Result.ofOption "Failed: SortableTests could not be generated."
            
                // 3. Evaluation & Binning
                log "Running Sorter Evaluations ..."
            
                let! qpEval = ProjectDb.makeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                             |> Result.ofOption "Failed: SorterSetEval query parameters could not be generated."
                let collectTests = rp.GetCollectSortableTests() |> Option.defaultValue false
                let sorterSetEval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) fullSorterSet tests collectTests

                let! qpBins = ProjectDb.makeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                             |> Result.ofOption "Failed: SorterEvalBins query parameters could not be generated."

                let binsV1 = sorterEvalBinsV1.createFromEvals 
                                (%qpBins.Id |> UMX.tag) 
                                (tests |> SortableTests.getId) 
                                sorterSetEval.SorterEvals 
            
                let sorterEvalBins = sorterEvalBins.V1 binsV1

                // 4. Persistence
                log (sprintf "Saving SorterEvalBins %s" (string %qpBins.Id))
                let! (_:unit) = host.ProjectDb.saveAsync qpBins (sorterEvalBins |> outputData.SorterEvalBins) allowOverwrite
            
                log "Run Complete."
                return rp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Fatal Error in %s: %s" (rp |> RunParameters.getIdString) e.Message
                log errorMsg // Ensure the error is also sent to the progress stream
                return! Error errorMsg |> async.Return
        }


    let makeFullReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        asyncResult {
            try
                let! (_: unit) = checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                // 1. Load SorterEvalBins
                let qpBins = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                let! (outB:outputData) = host.ProjectDb.loadAsync qpBins
                let! bins = outB  |> OutputData.asSorterEvalBins

                // 2. Make report
                let reportName = sprintf "FullReport" |> UMX.tag<textReportName>
                let qpReport = host.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = bins |> SorterEvalBins.makeFullDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                //// 3. Save report (Using Host DB)
                let! (_: unit) = host.ProjectDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        }


    let makeBinsReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        asyncResult {
            try
                let! (_: unit) = checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Bins Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                // 1. Load SorterEvalBins
                let qpBins = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                let! (outB:outputData) = host.ProjectDb.loadAsync qpBins
                let! bins = outB  |> OutputData.asSorterEvalBins

                // 2. Make report
                let reportName = sprintf "BinsReport" |> UMX.tag<textReportName>
                let qpReport = host.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = bins |> SorterEvalBins.makeSummaryDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                //// 3. Save report (Using Host DB)
                let! (_: unit) = host.ProjectDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        }




    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSorterEvalBins 
                    makeStandardSorterModelSet
                    makeStandardTests
                    host rp allowOverwrite cts progress }


    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSorterEvalBins 
                    makeMergeSorterModelSet
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



    let getExecutor (executorType: evalExecutorType) : IRunParamsExecutor =
        match executorType with
        | Standard -> standardExecutor
        | Merge -> mergeExecutor
        | FullReport -> fullReportExecutor
        | BinsReport -> binsReportExecutor


