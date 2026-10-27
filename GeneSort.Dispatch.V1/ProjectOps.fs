namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime.OpsUtils
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Execution

module ProjectOps =

    /// Internal logic for a single unit of work
    let private runTask skipFinished persistParameters (db: IGeneSortDb)
                        (runName: string<runName>)
                        (buildQueryParams: runParameters -> geneSortDataType -> queryParams option)
                        parametersFor
                        executor 
                        allowOverwrite 
                        cts 
                        progress 
                        input =
        async {
            let rp: runParameters = parametersFor input
            let id = rp.GetId() |> Option.defaultValue (Guid.Empty |> UMX.tag)
            let repl = rp.GetRepl() |> Option.defaultValue (-1 |> UMX.tag)
    
            try
                if skipFinished && (rp.GetRunFinished() |> Option.defaultValue false) then
                    return Skipped (id, repl, sprintf "\n%s" (rp |> RunParameters.reportKvps))
                else
                    report progress 
                        (
                            sprintf "%s Started:\n%s" 
                                        (StringUtils.getTimestampString()) 
                                        (rp |> RunParameters.reportKvps)
                        )
                
                    // 1. Unwrapping the Result manually inside the async block
                    let! execResult = executor input allowOverwrite cts progress
                
                    match execResult with
                    | Error m -> 
                        return Failure (id, repl, sprintf "%s\n%s" m (rp |> RunParameters.reportKvps))
                    | Ok updated when persistParameters ->
                        let qp = buildQueryParams updated (geneSortDataType.RunParameters %runName)
                                 |> Option.defaultWith (fun () -> 
                                        let msg = sprintf "Failed to build query parameters for saving results of run %s, repl %d" 
                                                            (string %runName) (int %repl)
                                        report progress msg
                                        raise (Exception msg)
                                    )
                        // 2. Standard async bind (no Result wrapper here)
                        let! saved = db.saveAsync geneSortDataMode.Project qp (updated |> outputData.RunParameters) (true |> UMX.tag<allowOverwrite>)
                        match saved with
                        | Ok () -> return Success (id, repl, (updated |> RunParameters.reportKvps))
                        | Error error -> return Failure (id, repl, sprintf "Failed to save run parameters: %s" error)
                    | Ok updated ->
                        return Success (id, repl, (updated |> RunParameters.reportKvps))
            with 
            | :? OperationCanceledException -> return Cancelled (id, repl)
            | e -> return Failure (id, repl, sprintf "Fault: %s" e.Message)
        }

    /// The single entry point for executing a batch of runs
    let private executeBatch skipFinished persistParameters
            (minRepl: int<replNumber>)
            (maxRepl: int<replNumber>)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option)
            (host: runHost)
            (executor: runFunction)
            (maxParallel: int) =
        asyncResult {
            let timer = System.Diagnostics.Stopwatch.StartNew()
            let db = host.RunDb
            let runName = host.Run.RunName

            try
                report progress (sprintf "%s Starting project: %s" (StringUtils.getTimestampString()) %runName)

                let! (paramsArray: runParameters array) = 
                        db.getRunParameters 
                                runName (Some minRepl) (Some maxRepl) (Some cts.Token) progress
                
                if paramsArray.Length = 0 then 
                    report progress "No work found."
                    return [||]
                else
                    // Map parameters to tasks and execute with internal throttling
                    let! results = 
                        paramsArray 
                        |> Seq.map (runTask skipFinished persistParameters db runName host.QueryParamsFromRunParams
                                            id executor allowOverwrite cts progress)
                        |> fun tasks -> Async.Parallel(tasks, maxParallel)

                    // Unified reporting logic
                    let s, f, sk, c = 
                        results |> Array.fold (fun (s, f, sk, c) -> function
                            | Success _ -> (s+1, f, sk, c) | Failure _ -> (s, f+1, sk, c)
                            | Skipped _ -> (s, f, sk+1, c) | Cancelled _ -> (s, f, sk, c+1)
                        ) (0, 0, 0, 0)

                    timer.Stop()
                    report progress (sprintf "\n=== Batch Complete ===\nTotal elapsed time: %s\nSucceeded: %d, Failed: %d, Skipped: %d, Cancelled: %d" (timer.Elapsed.ToString("c")) s f sk c)
                    return results
            with e -> 
                let msg = sprintf "Fatal: %s" e.Message
                report progress msg
                return! async { return Error msg }
        }

    /// Executes the run function resolved on the host.
    let executeRuns minRepl maxRepl allowOverwrite cts progress (host: runHost) maxParallel =
        executeBatch true true minRepl maxRepl allowOverwrite cts progress host host.ExecuteFunction maxParallel

    /// Reopen saved executions once, at the application boundary.
    let reopenPaths minRepl maxRepl (cts: CancellationTokenSource) progress (host: runHost) = asyncResult {
        try
            let! rows = host.RunDb.getRunParameters host.Run.RunName (Some minRepl) (Some maxRepl) (Some cts.Token) progress
            return rows |> Array.map (EvoPath.fromParameters host.Run)
        with error -> return! Error error.Message
    }

    /// Execute the supplied paths directly and retain their updated progress after successful saves.
    let executePaths allowOverwrite cts progress (host: runHost) maxParallel (paths: (evoPath * evoProgress)[]) =
        let paths = Array.copy paths
        asyncResult {
            try
                if (paths |> Array.distinctBy (fun (path, _) -> path.Id)).Length <> paths.Length then
                    invalidArg "paths" "The selection contains duplicate evolutionary paths."
                // Validate storage before any execution; changed files require an explicit reopen.
                for path, state in paths do
                    EvoPath.requireRun host.Run path
                    let row = EvoPath.toParameters path state
                    let query = host.QueryParamsFromRunParams row (geneSortDataType.RunParameters host.Run.RunName)
                                |> Option.defaultWith (fun () -> invalidOp "Cannot identify the RunParameter file.")
                    let! current = host.RunDb.loadAsync geneSortDataMode.Project query |> Async.map (Result.bind OutputData.asRunParameters)
                    do! if current = row then Ok () else Error "A selected RunParameter file has changed. Reopen the saved run before execution."
                let execute = ExecuteFunctionCatalog.bindPath host.Storage
                let! completed =
                    paths |> Seq.map (fun (path, state) -> async {
                        let mutable updatedState = state
                        let executor (path, state) overwrite cancellation sink = async {
                            let! result = execute path state overwrite cancellation sink
                            return result |> Result.map (fun updated ->
                                updatedState <- updated
                                EvoPath.toParameters path updated)
                        }
                        let! result = runTask true true host.RunDb host.Run.RunName host.QueryParamsFromRunParams
                                             (fun (path, state) -> EvoPath.toParameters path state)
                                             executor allowOverwrite cts progress (path, state)
                        let retained = match result with Success _ -> updatedState | _ -> state
                        return result, (path, retained)
                    }) |> fun tasks -> Async.Parallel(tasks, maxParallel)
                return Array.unzip completed
            with error -> return! Error error.Message
        }

    /// Local preparation only. Persist all Setup artifacts before transferring Project and Setup.
    let prepareRuns minRepl maxRepl allowOverwrite cts progress (host: runHost) maxParallel =
        asyncResult {
            try
                let paths = System.Collections.Concurrent.ConcurrentBag<evoPath * evoProgress>()
                let setup = SetupExecutorCatalog.prepare WorkflowSources.defaults host.Run host.Storage
                let setupExecutor : runFunction = fun row overwrite cancellation sink -> async {
                    let! result = setup row overwrite cancellation sink
                    return result |> Result.map (fun ready ->
                        match ready with
                        | Evolution (path, progress) -> paths.Add(path, progress)
                        | Parameters _ -> ()
                        RunPhaseOutput.toParameters ready)
                }
                let! results = executeBatch true true minRepl maxRepl allowOverwrite cts progress host setupExecutor maxParallel
                let succeeded = results |> Array.choose (function Success (id, _, _) -> Some id | _ -> None) |> Set.ofArray
                let saved = paths |> Seq.filter (fun (path, _) -> Set.contains path.Id succeeded) |> Seq.toArray
                return results, saved
            with error ->
                return! Error (sprintf "Failed to resolve or execute setup: %s" error.Message)
        }

    /// Parameter-only callers deliberately discard live paths before reopening saved runs.
    let executeSetupRuns minRepl maxRepl allowOverwrite cts progress host maxParallel =
        prepareRuns minRepl maxRepl allowOverwrite cts progress host maxParallel |> Async.map (Result.map fst)

    /// Reports include completed runs and never rewrite run parameters.
    let executeReportRuns minRepl maxRepl allowOverwrite cts progress host reportExecutor maxParallel =
        executeBatch false false minRepl maxRepl allowOverwrite cts progress host reportExecutor maxParallel
