namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1
open GeneSort.Core
open GeneSort.Db.V1
open FsToolkit.ErrorHandling
open FSharp.UMX
open System
open GeneSort.Dispatch.V1
open System.Threading
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Eval.V1.Sgd


module Reporting =

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
                let newRp = rp.WithGenerationCurrent (rp.GetGenerationLast())
                let! qpSorterRunResult = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterRunResult."
                let! outB = host.RunDb.loadAsync qpSorterRunResult
                let! (srResult : sorterRunResult) = outB |> OutputData.asSorterRunResult |> Async.singleton

                let reportName = (sprintf "SorterRunResult_FullReport" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = srResult |> SorterRunResult.toDataTableRecords ""
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                let yab = (newRp : runParameters).WithRunFinished(Some true)
                return yab
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)



    let makeSummaryReport 
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
                let newRp = rp.WithGenerationCurrent (rp.GetGenerationLast())
                let! qpSorterRunResult = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterRunResult."
                let! outB = host.RunDb.loadAsync qpSorterRunResult
                let! (srResult : sorterRunResult) = outB |> OutputData.asSorterRunResult |> Async.singleton

                let reportName = (sprintf "SorterRunResult_SummaryReport" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = srResult |> SorterRunResult.toDataTableRecords ""
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                let yab = (newRp : runParameters).WithRunFinished(Some true)
                return yab
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)