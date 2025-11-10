namespace GeneSort.Project

open System
open System.Threading
open System.Threading.Tasks

open FSharp.UMX

open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

open GeneSort.Runs.Params
open GeneSort.Db
open GeneSort.Runs


module ProjectOps =  

    /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    /// Executes async computations in parallel, limited to maxDegreeOfParallelism at a time
    let private ParallelWithThrottle (maxDegreeOfParallelism: int) (computations: seq<Async<unit>>) : Async<unit> =
        async {
            use semaphore = new System.Threading.SemaphoreSlim(maxDegreeOfParallelism)
            let tasks =
                computations
                |> Seq.map (fun comp ->
                    async {
                        try
                            do! Async.AwaitTask (semaphore.WaitAsync())
                            do! comp
                        finally
                            semaphore.Release() |> ignore
                    }
                    |> Async.StartAsTask : Task<unit>)
                |> Seq.toArray
            let! _ = Async.AwaitTask (Task.WhenAll(tasks))
            return ()
        }


    let executeRunParameters
            (db: IGeneSortDb)
            (executor: IGeneSortDb -> runParameters -> CancellationTokenSource -> IProgress<string> option -> Async<unit>)
            (runParameters: runParameters) 
            (cts: CancellationTokenSource)  
            (progress: IProgress<string> option) : Async<RunResult> = 
        async {
            let index = runParameters.GetIndex().Value
            let repl = runParameters.GetRepl().Value
        
            try
                if runParameters.IsRunFinished().Value then
                    let result = Skipped (index, repl, "already finished")
                    ProgressMessage.reportRunResult progress result
                    return result
                else
                    do! executor db runParameters cts progress
                
                    let queryParamsForRunParams = 
                        queryParams.create( 
                            runParameters.GetProjectName(),
                            runParameters.GetIndex(), 
                            runParameters.GetRepl(), 
                            None, 
                            outputDataType.RunParameters)
                
                    do! db.saveAsync queryParamsForRunParams (runParameters |> outputData.RunParameters)

                    let result = Success (index, repl)
                    ProgressMessage.reportRunResult progress result
                    return result
                
            with e ->
                let errorMsg = sprintf "%s: %s" (e.GetType().Name) e.Message
                let result = Failure (index, repl, errorMsg)
                ProgressMessage.reportRunResult progress result
                return result
        }




    let executeRunParametersSeq
        (db: IGeneSortDb)
        (maxDegreeOfParallelism: int) 
        (executor: IGeneSortDb -> runParameters -> CancellationTokenSource -> IProgress<string> option -> Async<unit>)
        (runParameters: runParameters seq)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        : Async<RunResult[]> =
    
        async {
            try
                cts.Token.ThrowIfCancellationRequested()
            
                let tasks =
                    runParameters
                    |> Seq.map (fun rps -> executeRunParameters db executor rps cts progress)
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
                match progress with
                | Some p -> p.Report("Execution cancelled by user")
                | None -> ()
                return [||]
            | e ->
                match progress with
                | Some p -> p.Report(sprintf "Fatal error in batch execution: %s" e.Message)
                | None -> ()
                return [||]
        }



    let initParametersFiles 
            (db: IGeneSortDb)
            (projectName: string<projectName>)
            (runParameterArray: runParameters[]) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<unit, string>> =
        async {
            try
                match progress with
                | Some p -> p.Report(sprintf "Saving RunParameter files for %s" %projectName)
                | None -> ()
            
                cts.Token.ThrowIfCancellationRequested()
            
                do! db.saveAllRunParametersAsync runParameterArray (Some cts.Token) progress
            
                match progress with
                | Some p -> p.Report(sprintf "Successfully saved %d RunParameter files" runParameterArray.Length)
                | None -> ()
            
                return Ok ()
            
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



    let initProjectFiles
        (db: IGeneSortDb)
        (project: project)
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<unit, string>> =
        async {
            try
                match progress with
                | Some p -> p.Report(sprintf "Saving project file: %s" %project.ProjectName)
                | None -> ()
            
                let queryParams = queryParams.createForProject project.ProjectName
                do! db.saveAsync queryParams (project |> outputData.Project)
            
                match progress with
                | Some p -> p.Report(sprintf "Saving run parameters files: (%d)" project.RunParametersArray.Length)
                | None -> ()
            
                let! initResult = initParametersFiles db project.ProjectName project.RunParametersArray cts progress
            
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
                match progress with
                | Some p -> p.Report(errorMsg)
                | None -> ()
                return Error errorMsg
        }



    let executeRuns
        (db: IGeneSortDb)
        (projectName: string<projectName>)
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) 
        (executor: IGeneSortDb -> runParameters -> CancellationTokenSource -> IProgress<string> option -> Async<unit>)
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
                        let! results = executeRunParametersSeq db 8 executor runParametersArray cts progress
                        return Ok results
            
            with e ->
                let msg = sprintf "Fatal error executing runs: %s" e.Message
                match progress with
                | Some p -> p.Report(msg)
                | None -> ()
                return Error msg
        }

