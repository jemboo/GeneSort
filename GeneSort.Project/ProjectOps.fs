namespace GeneSort.Project
open System
open System.Threading
open FSharp.UMX
open GeneSort.Db
open GeneSort.Runs
open System.Threading.Tasks

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
        
            // 1. Check if already done
            if runParameters.IsRunFinished() |> Option.defaultValue false then
                return Skipped (index, repl, "already finished")
            else
                // 2. Run the executor
                let! execResult = executor db runParameters allowOverwrite cts progress
            
                match execResult with
                | Error msg -> 
                    let result = Failure (index, repl, msg)
                    ProgressMessage.reportRunResult progress result
                    return result
                | Ok updatedParams ->
                    // 3. Only save the "Finished" status if the executor actually succeeded
                    let qp = buildQueryParams updatedParams outputDataType.RunParameters
                    let! saveRes = db.saveAsync qp (updatedParams |> outputData.RunParameters) (true |> UMX.tag<allowOverwrite>)
                
                    match saveRes with
                    | Error err -> 
                        return Failure (index, repl, sprintf "Work succeeded but failed to save status: %s" err)
                    | Ok () ->
                        let result = Success (index, repl)
                        ProgressMessage.reportRunResult progress result
                        return result
        }


    let private processRunParametersSeq
            (maxDegreeOfParallelism: int)
            (runParameters: runParameters seq)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option)
            (processor: runParameters -> Async<RunResult>)
            : Async<RunResult[]> =
        async {
            try
                cts.Token.ThrowIfCancellationRequested()
                
                // Using a Semaphore with Async.Parallel is often cleaner in F# 
                // than converting everything to Tasks manually.
                use semaphore = new SemaphoreSlim(maxDegreeOfParallelism)
                
                let! results = 
                    runParameters 
                    |> Seq.map (fun rp -> 
                        async {
                            let! _ = semaphore.WaitAsync(cts.Token) |> Async.AwaitTask
                            try 
                                return! processor rp
                            finally 
                                semaphore.Release() |> ignore
                        })
                    |> Async.Parallel

                let successCount = results |> Array.filter (function Success _ -> true | _ -> false) |> Array.length
                let failureCount = results |> Array.filter (function Failure _ -> true | _ -> false) |> Array.length
                let skippedCount = results |> Array.filter (function Skipped _ -> true | _ -> false) |> Array.length
                
                report progress (sprintf "\n=== Batch Complete: %d succeeded, %d failed, %d skipped ===" successCount failureCount skippedCount)
                return results
            with
            | :? OperationCanceledException ->
                report progress "Execution cancelled by user"
                return [||]
            | e ->
                report progress (sprintf "Fatal error in batch execution: %s" e.Message)
                return [||]
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
                // Updated signature: Result<runParameters, string>
                (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<Result<runParameters, string>>)
                (runParameters: runParameters seq)
                (buildQueryParams: runParameters -> outputDataType -> queryParams)
                (allowOverwrite: bool<allowOverwrite>)
                (cts: CancellationTokenSource)
                (progress: IProgress<string> option)
                : Async<RunResult[]> =
        
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
            (progress: IProgress<string> option) : Async<Result<unit, string>> =
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
                // Updated signature: Result<runParameters, string>
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