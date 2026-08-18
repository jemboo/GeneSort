namespace GeneSort.Dispatch.V1.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1

module Reporting =

    /// Core engine for generating dynamic reports.
    let private makeDynamicReport
            (extractRecords: sorterRunResult -> dataTableRecord seq)
            (reportNameTag: string)
            (genDb: IGeneSortGenDb)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = 
            OpsUtils.report progress (sprintf "%s [%s] %s" 
                    (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString 
                OpsUtils.report progress (sprintf "%s Starting Dynamic %s for Run %s" 
                        (StringUtils.getTimestampString()) reportNameTag %runId)

                // 1. Dynamic discovery and stream slices lazily via Utils
                let! (curGen: int<generationNumber>) = 
                            rp.GetGenerationCurrent() |> Result.ofOption "Missing GenerationCurrent."
                let! srrSeq = Utils.loadAvailableSorterRunResults genDb curGen rp cts.Token log

                if Seq.isEmpty srrSeq then
                    return! Error "No valid SorterRunResult files were discovered starting at generation 0."

                // 2. Eagerly collect records and capture max generation number instantly
                let mutable recordCount = 0
                let mutable maxGenOpt = None
                let accumulatedDetails = System.Collections.Generic.List<dataTableRecord>()

                for srr in srrSeq do
                    cts.Token.ThrowIfCancellationRequested()
                    recordCount <- recordCount + 1
                    let gen = srr.FinalPoolSet.GenerationNumber
                    maxGenOpt <- 
                        match maxGenOpt with
                        | None -> Some gen
                        | Some curMax -> Some (max curMax gen)
                    accumulatedDetails.AddRange(extractRecords srr)

                let lastGen = 
                    match maxGenOpt with
                    | Some g -> g
                    | None -> %0 : int<generationNumber>

                log (sprintf "Discovered and processed %d contiguous SorterRunResult slice(s)." recordCount)

                // 3. Prepare target metadata and query params with updated lastGen
                let reportName = reportNameTag |> UMX.tag<textReportName>
                let finalRp = rp.WithGenerationCurrent(Some lastGen)

                let! qpReport = 
                    genDb.MakeQueryParamsFromRunParams finalRp (outputDataType.TextReport reportName)
                    |> Result.ofOption "Failed to create QueryParams for output Report."

                // 4. Combine metadata lead columns with dynamic records
                log "Combining collected records into final report format..."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let combinedDtrs = dataTableRecord.combineWithMany accumulatedDetails leadCols
                let report = DataTableReport.fromDataTableRecords combinedDtrs

                // 5. Hard stop before saving if cancellation was requested
                do! checkCancellation cts.Token
                do! genDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite

                log (sprintf "%s successfully completed." reportNameTag)
                return finalRp

            with e -> 
                return! Error (sprintf "Error in %s for run %s: %s" reportNameTag (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress (fun msg -> OpsUtils.report progress msg))


    /// Generates a summary/intermediate history report across all discovered generation slices.
    let makeSummaryReport
            (host: IRunHost)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        let genDb = host.RunDb :?> IGeneSortGenDb
        makeDynamicReport 
            (SorterRunResult.toDataTableRecordsIntermediateHistory "") 
            "SorterRunResult_SummaryReport" 
            genDb rp allowOverwrite cts progress

    /// Generates a snapshot report across all discovered generation slices.
    let makeSnapshotReport
            (host: IRunHost)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        let genDb = host.RunDb :?> IGeneSortGenDb
        makeDynamicReport 
            (SorterRunResult.toDataTableRecordsSnapshot "") 
            "SorterRunResult_SnapshotReport" 
            genDb rp allowOverwrite cts progress

    /// Executor instance for full/summary reporting.
    let summaryReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeSummaryReport host rp allowOverwrite cts progress }

    /// Executor instance for snapshot reporting.
    let snapshotReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeSnapshotReport host rp allowOverwrite cts progress }