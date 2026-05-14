namespace GeneSort.Dispatch.V1.SorterEvalBins

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
open GeneSort.Model.Sorting.Simple.V1
open Common
open GeneSort.Dispatch.V1.OpsUtils

module EvalBinsExecutor =

    let makeStandardTests (rp:runParameters) : Async<Result<Sortable.sortableTest, string>> =
        async {
            let paramsOpt = option {
                let! sortingWidth = rp.GetSortingWidth()
                let! qpTests = ProjectDb.makeStandardQueryParamsFromRunParams rp (outputDataType.SortableTest "")
                return (sortingWidth, qpTests)
            }
            match paramsOpt with
            | Some (sortingWidth, qpTests) ->
                let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
                return Ok (SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel standardSortableDataFormat)
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }


    let makeMergeTests (rp: runParameters) : Async<Result<Sortable.sortableTest, string>> =
        async {
            let paramsOpt = option {
                let! repl = rp.GetRepl()
                let! sw = rp.GetSortingWidth()
                let! md = rp.GetMergeDimension()
                let! mst = rp.GetMergeSuffixType()
                let! sdf = rp.GetSortableDataFormat()
                return (repl, sw, md, mst, sdf)
            }

            match paramsOpt with
            | Some (repl, sw, md, mst, sdf) ->
                return! GeneSort.Dispatch.V1.SortableTest.ProjectDb.getMergeSorterTestSet 
                                        repl sw md mst sdf  
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }


    let makeStandardSorterModelSet (rp:runParameters) : sorterModelSet option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! simpleSorterModelType = rp.GetSimpleSorterModelType()
            let stageLength = Common.getStandardStageLength simpleSorterModelType sortingWidth

            let! repl = rp.GetRepl()
            let! sorterCount = rp.GetSorterCount()
            let! rngType = rp.GetRngType()
            let  rngFactory = rngType |> RngFactory.create
            let firstIdx = (%repl * %sorterCount) |> UMX.tag<sorterCount>
            let sorterModelGen = SimpleSorterModelGen.makeUniform 
                                    rngFactory 
                                    sortingWidth 
                                    stageLength 
                                    simpleSorterModelType
                                    ExcludeSelfCe
                                 |> sorterModelGen.Simple

            let! qpModelSet = ProjectDb.makeStandardQueryParamsFromRunParams rp (outputDataType.SorterModelSet "")
            return SorterModelGen.makeSorterModelSet 
                            (%qpModelSet.Id |> UMX.tag) firstIdx sorterCount sorterModelGen
        }


    let makeMergeSorterModelSet (rp:runParameters) : sorterModelSet option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! simpleSorterModelType = rp.GetSimpleSorterModelType()
            let stageLength = Common.getMergeStageLength simpleSorterModelType sortingWidth
            let! repl = rp.GetRepl()
            let! sorterCount = rp.GetSorterCount()
            let! rngType = rp.GetRngType()
            let  rngFactory = rngType |> RngFactory.create
            let firstIdx = (%repl * %sorterCount) |> UMX.tag<sorterCount>
            let sorterModelGen = SimpleSorterModelGen.makeUniform 
                                    rngFactory
                                    sortingWidth 
                                    stageLength 
                                    simpleSorterModelType
                                    ExcludeSelfCe
                                 |> sorterModelGen.Simple

            let! qpModelSet = ProjectDb.makeMergeQueryParamsFromRunParams rp (outputDataType.SorterModelSet "")
            return SorterModelGen.makeSorterModelSet 
                            (%qpModelSet.Id |> UMX.tag) firstIdx sorterCount sorterModelGen
        }


    let _makeSorterEvalBins 
            (makeSorterModelSet: runParameters -> sorterModelSet option)
            (makeTests: runParameters -> Async<Result<Sortable.sortableTest, string>>)
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
    
        let log msg = OpsUtils.report progress (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                // 1. Initial Check & Sorter Model Creation
                do! checkCancellation cts.Token
                log "Creating Sorter Model Set..."
            
                let! modelSet = 
                        makeSorterModelSet rp 
                        |> Result.ofOption "Failed: SorterModelSet could not be initialized."
            
                let fullSorterSet = SorterModelSet.makeSorterSet (%modelSet.Id |> UMX.tag) modelSet
                log (sprintf "Success: Created SorterSet %A" modelSet.Id)

                // 2. Test Generation (Now non-blocking Async)
                do! checkCancellation cts.Token
                log "Generating Sortable Tests..."
            
                let! tests = makeTests rp 
            
                // 3. Evaluation & Binning
                log "Running Sorter Evaluations ..."
            
                let! qpEval = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                             |> Result.ofOption "Failed to create QueryParams for SorterSetEval."

                let collectTests = Common.CollectSortableTests
                let sorterSetEval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) fullSorterSet tests collectTests

                let! qpBins = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                             |> Result.ofOption "Failed to create QueryParams for Bins."
                let binsV1 = sorterEvalBinsV1.createFromEvals 
                                (%qpBins.Id |> UMX.tag) 
                                (tests |> SortableTests.getId) 
                                sorterSetEval.SorterEvals 
            
                let sorterEvalBins = sorterEvalBins.V1 binsV1

                // 4. Persistence
                log (sprintf "Saving SorterEvalBins %s" (string %qpBins.Id))
                do! host.ProjectDb.saveAsync qpBins (sorterEvalBins |> outputData.SorterEvalBins) allowOverwrite
            
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
        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                let! qpBins = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                              |> Result.ofOption "Failed to create QueryParams for Bins."
                let! outB = host.ProjectDb.loadAsync qpBins
                let! bins = outB |> OutputData.asSorterEvalBins |> Async.singleton

                let reportName = sprintf "FullReport" |> UMX.tag<textReportName>
                let! qpReport = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = bins |> SorterEvalBins.makeFullDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.ProjectDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
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
    
                let! qpBins = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                                    |> Result.ofOption "Failed to create QueryParams for Bins."
                let! outB = host.ProjectDb.loadAsync qpBins 
                let! bins = outB |> OutputData.asSorterEvalBins |> Async.singleton

                let reportName = sprintf "BinsReport" |> UMX.tag<textReportName>
                let! qpReport = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                    |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = bins |> SorterEvalBins.makeSummaryDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.ProjectDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
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