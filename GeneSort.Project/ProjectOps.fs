namespace GeneSort.Project
open System
open System.Threading
open FSharp.UMX
open GeneSort.Db
open GeneSort.Runs
open GeneSort.Core

module ProjectOps =

    let inline report (progress: IProgress<string> option) msg =
        progress |> Option.iter (fun p -> p.Report msg)

    let reportRunParameters
            (runParameters: runParameters)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<RunResult> =
        async {
            cts.Token.ThrowIfCancellationRequested()
            let id = runParameters.GetId().Value
            let repl = runParameters.GetRepl().Value
            try
                report progress (runParameters |> RunParameters.reportKvps)
                return Success (id, repl, "")
            with e ->
                let msg = $"{e.GetType().Name}: {e.Message}"
                let result = Failure (id, repl, msg)
                ProgressMessage.reportRunResult progress result
                return result
        }


    let executeRunParameters
            (db: IGeneSortDb)
            (projectFolder: string<projectFolder>)
            (buildQueryParams: runParameters -> outputDataType -> queryParams) 
            (executor: IGeneSortDb ->  string<projectFolder> -> runParameters -> bool<allowOverwrite> ->
                       CancellationTokenSource -> IProgress<string> option 
                        -> Async<Result<runParameters, string>>)
            (runParameters: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<RunResult> =
        async {
            let runId = runParameters.GetId() |> Option.defaultValue ( Guid.Empty |> UMX.tag<idValue>)
            let repl = runParameters.GetRepl() |> Option.defaultValue (-1 |> UMX.tag)
    
            // 1. Check if already done (Synchronous check remains the same)
            if runParameters.IsRunFinished() |> Option.defaultValue false then
                return Skipped (runId, repl, sprintf "already finished: %s" (runParameters |> RunParameters.reportKvps))
            else
                report progress (sprintf "%s Run %s Repl %d started %s" (MathUtils.getTimestampString()) (%runId.ToString()) %repl (runParameters |> RunParameters.reportKvps))
                // 2. Use the builder to handle the execution + status save sequence
                let! finalResult = asyncResult {
                    // Execute the main work
                    let! updatedParams = executor db projectFolder runParameters allowOverwrite cts progress
                
                    // If main work succeeded, save the "Finished" status
                    let qp = buildQueryParams updatedParams outputDataType.RunParameters
                    let! _ = 
                        db.saveAsync projectFolder qp (updatedParams |> outputData.RunParameters) (true |> UMX.tag<allowOverwrite>)
                        |> AsyncResult.mapError (fun err -> sprintf "Work succeeded but failed to save status: %s" err)

                    return updatedParams
                }

                // 3. Map the AsyncResult back to the RunResult DU
                match finalResult with
                | Ok newRunParams ->
                    let runRes = Success (runId, repl, (newRunParams |> RunParameters.reportKvps))
                    ProgressMessage.reportRunResult progress runRes
                    return runRes
                | Error msg ->
                    let msgFail = sprintf "%s %s" msg (runParameters |> RunParameters.reportKvps)
                    let runRes = Failure (runId, repl, msgFail)
                    ProgressMessage.reportRunResult progress runRes
                    return runRes
        }


    let private processRunParametersSeq
            (maxDegreeOfParallelism: int)
            (runParameters: runParameters seq)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option)
            (processor: runParameters -> Async<RunResult>)
            : Async<RunResult[]> =
        async {
            use semaphore = new SemaphoreSlim(maxDegreeOfParallelism)
        
            let processInternal (rp: runParameters) = async {
                let id = rp.GetId() |> Option.defaultValue ( Guid.Empty |> UMX.tag<idValue>)
                let repl = (rp.GetRepl() |> Option.defaultValue (-1 |> UMX.tag))
            
                try 
                    // 1. Wait for slot
                    let! _ = semaphore.WaitAsync(cts.Token) |> Async.AwaitTask
                    try 
                        // 2. Execute work
                        return! processor rp
                    finally 
                        // 3. Always release the slot
                        semaphore.Release() |> ignore
                with 
                | :? OperationCanceledException ->
                    return Cancelled (id, repl)
                | e ->
                    return Failure (id, repl, sprintf "Inner fault: %s" e.Message)
            }

            // Run all in parallel (throttled by semaphore)
            let! results = 
                runParameters 
                |> Seq.map processInternal
                |> Async.Parallel

            // Tally results using the new Cancelled case
            let s, f, sk, c = 
                results |> Array.fold (fun (s, f, sk, c) res ->
                    match res with
                    | Success _   -> (s + 1, f, sk, c)
                    | Failure _   -> (s, f + 1, sk, c)
                    | Skipped _   -> (s, f, sk + 1, c)
                    | Cancelled _ -> (s, f, sk, c + 1)
                ) (0, 0, 0, 0)

            // Final summary report
            let summary = 
                sprintf "\n=== Batch Complete ===\nSucceeded: %d\nFailed:    %d\nSkipped:   %d\nCancelled: %d" 
                    s f sk c
            report progress summary
            
            return results
        }


    let reportRunParametersSeq
        (maxDegreeOfParallelism: int)
        (runParameters: runParameters seq)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        : Async<RunResult[]> =
        processRunParametersSeq 
                    maxDegreeOfParallelism 
                    runParameters cts progress 
                    (fun rp -> reportRunParameters rp cts progress)


    let executeRunParametersSeq
                (db: IGeneSortDb)
                (projectFolder: string<projectFolder>)
                (maxDegreeOfParallelism: int)
                (executor: IGeneSortDb ->  string<projectFolder> -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<Result<runParameters, string>>)
                (runParameters: runParameters seq)
                (buildQueryParams: runParameters -> outputDataType -> queryParams)
                (allowOverwrite: bool<allowOverwrite>)
                (cts: CancellationTokenSource)
                (progress: IProgress<string> option)    : Async<RunResult[]> =
        
            processRunParametersSeq 
                maxDegreeOfParallelism 
                runParameters cts progress 
                (fun rp -> executeRunParameters 
                                db projectFolder 
                                buildQueryParams executor 
                                rp allowOverwrite cts progress)


    let saveParametersFiles
            (db: IGeneSortDb)
            (projectFolder: string<projectFolder>)
            (runParameterArray: runParameters[])
            (buildQueryParams: runParameters -> outputDataType -> queryParams)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option)          : Async<Result<unit, string>> =
        async {
            try
                report progress (sprintf "%s Saving RunParameter files in %s" (MathUtils.getTimestampString()) %projectFolder)
                cts.Token.ThrowIfCancellationRequested()
                let! res = db.saveAllRunParametersAsync projectFolder runParameterArray buildQueryParams allowOverwrite (Some cts.Token) progress
                match res with
                | Ok () -> 
                    report progress (sprintf "%s Successfully saved %d RunParameter files" (MathUtils.getTimestampString()) runParameterArray.Length)
                    return Ok ()
                | Error msg -> return Error msg
            with
            | :? OperationCanceledException ->
                let msg = "Saving RunParameter files was cancelled"
                report progress msg
                return Error msg
            | e ->
                let msg = sprintf "%s Failed to save RunParameter files in %s: %s" (MathUtils.getTimestampString()) %projectFolder e.Message
                report progress msg
                return Error msg
        }


    let initProjectFiles
        (db: IGeneSortDb)
        (projectFolder: string<projectFolder>)
        (buildQueryParams: runParameters -> outputDataType -> queryParams)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        (project: project)
        (minReplica: int<replNumber>)
        (maxReplica: int<replNumber>)
        (allowOverwrite: bool<allowOverwrite>)
        (paramRefiner: runParameters seq -> runParameters seq) : Async<Result<unit, string>> =
        async {
            try
                report progress (sprintf "%s Saving project file: %s" (MathUtils.getTimestampString()) %project.ProjectName)
                let queryParams = queryParams.createForProject project.ProjectName
                let! saveProjRes = db.saveAsync projectFolder queryParams (project |> outputData.Project) (true |> UMX.tag<allowOverwrite>)
                match saveProjRes with
                | Error err -> return Error err
                | Ok () ->
                    let runParametersArray = Project.makeRunParameters minReplica maxReplica project.ParameterSpans paramRefiner |> Seq.toArray
                    report progress (sprintf "%s Saving run parameters files: (%d)" (MathUtils.getTimestampString()) runParametersArray.Length)
                    return! saveParametersFiles 
                                db projectFolder
                                runParametersArray buildQueryParams 
                                allowOverwrite cts progress
            with e ->
                let errorMsg = sprintf "Failed to initialize project files: %s" e.Message
                report progress errorMsg
                return Error errorMsg
        }


    let printRunParams
            (db: IGeneSortDb)
            (projectFolder: string<projectFolder>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option)
                               : Async<Result<RunResult[], string>> =
        asyncResult {
            try
                report progress (sprintf "Reporting Runs from %s" %projectFolder)

                // 1. Load Parameters (Auto-handles Error short-circuit)
                let! runParametersArray = 
                    db.getAllProjectRunParametersAsync projectFolder (Some cts.Token) progress

                report progress (sprintf "Found %d runs to report" runParametersArray.Length)

                if runParametersArray.Length = 0 then
                    report progress "No runs found to report"
                    return [||]
                else
                let maxDegreeOfParallelism = 1
                let! results = 
                    reportRunParametersSeq maxDegreeOfParallelism runParametersArray cts progress
                    |> Async.map Ok

                let summary = RunResult.analyze results
                
                report progress (sprintf "--- Report Summary ---")
                report progress (sprintf "Successfully verified: %d/%d" summary.Successes summary.Total)
                
                if summary.MissingLog.Length > 0 then
                    report progress (sprintf "\nFound %d missing runs:" summary.MissingLog.Length)
                    summary.MissingLog |> Array.iter (report progress)

                if summary.IssueLog.Length > 0 then
                    report progress (sprintf "\nFound %d issues/crashes:" summary.IssueLog.Length)
                    summary.IssueLog |> Array.iter (report progress)

                return results

            with e ->
                let msg = sprintf "Fatal error reporting runs: %s" e.Message
                report progress msg
                return! async { return Error msg }
        }


    let executeRuns
            (db: IGeneSortDb)
            (projectFolder: string<projectFolder>)
            (buildQueryParams: runParameters -> outputDataType -> queryParams)
            (projectName: string<projectName>)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option)
            (executor: IGeneSortDb -> string<projectFolder> -> runParameters ->
                       bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option
                            -> Async<Result<runParameters, string>>)
            (maxDegreeOfParallelism: int)
            : Async<Result<RunResult[], string>> =
    
        asyncResult {
            try
                report progress (sprintf "%s Executing Runs for %s" (MathUtils.getTimestampString()) %projectName)

                // 1. Load Parameters
                let! runParametersArray = 
                    db.getAllProjectRunParametersAsync projectFolder (Some cts.Token) progress

                report progress (sprintf "Found %d runs to execute" runParametersArray.Length)

                if runParametersArray.Length = 0 then
                    report progress "No runs found to execute"
                    return [||]
                else
                    // 1. Run the sequence (returns Async<RunResult[]>)
                    let! results = 
                        executeRunParametersSeq 
                            db projectFolder maxDegreeOfParallelism executor 
                            runParametersArray buildQueryParams allowOverwrite cts progress
                        |> Async.map Ok // Wrap the naked array in a Result.Ok

                    return results

            with e ->
                // Consistent with your executors, handle the unexpected
                let msg = sprintf "%s Fatal error executing runs: %s" (MathUtils.getTimestampString()) e.Message
                report progress msg
                return! async { return Error msg }
        }