namespace GeneSort.Project

open System.Threading.Tasks
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System
open System.Threading
open GeneSort.Runs.Params
open GeneSort.Db
open ProgressMessage


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
                    reportRunResult progress result
                    return result
                else
                    do! executor db runParameters cts progress
                
                    let queryParamsForRunParams = 
                        queryParams.Create( 
                            runParameters.GetProjectName().Value, 
                            runParameters.GetIndex(), 
                            runParameters.GetRepl(), 
                            None, 
                            outputDataType.RunParameters)
                
                    do! db.saveAsync queryParamsForRunParams (runParameters |> outputData.RunParameters)
                
                    let result = Success (index, repl)
                    reportRunResult progress result
                    return result
                
            with e ->
                let errorMsg = sprintf "%s: %s" (e.GetType().Name) e.Message
                let result = Failure (index, repl, errorMsg)
                reportRunResult progress result
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