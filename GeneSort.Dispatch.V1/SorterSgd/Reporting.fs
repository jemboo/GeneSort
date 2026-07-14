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
open GeneSort.Eval.V1


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
                let! genLast = rp.GetGenerationLast() |> Result.ofOption "Missing genLast."
                let! genCurrent = rp.GetGenerationCurrent() |> Result.ofOption "Missing genCurrent."
                let! genReportInterval = rp.GetGenerationReportInterval() |> Result.ofOption "Missing generation report interval."

                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (MathUtils.getTimestampString()) %runId)

                // 1. Calculate the target generation slices
                let reportGenerations = 
                    [ (%genCurrent + %genReportInterval) .. %genReportInterval .. %genLast ]
                    |> List.map UMX.tag<generationNumber>

                if List.isEmpty reportGenerations then
                    return! Error "No generation steps calculated for the full report. Verify genCurrent, genLast, and genReportInterval bounds."

                log (sprintf "Discovered %d report slices to load and collect..." (List.length reportGenerations))

                // 2. Accumulate all details records across our slices
                let mutable accumulatedDetails = Seq.empty<dataTableRecord>

                for targetGen in reportGenerations do
                    do! checkCancellation cts.Token
                    log (sprintf "Loading SorterRunResult slice for Gen %d..." %targetGen)
                    
                    let sliceRp = rp.WithGenerationCurrent (Some targetGen)
                    let! qpSlice = 
                        host.RunDb.MakeQueryParamsFromRunParams sliceRp (outputDataType.SorterRunResult "")
                        |> Result.ofOption (sprintf "Failed to create QueryParams for SorterRunResult at generation %d." %targetGen)

                    let! outData = host.RunDb.loadAsync qpSlice
                    let! (sliceResult: sorterRunResult) = outData |> OutputData.asSorterRunResult |> Async.singleton

                    // Extract the raw dataTableRecords from this slice's domain model
                    let sliceDetails = sliceResult |> SorterRunResult.toDataTableRecords ""
                    accumulatedDetails <- Seq.append accumulatedDetails sliceDetails

                // 3. Prepare query params and metadata headers for the final compiled text report
                let reportName = (sprintf "SorterRunResult_FullReport" |> UMX.tag<textReportName>)
                let newRp = rp.WithGenerationCurrent (rp.GetGenerationLast())

                let! qpReport = 
                    host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.TextReport reportName)
                    |> Result.ofOption "Failed to create QueryParams for Report."

                // Combine the collected details with the lead/metadata columns
                log "Combining collected records and generating final report format..."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let combinedDtrs = dataTableRecord.combineWithMany accumulatedDetails leadCols
                
                // 4. Transform into the actual dataTableReport
                let report = DataTableReport.fromDataTableRecords combinedDtrs

                // 5. Persist the compiled report to the database
                let! (_: unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                
                log "makeFullReport successfully completed."
                return newRp.WithRunFinished(Some true)
                
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