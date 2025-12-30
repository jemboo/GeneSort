namespace GeneSort.Project
open System
open System.Threading
open FSharp.UMX
open GeneSort.Db
open GeneSort.Runs
open System.Threading.Tasks
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
            let index = runParameters.GetId().Value
            let repl = runParameters.GetRepl().Value
            try
                report progress (runParameters.toString())
                return Success (index, repl)
            with e ->
                let msg = $"{e.GetType().Name}: {e.Message}"
                let result = Failure (index, repl, msg)
                ProgressMessage.reportRunResult progress result
                return result
        }


    let executeRunParameters
            (db: IGeneSortDb)
            (buildQueryParams: runParameters -> outputDataType -> queryParams) 
            (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option 
                                    -> Async<Result<runParameters, string>>)
            (runParameters: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<RunResult> =
        async {
            let index = runParameters.GetId() |> Option.defaultValue (% "unknown")
            let repl = runParameters.GetRepl() |> Option.defaultValue (0 |> UMX.tag)
        
            // 1. Check if already done (Synchronous check remains the same)
            if runParameters.IsRunFinished() |> Option.defaultValue false then
                return Skipped (index, repl, "already finished")
            else
                // 2. Use the builder to handle the execution + status save sequence
                let! finalResult = asyncResult {
                    // Execute the main work
                    let! updatedParams = executor db runParameters allowOverwrite cts progress
                
                    // If main work succeeded, save the "Finished" status
                    let qp = buildQueryParams updatedParams outputDataType.RunParameters
                    let! _ = 
                        db.saveAsync qp (updatedParams |> outputData.RunParameters) (true |> UMX.tag<allowOverwrite>)
                        |> AsyncResult.mapError (fun err -> sprintf "Work succeeded but failed to save status: %s" err)

                    return updatedParams
                }

                // 3. Map the AsyncResult back to the RunResult DU
                match finalResult with
                | Ok _ ->
                    let res = Success (index, repl)
                    ProgressMessage.reportRunResult progress res
                    return res
                | Error msg ->
                    let res = Failure (index, repl, msg)
                    ProgressMessage.reportRunResult progress res
                    return res
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
                let index = rp.GetId() |> Option.defaultValue (% "unknown")
                let repl = rp.GetRepl() |> Option.defaultValue (0 |> UMX.tag)
            
                try 
                    // Wait for slot, but respect cancellation
                    let! _ = semaphore.WaitAsync(cts.Token) |> Async.AwaitTask
                    try 
                        return! processor rp
                    finally 
                        semaphore.Release() |> ignore
                with 
                | :? OperationCanceledException ->
                    // Return a specific result instead of letting the exception bubble up
                    return Skipped (index, repl, "Cancelled by user")
                | e ->
                    // Handle unexpected errors within a single task so others can continue
                    return Failure (index, repl, sprintf "Inner fault: %s" e.Message)
            }

            // 1. Run in parallel - this will no longer throw on cancellation
            let! results = 
                runParameters 
                |> Seq.map processInternal
                |> Async.Parallel

            // 2. Tally results
            let success, failure, skipped = 
                results |> Array.fold (fun (s, f, sk) res ->
                    match res with
                    | Success _ -> (s + 1, f, sk)
                    | Failure _ -> (s, f + 1, sk)
                    | Skipped _ -> (s, f, sk + 1)
                ) (0, 0, 0)

            // 3. Final reporting
            if cts.IsCancellationRequested then
                report progress (sprintf "\n=== Batch Halted: %d finished, %d cancelled/skipped ===" success (failure + skipped))
            else
                report progress (sprintf "\n=== Batch Complete: %d succeeded, %d failed, %d skipped ===" success failure skipped)
            
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
                (maxDegreeOfParallelism: int)
                (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<Result<runParameters, string>>)
                (runParameters: runParameters seq)
                (buildQueryParams: runParameters -> outputDataType -> queryParams)
                (allowOverwrite: bool<allowOverwrite>)
                (cts: CancellationTokenSource)
                (progress: IProgress<string> option)    : Async<RunResult[]> =
        
            processRunParametersSeq 
                maxDegreeOfParallelism 
                runParameters cts progress 
                (fun rp -> executeRunParameters db buildQueryParams executor rp allowOverwrite cts progress)


    let saveParametersFiles
            (db: IGeneSortDb)
            (projectName: string<projectName>)
            (runParameterArray: runParameters[])
            (buildQueryParams: runParameters -> outputDataType -> queryParams)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option)          : Async<Result<unit, string>> =
        async {
            try
                report progress (sprintf "Saving RunParameter files for %s" %projectName)
                cts.Token.ThrowIfCancellationRequested()
                let! res = db.saveAllRunParametersAsync runParameterArray buildQueryParams allowOverwrite (Some cts.Token) progress
                match res with
                | Ok () -> 
                    report progress (sprintf "Successfully saved %d RunParameter files" runParameterArray.Length)
                    return Ok ()
                | Error msg -> return Error msg
            with
            | :? OperationCanceledException ->
                let msg = "Saving RunParameter files was cancelled"
                report progress msg
                return Error msg
            | e ->
                let msg = sprintf "Failed to save RunParameter files for %s: %s" %projectName e.Message
                report progress msg
                return Error msg
        }


    let initProjectFiles
        (db: IGeneSortDb)
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
                report progress (sprintf "Saving project file: %s" %project.ProjectName)
                let queryParams = queryParams.createForProject project.ProjectName
                let! saveProjRes = db.saveAsync queryParams (project |> outputData.Project) (true |> UMX.tag<allowOverwrite>)
                match saveProjRes with
                | Error err -> return Error err
                | Ok () ->
                    let runParametersArray = Project.makeRunParameters minReplica maxReplica project.ParameterSpans paramRefiner |> Seq.toArray
                    report progress (sprintf "Saving run parameters files: (%d)" runParametersArray.Length)
                    return! saveParametersFiles db project.ProjectName runParametersArray buildQueryParams allowOverwrite cts progress
            with e ->
                let errorMsg = sprintf "Failed to initialize project files: %s" e.Message
                report progress errorMsg
                return Error errorMsg
        }

    let reportRuns
        (db: IGeneSortDb)
        (projectName: string<projectName>)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
                   : Async<Result<RunResult[], string>> =
        async {
            try
                report progress (sprintf "Reporting Runs for %s" %projectName)
                let! runParamsResult = db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress
                match runParamsResult with
                | Error msg ->
                    report progress (sprintf "Failed to load run parameters: %s" msg)
                    return Error msg
                | Ok runParametersArray ->
                    report progress (sprintf "Found %d runs to report" runParametersArray.Length)
                    if runParametersArray.Length = 0 then
                        report progress "No runs found to report"
                        return Ok [||]
                    else
                        let maxDegree = 8
                        let! results = reportRunParametersSeq maxDegree runParametersArray cts progress
                        return Ok results
            with e ->
                let msg = sprintf "Fatal error reporting runs: %s" e.Message
                report progress msg
                return Error msg
        }


    let executeRuns
                (db: IGeneSortDb)
                (buildQueryParams: runParameters -> outputDataType -> queryParams)
                (projectName: string<projectName>)
                (allowOverwrite: bool<allowOverwrite>)
                (cts: CancellationTokenSource)
                (progress: IProgress<string> option)
                (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option
                                -> Async<Result<runParameters, string>>)
                (maxDegreeOfParallelism: int)
                : Async<Result<RunResult[], string>> =
            async {
                try
                    report progress (sprintf "Executing Runs for %s" %projectName)
                    let! runParamsResult = db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress
                
                    match runParamsResult with
                    | Error msg ->
                        report progress (sprintf "Failed to load run parameters: %s" msg)
                        return Error msg
                    | Ok runParametersArray ->
                        report progress (sprintf "Found %d runs to execute" runParametersArray.Length)
                        if runParametersArray.Length = 0 then
                            report progress "No runs found to execute"
                            return Ok [||]
                        else
                            // This call now passes the Result-based executor down the chain
                            let! results = 
                                executeRunParametersSeq 
                                    db maxDegreeOfParallelism executor 
                                    runParametersArray buildQueryParams allowOverwrite cts progress
                            return Ok results
                with e ->
                    let msg = sprintf "Fatal error executing runs: %s" e.Message
                    report progress msg
                    return Error msg
            }