namespace GeneSort.Project


open System
open FSharp.UMX

type progressMessage =
    | Info of string
    | RunCompleted of RunResult
    | BatchCompleted of successCount: int * failureCount: int * skippedCount: int


module ProgressMessage =

    // Helper to report structured progress
    let reportRunResult (progress: IProgress<string> option) (result: RunResult) =
        match progress with
        | Some p ->
            let timeStamp = DateTime.Now.ToLongTimeString ()
            let msg = 
                match result with
                | Success (idx, repl) -> 
                    sprintf "✓ %s  Run %s_%d completed successfully" timeStamp %idx %repl 
                | Failure (idx, repl, error) -> 
                    sprintf "✗ %s  Run %s_%d failed: %s" timeStamp %idx %repl error
                | Skipped (idx, repl, reason) -> 
                    sprintf "⊘ %s  Run %s_%d skipped: %s" timeStamp %idx %repl reason
            p.Report(msg)
        | None -> ()

