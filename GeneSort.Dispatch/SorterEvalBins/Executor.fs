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


module EvalBinsExecutor =


    let makeStandardTests (host:IRunHost) (rp:runParameters) : Sortable.sortableTest option =
        maybe {
            let! sdt = rp.GetSortableDataFormat()
            let! sortingWidth = rp.GetSortingWidth()
            let qpTests = host.MakeQueryParamsFromRunParams rp (outputDataType.SortableTest "")
            let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
            return SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel sdt
        }

    let makeMergeTests (host:IRunHost) (rp:runParameters) : Sortable.sortableTest option =
        maybe {
                //let qpTests = this.MakeQueryParams (Some (0 |> UMX.tag)) (Some sw) (Some md) (Some mst) None (outputDataType.SortableTest "")
                //let dbMergeTests = new GeneSortDbMp(this._spec.MergeTestsProjectFolder |> UMX.tag) :> IGeneSortDb
                //let! rawTestData = dbMergeTests.loadAsync qpTests 
                //let! tests = rawTestData |> OutputData.asSortableTest
            let! sdt = rp.GetSortableDataFormat()
            let! sortingWidth = rp.GetSortingWidth()
            let qpTests = host.MakeQueryParamsFromRunParams rp (outputDataType.SortableTest "")
            let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
            return SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel sdt
        }


    let makeSorterModelSet (host:IRunHost) (rp:runParameters) : sorterModelSet option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! stageLength = rp.GetStageLength()
            let! simpleSorterModelType = rp.GetSimpleSorterModelType()
            let! repl = rp.GetRepl()
            let! sorterCount = rp.GetSorterCount()
            let! rngFactory = rp.GetRngType() |> Option.map RngFactory.create
            let firstIdx = (%repl * %sorterCount) |> UMX.tag<sorterCount>
            let sorterModelGen = SimpleSorterModelGen.makeUniform 
                                        rngFactory 
                                        sortingWidth stageLength simpleSorterModelType
                                 |> sorterModelGen.Simple

            let qpModelSet = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterModelSet "")
            return SorterModelGen.makeSorterModelSet 
                            (%qpModelSet.Id |> UMX.tag) firstIdx sorterCount sorterModelGen
        }


    let _makeFullSorterEvalBins 
        (makeSorterModel: IRunHost -> runParameters -> sorterModelSet option)
        (makeTests: IRunHost ->runParameters -> Sortable.sortableTest option)
        (host: IRunHost)
        (rp: runParameters) 
        (allowOverwrite: bool<allowOverwrite>) 
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        
        asyncResult {
            try
                let! (_: unit) = checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Sorter Run %s" (MathUtils.getTimestampString()) %runId)

                let! modelSet = makeSorterModel host rp |> Result.ofOption "Failed to create SorterModelSet"
                let fullSorterSet = SorterModelSet.makeSorterSet (%modelSet.Id |> UMX.tag) modelSet

                let! (_: unit) = checkCancellation cts.Token
                let! tests = makeTests host rp |> Result.ofOption "Failed to create SortableTests"

                let qpEval = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                let collectTests = rp.GetCollectSortableTests() |> Option.defaultValue false
                let sorterSetEval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) fullSorterSet tests collectTests

                let qpBins = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                let sorterEvalBins = sorterEvalBinsV1.createFromEvals 
                                            (%qpBins.Id |> UMX.tag) 
                                            (tests |> SortableTests.getId) 
                                            sorterSetEval.SorterEvals 
                                     |> sorterEvalBins.V1

                let! (_: unit) = host.ProjectDb.saveAsync qpBins (sorterEvalBins |> outputData.SorterEvalBins) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        }

    let _makeMergeSorterEvalBins 
        (makeSorterModel: IRunHost -> runParameters -> sorterModelSet option)
        (makeTests: IRunHost ->runParameters -> Sortable.sortableTest option)
        (host: IRunHost)
        (rp: runParameters) 
        (allowOverwrite: bool<allowOverwrite>) 
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        
        asyncResult {
            try
                let! (_: unit) = checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Sorter Run %s" (MathUtils.getTimestampString()) %runId)

                let! modelSet = makeSorterModel host rp |> Result.ofOption "Failed to create SorterModelSet"
                let fullSorterSet = SorterModelSet.makeSorterSet (%modelSet.Id |> UMX.tag) modelSet

                let! (_: unit) = checkCancellation cts.Token
                let! tests = makeTests host rp |> Result.ofOption "Failed to create SortableTests"

                let qpEval = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                let collectTests = rp.GetCollectSortableTests() |> Option.defaultValue false
                let sorterSetEval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) fullSorterSet tests collectTests

                let qpBins = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                let sorterEvalBins = sorterEvalBinsV1.createFromEvals 
                                            (%qpBins.Id |> UMX.tag) 
                                            (tests |> SortableTests.getId) 
                                            sorterSetEval.SorterEvals 
                                     |> sorterEvalBins.V1

                let! (_: unit) = host.ProjectDb.saveAsync qpBins (sorterEvalBins |> outputData.SorterEvalBins) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
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
                _makeFullSorterEvalBins 
                    makeSorterModelSet
                    makeStandardTests
                    host rp allowOverwrite cts progress }


    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeMergeSorterEvalBins 
                    makeSorterModelSet
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


    // --- The Dispatcher ---

    let getExecutor (executorType: executorType) : IRunParamsExecutor =
        match executorType with
        | Standard -> standardExecutor
        | Merge -> mergeExecutor
        | FullReport -> fullReportExecutor
        | BinsReport -> binsReportExecutor


