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

    /// Generic execution engine for dynamic generation-sliced reports.
    let private makeDynamicReportFromSlices<'T>
            (loadSlices: IGeneSortGenDb -> int<generationNumber> -> runParameters -> CancellationToken -> (string -> unit) -> Async<seq<'T>>)
            (getGeneration: 'T -> int<generationNumber>)
            (extractRecords: 'T -> dataTableRecord seq)
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

                let! (curGen: int<generationNumber>) = 
                    rp.GetGenerationCurrent() |> Result.ofOption "Missing GenerationCurrent."

                // 1. Dynamic discovery and streaming of slices
                let! slicesSeq = loadSlices genDb curGen rp cts.Token log

                if Seq.isEmpty slicesSeq then
                    return! Error (sprintf "No valid slice files discovered for %s starting at generation %d." reportNameTag %curGen)

                // 2. Accumulate records and determine maximum generation number reached
                let mutable recordCount = 0
                let mutable maxGenOpt = None
                let accumulatedDetails = System.Collections.Generic.List<dataTableRecord>()

                for slice in slicesSeq do
                    cts.Token.ThrowIfCancellationRequested()
                    recordCount <- recordCount + 1
                    let gen = getGeneration slice
                    maxGenOpt <- 
                        match maxGenOpt with
                        | None -> Some gen
                        | Some curMax -> Some (max curMax gen)

                    accumulatedDetails.AddRange(extractRecords slice)

                let lastGen = defaultArg maxGenOpt (%0 : int<generationNumber>)

                log (sprintf "Discovered and processed %d %s slice(s)." recordCount reportNameTag)

                // 3. Prepare target metadata and QueryParams
                let reportName = reportNameTag |> UMX.tag<textReportName>
                let finalRp = rp.WithGenerationCurrent(Some lastGen)

                let! qpReport = 
                    genDb.MakeQueryParamsFromRunParams finalRp (outputDataType.TextReport reportName)
                    |> Result.ofOption (sprintf "Failed to create QueryParams for output %s." reportNameTag)

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

    // --- Specific Report Builders ---

    let private makeSummaryReport (host: IRunHost) rp allowOverwrite cts progress =
        let genDb = host.RunDb :?> IGeneSortGenDb
        makeDynamicReportFromSlices
            Utils.loadAvailableSorterRunResults
            (fun srr -> srr.FinalPoolSet.GenerationNumber)
            (SorterRunResult.toDataTableRecordsIntermediateHistory "")
            "SorterRunResult_SummaryReport"
            genDb rp allowOverwrite cts progress

    let private makeSnapshotReport (host: IRunHost) rp allowOverwrite cts progress =
        let genDb = host.RunDb :?> IGeneSortGenDb
        makeDynamicReportFromSlices
            Utils.loadAvailableSorterRunResults
            (fun srr -> srr.FinalPoolSet.GenerationNumber)
            (SorterRunResult.toDataTableRecordsSnapshot "")
            "SorterRunResult_SnapshotReport"
            genDb rp allowOverwrite cts progress

    let private makePoolHistoryReport (host: IRunHost) rp allowOverwrite cts progress =
        let genDb = host.RunDb :?> IGeneSortGenDb
        makeDynamicReportFromSlices
            Utils.loadAvailableSorterPoolSetHistories
            (fun hist -> hist.SaveGeneration)
            SorterPoolSetHistory.toDataTableRecords
            "SorterPoolSetHistory_Report"
            genDb rp allowOverwrite cts progress

    // --- Executors ---

    let summaryReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeSummaryReport host rp allowOverwrite cts progress }

    let snapshotReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeSnapshotReport host rp allowOverwrite cts progress }

    let sorterPoolHistoryReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makePoolHistoryReport host rp allowOverwrite cts progress }
