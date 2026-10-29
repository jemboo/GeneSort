namespace GeneSort.Project.V1.Catalogs.Reports.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Bins.Standard
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Runtime.OpsUtils
open GeneSort.Project.V1.Catalogs.Evolution

module Reporting =

    /// Generic execution engine for dynamic generation-sliced reports.
    let private makeDynamicReportFromSlices<'T>
            (loadSlices: genIntervalConfig -> IGeneSortDb -> int<generationNumber> -> runParameters -> CancellationToken -> (string -> unit) -> Async<seq<'T>>)
            (getGeneration: 'T -> int<generationNumber>)
            (extractRecords: 'T -> dataTableRecord seq)
            (reportNameTag: string)
            (runName: string<runName>)
            (saveIntervals: genIntervalConfig)
            (genDb: IGeneSortDb)
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

                let curGen = rp.GetGenerationFirst() |> Option.defaultValue (UMX.tag<generationNumber> 0)

                // 1. Dynamic discovery and streaming of slices
                let! slicesSeq = loadSlices saveIntervals genDb curGen rp cts.Token log

                let slicesSeq = slicesSeq |> Seq.toArray

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

                if accumulatedDetails.Count = 0 then
                    return! Error (sprintf "%s contains no report rows across %d saved slices. Check whether the execution's sorter pools are empty." reportNameTag recordCount)

                // 3. Prepare target metadata and QueryParams
                let reportName = reportNameTag |> UMX.tag<textReportName>
                let finalRp = rp.WithGenerationCurrent(Some lastGen )

                let! qpReport = 
                    genDb.makeQueryParamsFromRunParams finalRp (geneSortDataType.TextReport reportName)
                    |> Result.ofOption (sprintf "Failed to create QueryParams for output %s." reportNameTag)

                // 4. Combine metadata lead columns with dynamic records
                log "Combining collected records into final report format..."
                let leadCols = qpReport |> QueryParams.makeReportDataTableRecord runName
                let combinedDtrs = dataTableRecord.combineWithMany accumulatedDetails leadCols
                let report = DataTableReport.fromDataTableRecords combinedDtrs

                // 5. Hard stop before saving if cancellation was requested
                do! checkCancellation cts.Token
                do! genDb.saveAsync geneSortDataMode.Results qpReport (report |> outputData.TextReport) allowOverwrite

                log (sprintf "%s successfully completed." reportNameTag)
                return finalRp

            with
            | :? System.OperationCanceledException when cts.IsCancellationRequested -> return! Error "Operation cancelled"
            | e ->
                return! Error (sprintf "Error in %s for run %s: %s" reportNameTag (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress (fun msg -> OpsUtils.report progress msg))

    // --- Specific Report Builders ---
    let private makeSummaryReport (configured: run) (storage: runStorage) rp allowOverwrite cts progress =
        match RunIntervals.forRun configured with
        | None -> async { return Error "Generation reports require an SgdRun with generation save intervals." }
        | Some saveIntervals ->
          let genDb = storage.RunDb
          let recordExtractor (prefix:string) (spsSummaries: sorterPoolSetSummary_Standard seq) : dataTableRecord seq =
                spsSummaries |> Seq.collect(fun poolSetSummary ->
                poolSetSummary
                |> SorterPoolSetSummary_Standard.toDataTableRecords prefix
            )
          makeDynamicReportFromSlices
            Utils.loadAvailableSorterPoolSetSummarySets
            (fun spss -> spss.LastGeneration)
            (SpSummarySet.toDataTableRecords "")
            "SummaryReport"
            configured.RunName
            saveIntervals
            genDb rp allowOverwrite cts progress


    let private makeSnapshotReport (configured: run) (storage: runStorage) rp allowOverwrite cts progress =
        match RunIntervals.forRun configured with
        | None -> async { return Error "Generation reports require an SgdRun with generation save intervals." }
        | Some saveIntervals ->
          let genDb = storage.RunDb
          makeDynamicReportFromSlices
            Utils.loadAvailableSorterPoolSets
            (fun srtrPoolSet -> srtrPoolSet.GenerationNumber)
            (SorterPoolSet.toDataTableRecordsSnapshot "")
            "SnapshotReport"
            configured.RunName
            saveIntervals
            genDb rp allowOverwrite cts progress


    let private makePoolHistoryReport (configured: run) (storage: runStorage) rp allowOverwrite cts progress =
        match RunIntervals.forRun configured with
        | None -> async { return Error "Generation reports require an SgdRun with generation save intervals." }
        | Some saveIntervals ->
          let genDb = storage.RunDb
          makeDynamicReportFromSlices
            Utils.loadAvailableSorterPoolSetHistories
            (fun hist -> hist.SaveGeneration)
            Spsh.toDataTableRecords
            "SorterPoolSetHistoryReport"
            configured.RunName
            saveIntervals
            genDb rp allowOverwrite cts progress


    let private makePoolBinsReport (configured: run) (storage: runStorage) rp allowOverwrite cts progress =
        match RunIntervals.forRun configured with
        | None -> async { return Error "Generation reports require an SgdRun with generation save intervals." }
        | Some saveIntervals ->
          let genDb = storage.RunDb
          makeDynamicReportFromSlices
            Utils.loadAvailableSorterPoolBins
            (fun hist -> hist.MaxGeneration)
            SorterPoolBinsSetSeries.makeDataTableRecords
            "SorterPoolBinsReport"
            configured.RunName
            saveIntervals
            genDb rp allowOverwrite cts progress


    // --- Executors ---

    let summaryReportExecutor =
        makeSummaryReport

    let snapshotReportExecutor =
        makeSnapshotReport

    let poolHistoryReportExecutor =
        makePoolHistoryReport

    let poolBinsReportExecutor =
        makePoolBinsReport
