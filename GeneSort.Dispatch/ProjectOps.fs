namespace GeneSort.Dispatch.V1
open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open OpsUtils
open GeneSort.Project.V1
open GeneSort.Db.V1

module ProjectOps =


    let executeRunParameters
            (db: IGeneSortDb)
            (buildQueryParams: runParameters -> outputDataType -> queryParams) 
            (executor: runParameters -> bool<allowOverwrite> ->
                       CancellationTokenSource -> IProgress<string> option 
                        -> Async<Result<runParameters, string>>)
            (runParameters: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<RunResult> =
        async {
            let runId = runParameters.GetId() |> Option.defaultValue ( Guid.Empty |> UMX.tag<queryParamsId>)
            let repl = runParameters.GetRepl() |> Option.defaultValue (-1 |> UMX.tag)
    
            // 1. Check if already done (Synchronous check remains the same)
            if runParameters.GetRunFinished() |> Option.defaultValue false then
                let runRes = Skipped (runId, repl, sprintf "\n%s" (sprintf "%s\n" (runParameters |> RunParameters.reportKvps)) )
                ProgressMessage.reportRunResult progress runRes
                return runRes
            else
                report progress (sprintf "%s Run %s Repl %d started %s" (MathUtils.getTimestampString()) (%runId.ToString()) %repl (sprintf "%s\n" (runParameters |> RunParameters.reportKvps)))
                // 2. Use the builder to handle the execution + status save sequence
                let! finalResult = asyncResult {
                    // Execute the main work
                    let! updatedParams = executor runParameters allowOverwrite cts progress
                
                    // If main work succeeded, save the "Finished" status
                    let qp = buildQueryParams updatedParams outputDataType.RunParameters
                    let! _ = 
                        db.saveAsync qp (updatedParams |> outputData.RunParameters) (true |> UMX.tag<allowOverwrite>)
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
                    let msgFail = sprintf "%s\n%s" msg (runParameters |> RunParameters.reportKvps)
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
                let id = rp.GetId() |> Option.defaultValue ( Guid.Empty |> UMX.tag<queryParamsId>)
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


    let executeRunParametersSeq
                (db: IGeneSortDb)
                (maxDegreeOfParallelism: int)
                (executor: runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<Result<runParameters, string>>)
                (runParameters: runParameters seq)
                (buildQueryParams: runParameters -> outputDataType -> queryParams)
                (allowOverwrite: bool<allowOverwrite>)
                (cts: CancellationTokenSource)
                (progress: IProgress<string> option)    : Async<RunResult[]> =
        
            processRunParametersSeq 
                maxDegreeOfParallelism 
                runParameters cts progress 
                (fun rp -> executeRunParameters 
                                db 
                                buildQueryParams executor 
                                rp allowOverwrite cts progress)


    let executeRuns
            (db: IGeneSortDb)
            (minRepl: int<replNumber>)
            (maxRepl: int<replNumber>)
            (buildQueryParams: runParameters -> outputDataType -> queryParams)
            (projectName: string<projectName>)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option)
            (executor: runParameters ->
                       bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option
                            -> Async<Result<runParameters, string>>)
            (maxDegreeOfParallelism: int)
            : Async<Result<RunResult[], string>> =
    
        asyncResult {
            try
                report progress (sprintf "%s Executing Runs for %s\n" (MathUtils.getTimestampString()) %projectName)

                // 1. Load Parameters
                let! runParametersArray = 
                    db.getProjectRunParametersForReplRangeAsync (Some minRepl) (Some maxRepl) (Some cts.Token) progress

                report progress (sprintf "Found %d runs to execute\n" runParametersArray.Length)

                if runParametersArray.Length = 0 then
                    report progress "No runs found to execute"
                    return [||]
                else
                    // 1. Run the sequence (returns Async<RunResult[]>)
                    let! results = 
                        executeRunParametersSeq 
                            db maxDegreeOfParallelism executor 
                            runParametersArray buildQueryParams allowOverwrite cts progress
                        |> Async.map Ok // Wrap the naked array in a Result.Ok

                    return results

            with e ->
                // Consistent with your executors, handle the unexpected
                let msg = sprintf "%s Fatal error executing runs: %s\n" (MathUtils.getTimestampString()) e.Message
                report progress msg
                return! async { return Error msg }
        }