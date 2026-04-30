namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open OpsUtils
open GeneSort.Project.V1
open GeneSort.Db.V1

module ProjectOps =

    /// Internal logic for a single unit of work
    let private runTask (db: IGeneSortDb)
                        (runName: string<runName>)
                        buildQueryParams executor allowOverwrite cts progress (rp: runParameters) = 
        async {
            let id = rp.GetId() |> Option.defaultValue (Guid.Empty |> UMX.tag)
            let repl = rp.GetRepl() |> Option.defaultValue (-1 |> UMX.tag)
    
            try
                if rp.GetRunFinished() |> Option.defaultValue false then
                    return Skipped (id, repl, sprintf "\n%s" (rp |> RunParameters.reportKvps))
                else
                    report progress (sprintf "%s Run %s Repl %d started" (MathUtils.getTimestampString()) (%id.ToString()) %repl)
                
                    // 1. Unwrapping the Result manually inside the async block
                    let! execResult = executor rp allowOverwrite cts progress
                
                    match execResult with
                    | Error m -> 
                        return Failure (id, repl, sprintf "%s\n%s" m (rp |> RunParameters.reportKvps))
                    | Ok updated ->
                        let qp = buildQueryParams updated (outputDataType.RunParameters %runName)
                    
                        // 2. Standard async bind (no Result wrapper here)
                        let! _ = db.saveAsync qp (updated |> outputData.RunParameters) (true |> UMX.tag)
                    
                        return Success (id, repl, (updated |> RunParameters.reportKvps))
            with 
            | :? OperationCanceledException -> return Cancelled (id, repl)
            | e -> return Failure (id, repl, sprintf "Fault: %s" e.Message)
        }

    /// The single entry point for executing a batch of runs
    let executeRuns
            (db: IGeneSortDb)
            (minRepl: int<replNumber>)
            (maxRepl: int<replNumber>)
            (buildQueryParams: runParameters -> outputDataType -> queryParams)
            (runName: string<runName>)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option)
            (executor: runParameters -> bool<allowOverwrite> -> CancellationTokenSource -> IProgress<string> option -> Async<Result<runParameters, string>>)
            (maxParallel: int) =
        asyncResult {
            try
                report progress (sprintf "%s Starting project: %s" (MathUtils.getTimestampString()) %runName)

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
                        |> Seq.map (runTask db runName buildQueryParams executor allowOverwrite cts progress)
                        |> fun tasks -> Async.Parallel(tasks, maxParallel)

                    // Unified reporting logic
                    let s, f, sk, c = 
                        results |> Array.fold (fun (s, f, sk, c) -> function
                            | Success _ -> (s+1, f, sk, c) | Failure _ -> (s, f+1, sk, c)
                            | Skipped _ -> (s, f, sk+1, c) | Cancelled _ -> (s, f, sk, c+1)
                        ) (0, 0, 0, 0)

                    report progress (sprintf "\n=== Batch Complete ===\nSucceeded: %d, Failed: %d, Skipped: %d, Cancelled: %d" s f sk c)
                    return results
            with e -> 
                let msg = sprintf "Fatal: %s" e.Message
                report progress msg
                return! async { return Error msg }
        }