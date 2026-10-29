namespace GeneSort.FileDb.V1

open System
open System.Threading
open System.Threading.Tasks
open System.Text.RegularExpressions
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.Mp.V1

type runRowStatus =
    | Waiting = 0
    | Running = 1
    | Completed = 2
    | Cancelled = 3
    | Failed = 4

type runPhaseProgress = private { completed: int; total: int; message: string; rowIndex: int option; generation: int option; status: runRowStatus option } with
    member this.Completed = this.completed
    member this.Total = this.total
    member this.Message = this.message
    member this.RowIndex = this.rowIndex
    member this.Generation = this.generation
    member this.Status = this.status
    static member internal create completed total message rowIndex generation status =
        { completed = completed; total = total; message = message; rowIndex = rowIndex; generation = generation; status = status }

type runPhaseResult = private { succeeded: int; skipped: int; cancelled: int; errors: string[]; updates: (int * runPhaseOutput)[]; rowStatuses: runRowStatus[]; rowErrors: string[] } with
    member this.Succeeded = this.succeeded
    member this.Skipped = this.skipped
    member this.Cancelled = this.cancelled
    member this.Failed = this.errors.Length
    member this.Errors = Array.copy this.errors
    member this.RowStatuses = Array.copy this.rowStatuses
    member this.RowErrors = Array.copy this.rowErrors
    /// Saved outputs, including restored cancelled paths, indexed by the original selection, including when the display is sorted.
    member this.Updates = Array.copy this.updates
    member this.EvolutionPaths = this.updates |> Array.choose (function _, Evolution (path, progress) -> Some (path, progress) | _ -> None)
    static member internal create succeeded skipped cancelled errors updates rowStatuses rowErrors = { succeeded = succeeded; skipped = skipped; cancelled = cancelled; errors = errors; updates = updates; rowStatuses = Array.copy rowStatuses; rowErrors = Array.copy rowErrors }

