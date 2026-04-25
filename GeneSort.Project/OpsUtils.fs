namespace GeneSort.Project
open System
open System.Threading
open FSharp.UMX
open GeneSort.Db
open GeneSort.Runs
open GeneSort.Core

module OpsUtils =

    let inline report (progress: IProgress<string> option) msg =
        progress |> Option.iter (fun p -> p.Report msg)


    let makeErrorTable (failures: (runParameters * string) list) : string [] =
        let mutable modRunParameters = []

        for (rp, err) in failures do
            modRunParameters <- (rp.WithMessage(Some err)) :: modRunParameters

        let mutable tableRows =     
                            (RunParameters.makeIndexAndReplTable modRunParameters
                            |> Array.map (fun row -> String.Join("\t", row)))

        tableRows <- [|"\n--- ********* ---"|]
                     |> Array.append  tableRows
                     |> Array.append [|"--- Error Runs ---"|] 

        tableRows

    
    let makeSourceTable (runParams : runParameters []) : string [] =
        let mutable tableRows =     
                            (RunParameters.makeIndexAndReplTable runParams
                            |> Array.map (fun row -> String.Join("\t", row)))


        tableRows <- [|"\n--- ********* ---"|]
                     |> Array.append  tableRows
                     |> Array.append [|"--- Successful Runs ---"|] 

        tableRows


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



    let printRunParamsTable
            (db: IGeneSortDb)
            (minReplNumber: int<replNumber>)
            (maxReplNumber: int<replNumber>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) =
        asyncResult {
            try
                report progress (sprintf "Displaying Source Table for %s\n" %db.projectFolder)

                let! runParametersArray = 
                    db.getProjectRunParametersForReplRangeAsync (Some minReplNumber) (Some maxReplNumber) (Some cts.Token) progress

                if runParametersArray.Length = 0 then
                    report progress "No runs found to display.\n"
                else
                    let sourceTableRows = makeSourceTable runParametersArray
                    sourceTableRows |> Array.iter (report progress)
                    report progress (sprintf "\nFound %d run configurations.\n" runParametersArray.Length)
                return () 
            with e ->
                let msg = sprintf "Error displaying source table: %s" e.Message
                report progress msg
                return! async { return Error msg }
        }