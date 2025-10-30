namespace GeneSort.Project


open System
open FSharp.UMX

type ProgressMessage =
    | Info of string
    | RunCompleted of RunResult
    | BatchCompleted of successCount: int * failureCount: int * skippedCount: int


module ProgressMessage =

    // Helper to report structured progress
    let reportRunResult (progress: IProgress<string> option) (result: RunResult) =
        match progress with
        | Some p ->
            let msg = 
                match result with
                | Success (idx, repl) -> 
                    sprintf "✓ Run %d_%d completed successfully" %idx %repl
                | Failure (idx, repl, error) -> 
                    sprintf "✗ Run %d_%d failed: %s" %idx %repl error
                | Skipped (idx, repl, reason) -> 
                    sprintf "⊘ Run %d_%d skipped: %s" %idx %repl reason
            p.Report(msg)
        | None -> ()

