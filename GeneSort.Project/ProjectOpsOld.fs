namespace GeneSort.Project

open System
open System.Threading

open FSharp.UMX

open GeneSort.Db
open GeneSort.Runs


module ProjectOps =  

    let inline report (progress: IProgress<string> option) msg =
        progress |> Option.iter (fun p -> p.Report msg)


    let reportRunParameters 
            (runParameters: runParameters) 
            (progress: IProgress<string> option)  : Async<RunResult> = 
        async {
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
            (yab: runParameters -> outputDataType -> queryParams)
            (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<unit>)
            (runParameters: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)  
            (progress: IProgress<string> option) : Async<RunResult> = 
        async {
            let index = runParameters.GetId().Value
            let repl = runParameters.GetRepl().Value
        
            try
                if runParameters.IsRunFinished().Value then
                    let result = Skipped (index, repl, "already finished")
                    ProgressMessage.reportRunResult progress result
                    return result
                else
                    do! executor db runParameters allowOverwrite cts progress
                
                    let queryParamsForRunParams = yab runParameters outputDataType.RunParameters
   
                    do! db.saveAsync queryParamsForRunParams (runParameters |> outputData.RunParameters) (true |> UMX.tag<allowOverwrite>)

                    let result = Success (index, repl)
                    ProgressMessage.reportRunResult progress result
                    return result
                
            with e ->
                let errorMsg = sprintf "%s: %s" (e.GetType().Name) e.Message
                let result = Failure (index, repl, errorMsg)
                ProgressMessage.reportRunResult progress result
                return result
        }


    let reportRunParametersSeq
        (maxDegreeOfParallelism: int) 
        (runParameters: runParameters seq)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        : Async<RunResult[]> =
    
        async {
            try
                cts.Token.ThrowIfCancellationRequested()
            
                let tasks =
                    runParameters
                    |> Seq.map (fun rps -> reportRunParameters rps progress)
                    |> Seq.toList

                // Execute with throttling, collecting results
                let! results = 
                    async {
                        use semaphore = new System.Threading.SemaphoreSlim(maxDegreeOfParallelism)
                        let taskResults =
                            tasks
                            |> List.map (fun comp ->
                                async {
                                    try
                                        do! Async.AwaitTask (semaphore.WaitAsync())
                                        return! comp
                                    finally
                                        semaphore.Release() |> ignore
                                }
                                |> Async.StartAsTask)
                            |> List.toArray
                    
                        return! Async.AwaitTask (System.Threading.Tasks.Task.WhenAll(taskResults))
                    }
            
                // Report summary
                let successCount = results |> Array.filter (function Success _ -> true | _ -> false) |> Array.length
                let failureCount = results |> Array.filter (function Failure _ -> true | _ -> false) |> Array.length
                let skippedCount = results |> Array.filter (function Skipped _ -> true | _ -> false) |> Array.length
            
                match progress with
                | Some p -> 
                    p.Report(sprintf "\n=== Batch Complete: %d succeeded, %d failed, %d skipped ===" 
                        successCount failureCount skippedCount)
                | None -> ()
            
                return results
            
            with 
            | :? OperationCanceledException ->
                report progress "Execution cancelled by user"
                return [||]
            | e ->
                report progress (sprintf "Fatal error in batch execution: %s" e.Message)
                return [||]
        }


    let executeRunParametersSeq
        (db: IGeneSortDb)
        (maxDegreeOfParallelism: int) 
        (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<unit>)
        (runParameters: runParameters seq)
        (yab: runParameters -> outputDataType -> queryParams)
        (allowOverwrite: bool<allowOverwrite>)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        : Async<RunResult[]> =
    
        async {
            try
                cts.Token.ThrowIfCancellationRequested()
            
                let tasks =
                    runParameters
                    |> Seq.map (fun rps -> executeRunParameters db yab executor rps allowOverwrite cts progress)
                    |> Seq.toList

                // Execute with throttling, collecting results
                let! results = 
                    async {
                        use semaphore = new System.Threading.SemaphoreSlim(maxDegreeOfParallelism)
                        let taskResults =
                            tasks
                            |> List.map (fun comp ->
                                async {
                                    try
                                        do! Async.AwaitTask (semaphore.WaitAsync())
                                        return! comp
                                    finally
                                        semaphore.Release() |> ignore
                                }
                                |> Async.StartAsTask)
                            |> List.toArray
                    
                        return! Async.AwaitTask (System.Threading.Tasks.Task.WhenAll(taskResults))
                    }
            
                // Report summary
                let successCount = results |> Array.filter (function Success _ -> true | _ -> false) |> Array.length
                let failureCount = results |> Array.filter (function Failure _ -> true | _ -> false) |> Array.length
                let skippedCount = results |> Array.filter (function Skipped _ -> true | _ -> false) |> Array.length
            
                report progress (sprintf "\n=== Batch Complete: %d succeeded, %d failed, %d skipped ===" 
                                            successCount failureCount skippedCount)
                return results
            
            with 
            | :? OperationCanceledException ->
                report progress "Execution cancelled by user"
                return [||]
            | e ->
                report progress (sprintf "Fatal error in batch execution: %s" e.Message)
                return [||]
        }


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
                report progress (sprintf "Saving RunParameter files for %s" %projectName)
            
                cts.Token.ThrowIfCancellationRequested()
            
                do! db.saveAllRunParametersAsync runParameterArray yab allowOverwrite (Some cts.Token) progress
            
                report progress (sprintf "Successfully saved %d RunParameter files" runParameterArray.Length)
                return Ok ()
            
            with 
            | :? OperationCanceledException ->
                let msg = sprintf "Saving RunParameter files was cancelled"
                report progress msg
                return Error msg
            | e ->
                let msg = sprintf "Failed to save RunParameter files for %s: %s" %projectName e.Message
                report progress msg
                return Error msg
        }



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
                report progress (sprintf "Saving project file: %s" %project.ProjectName)
            
                // Save project
                let queryParams = queryParams.createForProject project.ProjectName
                do! db.saveAsync queryParams (project |> outputData.Project) allowOverwrite
            
                let runParametersArray = Project.makeRunParameters minReplica maxReplica project.ParameterSpans paramRefiner
                report progress (sprintf "Saving run parameters files: (%d)" runParametersArray.Length)

                let! initResult = saveParametersFiles 
                                    db project.ProjectName runParametersArray yab allowOverwrite cts progress
            
                match initResult with
                | Ok () ->
                    match progress with
                    | Some p -> p.Report("Project initialization completed successfully")
                    | None -> ()
                    return Ok ()
                | Error msg ->
                    return Error (sprintf "Project initialization failed: %s" msg)
            
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
                   : Async<Result<RunResult[], string>>  =
        async {
            try
                match progress with
                | Some p -> p.Report(sprintf "Executing Runs for %s" %projectName)
                | None -> ()
            
                let! runParamsResult = db.getAllProjectRunParametersAsync projectName None progress
            
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
                        let! results = reportRunParametersSeq 8 runParametersArray cts progress
                        return Ok results
            
            with e ->
                let msg = sprintf "Fatal error executing runs: %s" e.Message
                match progress with
                | Some p -> p.Report(msg)
                | None -> ()
                return Error msg
        }




    let executeRuns
        (db: IGeneSortDb)
        (yab: runParameters -> outputDataType -> queryParams)
        (projectName: string<projectName>)
        (allowOverwrite: bool<allowOverwrite>)
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) 
        (executor: IGeneSortDb -> runParameters -> bool<allowOverwrite> -> 
                                  CancellationTokenSource -> IProgress<string> option -> Async<unit>)
                   : Async<Result<RunResult[], string>>  =
        async {
            try
                match progress with
                | Some p -> p.Report(sprintf "Executing Runs for %s" %projectName)
                | None -> ()
            
                let! runParamsResult = db.getAllProjectRunParametersAsync projectName None progress
            
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
                        let! results = executeRunParametersSeq db 8 executor runParametersArray yab allowOverwrite cts progress
                        return Ok results
            
            with e ->
                let msg = sprintf "Fatal error executing runs: %s" e.Message
                report progress (msg)
                return Error msg
        }




