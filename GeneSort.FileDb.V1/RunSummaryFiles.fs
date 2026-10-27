namespace GeneSort.FileDb.V1

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Reports
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime

type savedSummaryReport = private {
    path: string
    generation: int
    headers: string[]
    rows: string[][]
} with
    member this.Path = this.path
    member this.Generation = this.generation
    member this.Headers = Array.copy this.headers
    member this.Rows = this.rows |> Array.map Array.copy
    static member internal create path generation headers rows =
        { path = path; generation = generation; headers = headers; rows = rows }

type summaryReportFiles = private { reports: savedSummaryReport[]; errors: string[] } with
    member this.Reports = Array.copy this.reports
    member this.Errors = Array.copy this.errors
    static member internal create reports errors = { reports = reports; errors = errors }

/// Summarize selected saved rows, including finished runs, without changing their execution state.
module RunSummaryFiles =
    /// Publish a display snapshot only after the complete TSV has been written successfully.
    let saveCombinedAsync path (report: dataTableReport) overwrite =
        Task.Run(fun () ->
            let target = Path.GetFullPath path
            let temporary = Path.Combine(Path.GetDirectoryName target, ".summary-" + Guid.NewGuid().ToString("N") + ".tmp")
            try
                do
                    use stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                    report.SaveToStream stream
                File.Move(temporary, target, overwrite)
                target
            finally
                if File.Exists temporary then File.Delete temporary)

    /// Reports contain all discovered slices. Use the latest saved report within each row's horizon,
    /// not GenerationCurrent, which can advance independently of report generation.
    let loadAsync projectsFolder (configured: run) (selected: runParameters[]) (cancellation: CancellationToken) =
        let selected = Array.copy selected
        Task.Run<summaryReportFiles>(Func<Task<summaryReportFiles>>(fun () -> task {
            ProjectCatalogs.registerAll ()
            let root = RunFileStorage.dataRoot projectsFolder configured
            let intervals = RunIntervals.forRun configured |> Option.defaultWith (fun () -> invalidArg "configured" "Summary Reports require an SGD run.")
            let queryFor = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
            let reportType = geneSortDataType.TextReport (UMX.tag "SummaryReport")
            let reports = ResizeArray<savedSummaryReport>()
            let errors = ResizeArray<string>()
            let loadedPaths = Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase)
            for row in selected do
                cancellation.ThrowIfCancellationRequested()
                let label = sprintf "Replica %s · MutationMod %s" (row.GetRepl() |> Option.map string |> Option.defaultValue "?")
                                                                 (row.GetMutationMod() |> Option.map string |> Option.defaultValue "?")
                try
                    if row.GetRunName() <> Some configured.RunName then invalidArg "selected" "A selected row belongs to another run."
                    let last = row.GetGenerationIntervalLast() |> Option.map int |> Option.defaultWith (fun () -> invalidOp "Missing final generation interval.")
                    let lastGeneration = GenIntervalConfig.getSampleAtIndex intervals last |> Option.defaultWith (fun () -> invalidOp "Invalid final generation interval.")
                    let first = row.GetGenerationFirst() |> Option.map int |> Option.defaultValue 0
                    let generations = GenIntervalConfig.getSamplesWithMinBound intervals first
                                      |> Seq.takeWhile (fun generation -> generation <= lastGeneration)
                                      |> Seq.map (fun generation -> cancellation.ThrowIfCancellationRequested(); generation)
                                      |> Seq.toArray |> Array.rev
                    let candidates = seq {
                        for generation in generations do
                            cancellation.ThrowIfCancellationRequested()
                            let qp = queryFor (row.WithGenerationCurrent(Some (UMX.tag<generationNumber> generation))) reportType
                                     |> Option.defaultWith (fun () -> invalidOp "Cannot identify this row's Summary Report.")
                            yield generation, string (OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results qp)
                    }
                    match candidates |> Seq.tryFind (snd >> File.Exists) with
                    | None -> errors.Add(label + ": No saved Summary Report. Use Generate Summary Reports first.")
                    | Some (generation, path) when loadedPaths.Add path ->
                        try
                            let! lines = File.ReadAllLinesAsync(path, cancellation)
                            if lines.Length < 2 then invalidOp "The report has no data rows."
                            let headers = lines[0].Split '\t'
                            if (headers |> Array.exists String.IsNullOrWhiteSpace) || (Array.distinct headers).Length <> headers.Length then
                                invalidOp "The report contains empty or duplicate column names."
                            let runIndex = headers |> Array.tryFindIndex ((=) ParamKeys.runNameKey)
                                           |> Option.defaultWith (fun () -> invalidOp "The report has no RunName column. Regenerate it first.")
                            let rows = lines[1..] |> Array.mapi (fun index line ->
                                cancellation.ThrowIfCancellationRequested()
                                let cells = line.Split '\t'
                                if cells.Length <> headers.Length then invalidOp (sprintf "Report line %d has %d values; expected %d." (index + 2) cells.Length headers.Length)
                                if cells[runIndex] <> string configured.RunName then invalidOp "The report belongs to another run. Regenerate it for this run."
                                cells)
                            reports.Add(savedSummaryReport.create path generation headers rows)
                        with
                        | :? OperationCanceledException as error -> raise error
                        | error -> errors.Add(sprintf "%s: %s (%s)" label error.Message path)
                    | Some _ -> ()
                with
                | :? OperationCanceledException as error -> raise error
                | error -> errors.Add(label + ": " + error.Message)
            return summaryReportFiles.create (reports.ToArray()) (errors.ToArray())
        }), cancellation)

    let runAsync projectsFolder configured selected progress =
        RunPhaseFiles.runAsync {
            Name = "Summary Reports"
            ToParameters = id
            Bind = fun configured storage ->
                let report = ReportCatalog.get ReportCatalog.evolutionSummary (WorkflowSourceFiles.create projectsFolder) configured storage
                fun row overwrite cts sink -> report row overwrite cts sink |> Async.map (Result.map Parameters)
            SkipFinished = false
            SaveParameters = false
        } projectsFolder configured selected progress
