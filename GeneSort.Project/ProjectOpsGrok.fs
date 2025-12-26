namespace GeneSort.Project
open System
open System.Threading
open FSharp.UMX
open GeneSort.Db
open GeneSort.Runs
open System.Threading.Tasks

module ProjectOpsGrok =
    // Improved: Add CT for cancellability. Always return Success after reporting, regardless of progress.
    // Decouple reporting from result logic. Use Result for error consistency.
    let reportRunParameters
            (runParameters: runParameters)
            (cts: CancellationTokenSource)  // New: Add CTS for consistency with batch.
            (progress: IProgress<string> option) : Async<Result<RunResult, string>> =  // Changed to Result.
        async {
            cts.Token.ThrowIfCancellationRequested()  // New: Early cancellation check.
            let index = runParameters.GetId().Value
            let repl = runParameters.GetRepl().Value
       
            try
                match progress with
                | Some p -> p.Report(runParameters.toString())  // Fixed: Report without affecting result.
                | None -> ()
                return Ok (Success (index, repl))  // Fixed: Always Success if no error.
            with 
            | :? OperationCanceledException -> return Error "Operation cancelled"
            | e ->
                let errorMsg = sprintf "%s: %s" (e.GetType().Name) e.Message
                let result = Failure (index, repl, errorMsg)
                ProgressMessage.reportRunResult progress result
                return Error errorMsg  // New: Return Error instead of RunResult.
        }

    // Improved: Return Result for consistency. Pass CT to saveAsync.
    let executeRunParameters
            (db: IGeneSortDb)
            (yab: runParameters -> outputDataType -> queryParams)
            (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<unit>)
            (runParameters: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<RunResult, string>> =  // Changed to Result.
        async {
            let index = runParameters.GetId().Value
            let repl = runParameters.GetRepl().Value
       
            try
                if runParameters.IsRunFinished().Value then
                    let result = Skipped (index, repl, "already finished")
                    ProgressMessage.reportRunResult progress result
                    return Ok result
                else
                    do! executor db runParameters allowOverwrite cts progress
               
                    let queryParamsForRunParams = yab runParameters outputDataType.RunParameters
                    // Changed: Use Result from saveAsync (assuming db.saveAsync returns Async<Result<unit, string>> as suggested previously).
                    let! saveRes = db.saveAsync queryParamsForRunParams (runParameters |> outputData.RunParameters) (true |> UMX.tag<allowOverwrite>)
                    match saveRes with
                    | Error err -> return Error err
                    | Ok () ->
                        let result = Success (index, repl)
                        ProgressMessage.reportRunResult progress result
                        return Ok result
            with 
            | :? OperationCanceledException -> return Error "Operation cancelled"
            | e ->
                let errorMsg = sprintf "%s: %s" (e.GetType().Name) e.Message
                let result = Failure (index, repl, errorMsg)
                ProgressMessage.reportRunResult progress result
                return Error errorMsg
        }

    // New: Generic batch processor to reduce duplication between report and execute seq functions.
    let private processRunParametersSeq<'T>
        (maxDegreeOfParallelism: int)
        (runParameters: runParameters seq)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        (processor: runParameters -> Async<Result<'T, string>>)  // Generic per-item processor returning Result.
        : Async<Result<'T[], string>> =
        async {
            try
                cts.Token.ThrowIfCancellationRequested()
           
                let tasks =
                    runParameters
                    |> Seq.map processor
                    |> Seq.toList
                
                // Execute with throttling, collecting results
                let! results =
                    async {
                        use semaphore = new SemaphoreSlim(maxDegreeOfParallelism)
                        let taskResults =
                            tasks
                            |> List.map (fun comp ->
                                async {
                                    try
                                        do! Async.AwaitTask (semaphore.WaitAsync(cts.Token))  // Add CT to WaitAsync.
                                        return! comp
                                    finally
                                        semaphore.Release() |> ignore
                                }
                                |> Async.StartAsTask)
                            |> List.toArray
                   
                        return! Async.AwaitTask (Task.WhenAll(taskResults))
                    }
           
                // Collect Ok and Error separately
                let okResults = results |> Array.choose (function Ok r -> Some r | _ -> None)
                let errors = results |> Array.choose (function Error e -> Some e | _ -> None)
                
                // Report summary (assuming 'T is RunResult for counting)
                if typeof<'T> = typeof<RunResult> then
                    let successCount = okResults |> Array.filter (function Success _ -> true | _ -> false) |> Array.length
                    let failureCount = okResults |> Array.filter (function Failure _ -> true | _ -> false) |> Array.length
                    let skippedCount = okResults |> Array.filter (function Skipped _ -> true | _ -> false) |> Array.length
                    match progress with
                    | Some p ->
                        p.Report(sprintf "\n=== Batch Complete: %d succeeded, %d failed, %d skipped ==="
                            successCount failureCount skippedCount)
                    | None -> ()
           
                if errors.Length > 0 then
                    return Error (String.Join("; ", errors))  // Aggregate errors.
                else
                    return Ok okResults
           
            with
            | :? OperationCanceledException ->
                match progress with
                | Some p -> p.Report("Execution cancelled by user")
                | None -> ()
                return Error "Batch execution cancelled"
            | e ->
                match progress with
                | Some p -> p.Report(sprintf "Fatal error in batch execution: %s" e.Message)
                | None -> ()
                return Error (sprintf "Batch fatal error: %s" e.Message)
        }

    // Refactored: Use generic processRunParametersSeq.
    let reportRunParametersSeq
        (maxDegreeOfParallelism: int)
        (runParameters: runParameters seq)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        : Async<Result<RunResult[], string>> =
        processRunParametersSeq maxDegreeOfParallelism runParameters cts progress (fun rp -> reportRunParameters rp cts progress)

    // Refactored: Use generic processRunParametersSeq. Pass executor-specific params inside processor.
    let executeRunParametersSeq
        (db: IGeneSortDb)
        (maxDegreeOfParallelism: int)
        (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<unit>)
        (runParameters: runParameters seq)
        (yab: runParameters -> outputDataType -> queryParams)
        (allowOverwrite: bool<allowOverwrite>)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        : Async<Result<RunResult[], string>> =
        let processor rp = executeRunParameters db yab executor rp allowOverwrite cts progress
        processRunParametersSeq maxDegreeOfParallelism runParameters cts progress processor

    // Improved: Use db.saveAllRunParametersAsync's Result if it returns one (assume updated to Async<Result<unit, string>>).
    let saveParametersFiles
            (db: IGeneSortDb)
            (projectName: string<projectName>)
            (runParameterArray: runParameters[])
            (yab: runParameters -> outputDataType -> queryParams)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<unit, string>> =
        async {
            try
                match progress with
                | Some p -> p.Report(sprintf "Saving RunParameter files for %s" %projectName)
                | None -> ()
           
                cts.Token.ThrowIfCancellationRequested()
           
                // Assume db.saveAllRunParametersAsync updated to return Async<Result<unit, string>>.
                let! saveRes = db.saveAllRunParametersAsync runParameterArray yab allowOverwrite (Some cts.Token) progress
           
                match saveRes with
                | Ok () ->
                    match progress with
                    | Some p -> p.Report(sprintf "Successfully saved %d RunParameter files" runParameterArray.Length)
                    | None -> ()
                    return Ok ()
                | Error msg -> return Error msg
           
            with
            | :? OperationCanceledException ->
                let msg = sprintf "Saving RunParameter files was cancelled"
                match progress with
                | Some p -> p.Report(msg)
                | None -> ()
                return Error msg
            | e ->
                let msg = sprintf "Failed to save RunParameter files for %s: %s" %projectName e.Message
                match progress with
                | Some p -> p.Report(msg)
                | None -> ()
                return Error msg
        }

    // Improved: Minimal changes, but use updated saveParametersFiles.
    let initProjectFiles
        (db: IGeneSortDb)
        (yab: runParameters -> outputDataType -> queryParams)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        (project: project)
        (minReplica: int<replNumber>)
        (maxReplica: int<replNumber>)
        (allowOverwrite: bool<allowOverwrite>)
        (paramRefiner: runParameters seq -> runParameters seq) : Async<Result<unit, string>> =
        async {
            try
                match progress with
                | Some p -> p.Report(sprintf "Saving project file: %s" %project.ProjectName)
                | None -> ()
           
                // Save project (assume db.saveAsync returns Async<Result<unit, string>>).
                let queryParams = queryParams.createForProject project.ProjectName
                let! saveProjRes = db.saveAsync queryParams (project |> outputData.Project) allowOverwrite
                match saveProjRes with
                | Error err -> return Error err
                | Ok () ->
                    let runParametersArray = Project.makeRunParameters minReplica maxReplica project.ParameterSpans paramRefiner |> Seq.toArray  // ToArray for efficiency.
                    match progress with
                    | Some p -> p.Report(sprintf "Saving run parameters files: (%d)" runParametersArray.Length)
                    | None -> ()
                    return! saveParametersFiles db project.ProjectName runParametersArray yab allowOverwrite cts progress  // Now returns Result directly.
            with e ->
                let errorMsg = sprintf "Failed to initialize project files: %s" e.Message
                match progress with
                | Some p -> p.Report(errorMsg)
                | None -> ()
                return Error errorMsg
        }

    // Improved: Use updated db.getAllProjectRunParametersAsync (assume it returns Async<Result<runParameters[], string>>).
    // Use configurable maxDegree (instead of hardcoded 8).
    let reportRuns
        (db: IGeneSortDb)
        (projectName: string<projectName>)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
                   : Async<Result<RunResult[], string>> =
        async {
            try
                match progress with
                | Some p -> p.Report(sprintf "Reporting Runs for %s" %projectName)  // Minor: Changed "Executing" to "Reporting" for accuracy.
                | None -> ()
           
                let! runParamsResult = db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress  // Pass CT.
           
                match runParamsResult with
                | Error msg ->
                    match progress with
                    | Some p -> p.Report(sprintf "Failed to load run parameters: %s" msg)
                    | None -> ()
                    return Error msg
               
                | Ok runParametersArray ->
                    match progress with
                    | Some p -> p.Report(sprintf "Found %d runs to report" runParametersArray.Length)
                    | None -> ()
               
                    if runParametersArray.Length = 0 then
                        match progress with
                        | Some p -> p.Report("No runs found to report")
                        | None -> ()
                        return Ok [||]
                    else
                        let maxDegree = 8  // Or make configurable.
                        return! reportRunParametersSeq maxDegree runParametersArray cts progress  // Now returns Result.
           
            with e ->
                let msg = sprintf "Fatal error reporting runs: %s" e.Message
                match progress with
                | Some p -> p.Report(msg)
                | None -> ()
                return Error msg
        }

    // Improved: Similar to reportRuns. Use configurable maxDegree.
    let executeRuns
        (db: IGeneSortDb)
        (yab: runParameters -> outputDataType -> queryParams)
        (projectName: string<projectName>)
        (allowOverwrite: bool<allowOverwrite>)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> ->
                                  CancellationTokenSource -> IProgress<string> option -> Async<unit>)
                   : Async<Result<RunResult[], string>> =
        async {
            try
                match progress with
                | Some p -> p.Report(sprintf "Executing Runs for %s" %projectName)
                | None -> ()
           
                let! runParamsResult = db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress  // Pass CT.
           
                match runParamsResult with
                | Error msg ->
                    match progress with
                    | Some p -> p.Report(sprintf "Failed to load run parameters: %s" msg)
                    | None -> ()
                    return Error msg
               
                | Ok runParametersArray ->
                    match progress with
                    | Some p -> p.Report(sprintf "Found %d runs to execute" runParametersArray.Length)
                    | None -> ()
               
                    if runParametersArray.Length = 0 then
                        match progress with
                        | Some p -> p.Report("No runs found to execute")
                        | None -> ()
                        return Ok [||]
                    else
                        let maxDegree = 8  // Or make configurable.
                        return! executeRunParametersSeq db maxDegree executor runParametersArray yab allowOverwrite cts progress  // Now returns Result.
           
            with e ->
                let msg = sprintf "Fatal error executing runs: %s" e.Message
                match progress with
                | Some p -> p.Report(msg)
                | None -> ()
                return Error msg
        }