/// Shared selection validation, publication lock, persistence and progress for saved run phases.
module internal RunPhaseFiles =
    type phaseDefinition<'input> = {
        Name: string
        ToParameters: 'input -> runParameters
        Bind: run -> runStorage -> ('input -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<Result<runPhaseOutput, string>>)
        SkipFinished: bool
        SaveParameters: bool
        RecoverCancelled: (run -> runStorage -> 'input -> Async<Result<runPhaseOutput, string>>) option
    }

    let runCancellableAsync (token: CancellationToken) maxParallel (phase: phaseDefinition<'input>) projectsFolder (configured: run) (selected: 'input[]) (progress: IProgress<runPhaseProgress>) =
        // Snapshot before dispatch: neither sorting nor later caller edits can change the work list.
        if maxParallel < 1 then invalidArg "maxParallel" "Parallel threads must be positive."
        let selected = Array.copy selected
        Task.Run<runPhaseResult>(Func<Task<runPhaseResult>>(fun () -> task {
            if selected.Length = 0 then invalidArg "selected" "Select at least one RunParameter row."
            ProjectCatalogs.registerAll ()
            let root = RunFileStorage.dataRoot projectsFolder configured
            use guard = RunFileStorage.acquire root configured
            let! saved = RunDefinitionFiles.loadAsync (RunDefinitionFiles.pathFor projectsFolder configured)
            if RunDto.fromDomain saved <> RunDto.fromDomain configured then
                invalidOp (sprintf "The saved run definition has changed. Open it again before running %s." phase.Name)
            let db = new GeneSortDbMp(root, saved.RunQueryCatalogName) :> IGeneSortDb
            let queryFor = QueryParamsCatalogRun.get saved.RunQueryCatalogName saved.ProjectName saved.DatabaseName
            let query (row: runParameters) =
                if row.GetRunName() <> Some saved.RunName then invalidArg "selected" "A selected row belongs to another run."
                queryFor row (geneSortDataType.RunParameters saved.RunName)
                |> Option.defaultWith (fun () -> invalidOp "Cannot identify a selected RunParameter file.")
            let entries = selected |> Array.mapi (fun index input ->
                let row = phase.ToParameters input
                index, query row, row, input)
            if (entries |> Array.distinctBy (fun (_, qp, _, _) -> qp.Id)).Length <> entries.Length then
                invalidArg "selected" "The selection contains duplicate RunParameter rows."
            // Validate the entire selection before producing any artifacts.
            for _, qp, row, _ in entries do
                let! loaded = db.loadAsync geneSortDataMode.Project qp |> Async.StartAsTask
                let current = loaded |> Result.bind OutputData.asRunParameters |> Result.defaultWith invalidOp
                if current <> row then invalidOp (sprintf "A selected RunParameter file has changed. Refresh saved files before running %s." phase.Name)
            let storage = runStorage.create { RunDb = db; QueryParamsFromRunParams = queryFor }
            let executor = phase.Bind saved storage
            use cancellation = CancellationTokenSource.CreateLinkedTokenSource(token)
            let errors = ResizeArray<string>()
            let updates = ResizeArray<int * runPhaseOutput>()
            let mutable succeeded, skipped, cancelled, completed = 0, 0, 0, 0
            let rowErrors = Array.create entries.Length ""
            let statuses = Array.create entries.Length runRowStatus.Waiting
            let sync = obj ()
            let reportRow rowIndex generation status message = lock sync (fun () ->
                match rowIndex, status with
                | Some index, Some value -> statuses[index] <- value
                | _ -> ()
                if not (isNull progress) then
                    try progress.Report(runPhaseProgress.create completed entries.Length message rowIndex generation status) with _ -> ())
            let report = reportRow None None None
            report (sprintf "Starting %s for %d selected rows." phase.Name entries.Length)
            let processRow (index, qp: queryParams, row: runParameters, input) = task {
                let variant = row.GetMutationMod() |> Option.map (fun value -> sprintf " · MutationMod %d" (int value)) |> Option.defaultValue ""
                let label = sprintf "Replica %s%s · %s" qp.ReplAsString variant ((string qp.Id).Substring(0, 8))
                let persist ready = task {
                    let updated = RunPhaseOutput.toParameters ready
                    if phase.SaveParameters then
                        if (query updated).Id <> qp.Id then invalidOp (phase.Name + " changed the RunParameter file identity.")
                        let! persisted = db.saveAsync geneSortDataMode.Project qp (outputData.RunParameters updated) (UMX.tag true) |> Async.StartAsTask
                        persisted |> Result.defaultWith invalidOp
                        lock sync (fun () -> updates.Add(index, ready))
                }
                let markCancelled () = lock sync (fun () ->
                    cancelled <- cancelled + 1
                    completed <- completed + 1
                    reportRow (Some index) None (Some runRowStatus.Cancelled) (label + ": cancelled."))
                if phase.SkipFinished && row.GetRunFinished() = Some true then
                    lock sync (fun () ->
                        skipped <- skipped + 1
                        completed <- completed + 1
                        reportRow (Some index) None (Some runRowStatus.Completed) (label + ": skipped (execution already finished)."))
                elif cancellation.IsCancellationRequested then
                    markCancelled ()
                else
                    reportRow (Some index) None (Some runRowStatus.Running) (sprintf "%s: running %s…" label phase.Name)
                    let rowProgress =
                        { new IProgress<string> with
                            member _.Report message =
                                let generation =
                                    if phase.Name = "Execution" && saved.IsEvolution then
                                        let found = Regex.Match(message, @"\bGeneration (\d+)\b", RegexOptions.CultureInvariant)
                                        match Int32.TryParse(found.Groups[1].Value) with
                                        | true, value when found.Success -> Some value
                                        | _ -> None
                                    else None
                                reportRow (Some index) generation None (label + ": " + message.Trim()) }
                    let recover () = task {
                        match phase.RecoverCancelled with
                        | Some recover ->
                            // Recovery and persistence must finish even though the execution token is cancelled.
                            let! restored = recover saved storage input |> Async.StartAsTask
                            do! persist (restored |> Result.defaultWith invalidOp)
                        | None -> ()
                        markCancelled ()
                    }
                    try
                        let! result = task {
                            try return! executor input (UMX.tag saved.AllowOverwrite) cancellation (Some rowProgress) |> Async.StartAsTask
                            with :? OperationCanceledException when cancellation.IsCancellationRequested -> return Error "Operation cancelled"
                        }
                        match result with
                        | Error message when cancellation.IsCancellationRequested &&
                                             (message = "Operation cancelled" || message = "runEvolutionAsync execution was cancelled.") ->
                            do! recover ()
                        | _ ->
                            do! persist (result |> Result.defaultWith invalidOp)
                            lock sync (fun () ->
                                succeeded <- succeeded + 1
                                completed <- completed + 1
                                reportRow (Some index) None (Some runRowStatus.Completed) (sprintf "%s: %s complete." label phase.Name))
                    with error ->
                        let message = label + ": " + error.GetBaseException().Message
                        lock sync (fun () ->
                            rowErrors[index] <- message
                            errors.Add message
                            completed <- completed + 1
                            reportRow (Some index) None (Some runRowStatus.Failed) message)
            }
            let mutable next = -1
            let worker () = task {
                let mutable index = Interlocked.Increment(&next)
                while index < entries.Length do
                    do! processRow entries[index]
                    index <- Interlocked.Increment(&next)
            }
            let! _ = Array.init (min maxParallel entries.Length) (fun _ -> Task.Run<unit>(Func<Task<unit>>(worker))) |> Task.WhenAll
            if cancelled > 0 then
                report (sprintf "%s cancelled: %d succeeded, %d failed, %d skipped, %d cancelled." phase.Name succeeded errors.Count skipped cancelled)
            else
                report (sprintf "%s finished: %d succeeded, %d failed, %d skipped." phase.Name succeeded errors.Count skipped)
            return runPhaseResult.create succeeded skipped cancelled (errors.ToArray()) (updates.ToArray()) statuses rowErrors
        }))

    let runParallelAsync maxParallel phase projectsFolder configured selected progress =
        runCancellableAsync CancellationToken.None maxParallel phase projectsFolder configured selected progress

    let runAsync phase projectsFolder configured selected progress =
        runParallelAsync 1 phase projectsFolder configured selected progress
