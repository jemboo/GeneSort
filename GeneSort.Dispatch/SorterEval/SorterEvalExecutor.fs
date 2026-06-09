namespace GeneSort.Dispatch.V1.SorterEval

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
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.Eval.V1

module SorterEvalExecutor =

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


    /// Creates and returns the generator using CommonSorterEval.
    let makeUniformSorterModelGen (rp: runParameters) : sorterModelGen option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! simpleSorterModelType = rp.GetSimpleSorterModelType()
            let! rngType = rp.GetRngType()

            return CommonSorterEval.getSimpleUniformSorterModelGen rngType sortingWidth simpleSorterModelType
        }


    let _makeSorterEvals 
            (makeModelGen: runParameters -> sorterModelGen option)
            (makeSortableTests: runParameters -> Async<Result<Sortable.sortableTest, string>>)
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                
                // 1. Unpack run configuration
                let totalSorterCount = rp.GetSorterCount().Value
                let sortersPerSplit = 1000 |> UMX.tag<sorterCount>
                let splitFactor = %totalSorterCount / %sortersPerSplit
                
                log (sprintf "Initializing Sorter Generation over %d chunks..." splitFactor)
                
                let! sorterModelGen = 
                    makeModelGen rp 
                    |> Result.ofOption "Failed: SorterModelGen could not be initialized from parameters."

                let! repl = 
                    rp.GetRepl() 
                    |> Result.ofOption "Missing replication number."

                let! sorterEvalType =
                    rp.GetSorterEvalType() 
                    |> Result.ofOption "Missing sorterEvalType."

                // 2. Generate common evaluation dependencies
                do! checkCancellation cts.Token
                log "Generating Sortable Tests..."
                let! tests = makeSortableTests rp 

                let! qpSorterSet = 
                    host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSet "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSet."

                let! qpEval = 
                    host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSetEval."

                let collectTests = CommonSorterEval.CollectSortableTests
                let testId = tests |> SortableTests.getId
                let baseFirstIdx = (%repl * %totalSorterCount)

                // 3. Setup Accumulators for Bins and Evals
                log "Running Split Sorter Generation, Array Map Evaluations, & Aggregation..."
                
                let allChunksEvals : sorterEval array[] = Array.zeroCreate splitFactor

                for i in 0 .. (splitFactor - 1) do
                    do! checkCancellation cts.Token
                    log (sprintf "Generating and processing chunk %d of %d..." (i + 1) splitFactor)
                    
                    let pieceFirstIdx = (baseFirstIdx + (i * %sortersPerSplit)) |> UMX.tag<sorterCount>
                    
                    // Create individual sorter models for this chunk segment
                    let modelSetChunk = 
                        SorterModelGen.makeSorterModelSetFromIndexSpan 
                            (Guid.Empty |> UMX.tag) pieceFirstIdx sortersPerSplit sorterModelGen

                    // Materialize models into a functional SorterSet chunk
                    let fullSorterSetChunk = 
                        SorterModelSet.makeSorterSet (Guid.Empty |> UMX.tag) modelSetChunk

                    // Compute sorter evaluations directly from the chunk array
                    let sorterEvalsChunk = 
                        SorterSetEval.makeSorterEvals fullSorterSetChunk.Sorters tests sorterEvalType collectTests

                    
                    // Accumulate the evaluations
                    allChunksEvals.[i] <- sorterEvalsChunk
                    System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
                    GC.Collect(2, GCCollectionMode.Forced, true, true)


                // 4. Build Master SorterSetEval directly using the specified ID rule
                log "Compiling final Master SorterSetEval structure..."
                
                let correctSorterSetId = (%qpSorterSet.Id) |> UMX.tag<sorterSetId>

                let finalEvalsArray = allChunksEvals |> Array.concat
                let finalSorterSetEval = 
                    sorterSetEval.create 
                        (%qpEval.Id |> UMX.tag) 
                        correctSorterSetId 
                        testId 
                        finalEvalsArray

                // 5. Persistence
                log (sprintf "Saving Combined SorterSetEval %s" (string %qpEval.Id))
                do! host.RunDb.saveAsync qpEval (finalSorterSetEval |> outputData.SorterSetEval) allowOverwrite
                
                log "Run Complete."
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


        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                let! qpSorterSetEval = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterSetEval."
                let! outB = host.RunDb.loadAsync qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                let reportName = (sprintf "FullEvalReport" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = sorterSetEvals |> SorterSetEval.makeFullDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                let yab = (rp : runParameters).WithRunFinished(Some true)
                return yab
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)


    let makeStageStatsReport 
                    (host: IRunHost)
                    (rp: runParameters) 
                    (allowOverwrite: bool<allowOverwrite>) 
                    (cts: CancellationTokenSource) 
                    (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Stage stats Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                let! qpSorterSetEval = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterSetEval."
                let! outB = host.RunDb.loadAsync qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                let reportName = (sprintf "StageStatsReport" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord

                // Define how an individual evaluation expands into custom data table rows
                let stageStatsRecordMaker (eval: sorterEval) : dataTableRecord [] =
                    eval
                    |> SorterStageStats.fromSorterEval
                    |> Array.map (fun sss -> sss.toDataTableRecord())

                let _sorterEvalMeasure = sorterEvalMeasure.CeSt (1.0, true);
                let _sorterEvalSelectionType = sorterEvalSelectionType.Tmb 300<sorterCount>
                let _sorterEvalSelection = SorterEvalSelection.makeSelection 
                                                _sorterEvalMeasure
                                                _sorterEvalSelectionType 
                                                sorterSetEvals.SorterEvals

                let dtrs = _sorterEvalSelection
                            |> EvalReporting.toManyDataTableRecords 
                                                        leadCols 
                                                        stageStatsRecordMaker

                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return (rp : runParameters).WithRunFinished(Some true)

            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)


    let makeCeBinSummaryStats
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Ce bins Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                let! qpSorterSetEval = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterSetEval."
                let! outB = host.RunDb.loadAsync qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                let reportName = (sprintf "CeBinsReport" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord

                let evalBins = 
                        sorterSetEvals.SorterEvals
                        |> Array.filter(fun se -> se |> SorterEval.getIsSorted)
                        |> SorterEvalBinStats.makeBins

                let dtrs = evalBins
                            |> Array.map (
                                fun bin -> 
                                    bin |> CeBinSummaryStats.toDataTableRecord |> dataTableRecord.combine leadCols)
                                
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                let yab = (rp : runParameters).WithRunFinished(Some true)
                return yab
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)



    let uniformStandardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSorterEvals 
                    makeUniformSorterModelGen
                    makeStandardTests
                    host rp allowOverwrite cts progress }

    let uniformMergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSorterEvals 
                    makeUniformSorterModelGen
                    makeMergeTests
                    host rp allowOverwrite cts progress }

    let stageStatsReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeStageStatsReport
                    host rp allowOverwrite cts progress }

    let ceBinsReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeCeBinSummaryStats
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeFullReport
                    host rp allowOverwrite cts progress }


    let getExecutor (executorType: sorterEvalExecutorType) : IRunParamsExecutor =
        match executorType with
        | GenStandard -> uniformStandardExecutor
        | GenMerge -> uniformMergeExecutor
        | FullReport -> fullReportExecutor
        | StageStatsReport -> stageStatsReportExecutor
        | CeBinReport -> ceBinsReportExecutor