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

module ProjectOps =

    /// Internal logic for a single unit of work
    let private runTask skipFinished persistParameters (db: IGeneSortDb)
                        (runName: string<runName>)
                        (buildQueryParams: runParameters -> geneSortDataType -> queryParams option)
                        executor 
                        allowOverwrite 
                        cts 
                        progress 
                        (rp: runParameters) = 
        async {
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
                    let! execResult = executor rp allowOverwrite cts progress
                
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
                                            executor allowOverwrite cts progress)
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

    /// Local preparation only. Persist all Setup artifacts before transferring Project and Setup.
    let executeSetupRuns minRepl maxRepl allowOverwrite cts progress (host: runHost) maxParallel =
        asyncResult {
            try
                let setupExecutor = host.SetupExecutor
                return! executeBatch true true minRepl maxRepl allowOverwrite cts progress host setupExecutor maxParallel
            with error ->
                return! Error (sprintf "Failed to resolve or execute setup: %s" error.Message)
        }

    /// Reports include completed runs and never rewrite run parameters.
    let executeReportRuns minRepl maxRepl allowOverwrite cts progress host reportExecutor maxParallel =
        executeBatch false false minRepl maxRepl allowOverwrite cts progress host reportExecutor maxParallel
