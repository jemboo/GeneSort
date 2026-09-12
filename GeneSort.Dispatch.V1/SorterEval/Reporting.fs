namespace GeneSort.Dispatch.V1.SorterEval

open System
open System.Threading
open FsToolkit.ErrorHandling
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
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
open GeneSort.SortingLib.Sorter

module Reporting =

    let makeFullReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log (msg: string) : unit = 
            OpsUtils.report progress 
                (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (StringUtils.getTimestampString()) %runId)
    
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

        let log (msg: string) : unit = 
            OpsUtils.report progress 
                (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Stage stats Report for Run %s" (StringUtils.getTimestampString()) %runId)
    
                let! qpSorterSetEval = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterSetEval."
                let! outB = host.RunDb.loadAsync qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                let reportName = (sprintf "StageStatsReport" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord

                let stageStatsRecordMaker (eval: sorterEval) : dataTableRecord [] =
                    eval
                    |> SorterStageStats.fromSorterEval
                    |> Array.map (fun sss -> sss.toDataTableRecord())

                let _sorterEvalMeasure = SorterEvalMeasure.stageBiased
                let _sorterEvalSelectionType = sorterSelectionType.Tmb 300<sorterCount>
                let _sorterEvalSelection = SorterSelection.makeSelection 
                                                _sorterEvalMeasure
                                                _sorterEvalSelectionType 
                                                sorterSetEvals.SorterEvals
                                                sorterSetEvals.SorterTestId

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

        let log (msg: string) : unit = 
            OpsUtils.report progress 
                (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Ce bins Report for Run %s" (StringUtils.getTimestampString()) %runId)
    
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
