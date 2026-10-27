namespace GeneSort.FileDb.V1

open System
open System.Threading
open System.Threading.Tasks
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.Mp.V1

type runPhaseProgress = private { completed: int; total: int; message: string } with
    member this.Completed = this.completed
    member this.Total = this.total
    member this.Message = this.message
    static member internal create completed total message = { completed = completed; total = total; message = message }

type runPhaseResult = private { succeeded: int; skipped: int; errors: string[] } with
    member this.Succeeded = this.succeeded
    member this.Skipped = this.skipped
    member this.Failed = this.errors.Length
    member this.Errors = Array.copy this.errors
    static member internal create succeeded skipped errors = { succeeded = succeeded; skipped = skipped; errors = errors }

/// Shared selection validation, publication lock, persistence and progress for saved run phases.
module internal RunPhaseFiles =
    type phaseDefinition = {
        Name: string
        Bind: run -> runStorage -> runFunction
        SkipFinished: bool
        SaveParameters: bool
    }

    let runAsync (phase: phaseDefinition) projectsFolder (configured: run) (selected: runParameters[]) (progress: IProgress<runPhaseProgress>) =
        // Snapshot before dispatch: neither sorting nor later caller edits can change the work list.
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
            let entries = selected |> Array.map (fun row -> query row, row)
            if (entries |> Array.distinctBy (fun (qp, _) -> qp.Id)).Length <> entries.Length then
                invalidArg "selected" "The selection contains duplicate RunParameter rows."
            // Validate the entire selection before producing any artifacts.
            for qp, row in entries do
                let! loaded = db.loadAsync geneSortDataMode.Project qp |> Async.StartAsTask
                let current = loaded |> Result.bind OutputData.asRunParameters |> Result.defaultWith invalidOp
                if current <> row then invalidOp (sprintf "A selected RunParameter file has changed. Refresh saved files before running %s." phase.Name)
            let storage = runStorage.create { RunDb = db; QueryParamsFromRunParams = queryFor }
            let executor = phase.Bind saved storage
            use cancellation = new CancellationTokenSource()
            let errors = ResizeArray<string>()
            let mutable succeeded, skipped, completed = 0, 0, 0
            let report message =
                if not (isNull progress) then
                    try progress.Report(runPhaseProgress.create completed entries.Length message) with _ -> ()
            report (sprintf "Starting %s for %d selected rows." phase.Name entries.Length)
            for qp, row in entries do
                let variant = row.GetMutationMod() |> Option.map (fun value -> sprintf " · MutationMod %d" (int value)) |> Option.defaultValue ""
                let label = sprintf "Replica %s%s · %s" qp.ReplAsString variant ((string qp.Id).Substring(0, 8))
                if phase.SkipFinished && row.GetRunFinished() = Some true then
                    skipped <- skipped + 1
                    completed <- completed + 1
                    report (label + ": skipped (execution already finished).")
                else
                    report (sprintf "%s: running %s…" label phase.Name)
                    let rowProgress = { new IProgress<string> with member _.Report message = report (label + ": " + message.Trim()) }
                    try
                        let! result = executor row (UMX.tag saved.AllowOverwrite) cancellation (Some rowProgress) |> Async.StartAsTask
                        let updated = result |> Result.defaultWith invalidOp
                        if phase.SaveParameters then
                            if (query updated).Id <> qp.Id then invalidOp (phase.Name + " changed the RunParameter file identity.")
                            let! persisted = db.saveAsync geneSortDataMode.Project qp (outputData.RunParameters updated) (UMX.tag true) |> Async.StartAsTask
                            persisted |> Result.defaultWith invalidOp
                        succeeded <- succeeded + 1
                        completed <- completed + 1
                        report (sprintf "%s: %s complete." label phase.Name)
                    with error ->
                        let message = label + ": " + error.GetBaseException().Message
                        errors.Add message
                        completed <- completed + 1
                        report message
            report (sprintf "%s finished: %d succeeded, %d failed, %d skipped." phase.Name succeeded errors.Count skipped)
            return runPhaseResult.create succeeded skipped (errors.ToArray())
        }))
