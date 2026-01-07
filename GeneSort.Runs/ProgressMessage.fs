namespace GeneSort.Runs

open System
open FSharp.UMX
open GeneSort.Core

type progressMessage =
    | Info of string
    | RunCompleted of RunResult
    | BatchCompleted of successCount: int * failureCount: int * skippedCount: int


module ProgressMessage =

    // Helper to report structured progress
    let reportRunResult (progress: IProgress<string> option) (result: RunResult) =
        match progress with
        | Some p ->
            let timeStamp = MathUtils.getTimestampString()
            let msg = 
                match result with
                | Success (idx, repl, congrats) -> 
                    sprintf "%s Run %s Repl %d completed %s" timeStamp ((%idx).ToString()) %repl congrats
                | Failure (idx, repl, error) -> 
                    sprintf "%s Run %s Repl %d failed: %s" timeStamp ((%idx).ToString()) %repl error
                | Skipped (idx, repl, reason) -> 
                    sprintf "%s Run %s Repl %d skipped: %s" timeStamp ((%idx).ToString()) %repl reason
                | Cancelled (idx, repl) ->
                    sprintf "%s Run %s Repl %d cancelled" timeStamp ((%idx).ToString()) %repl
            p.Report(msg)
        | None -> ()

