namespace GeneSort.Runs

open FSharp.UMX
open GeneSort.Core
open GeneSort.Runs


type RunResult =
    | Success of id: Guid<idValue> * repl: int<replNumber> * message: string
    | Failure of id: Guid<idValue> * repl: int<replNumber> * message: string
    | Skipped of id: Guid<idValue> * repl: int<replNumber> * message: string
    | Cancelled of id: Guid<idValue> * repl: int<replNumber>



module RunResult =

/// Analyzes results to identify what needs attention
    let analyze (results: RunResult[]) =
        let timeStamp = MathUtils.getTimestampString()
        // 1. Extract specifically which ones failed or were cancelled
        let issues = 
            results |> Array.choose (function
                | Failure (idx, r, msg) -> Some (sprintf "%s Run %s (Repl %d) FAILED: %s" timeStamp ((%idx).ToString()) r msg)
                | Cancelled (idx, r)    -> Some (sprintf "%s Run %s (Repl %d) was CANCELLED" timeStamp ((%idx).ToString()) r)
                | _ -> None)

        // 2. Extract specifically which ones are "Missing" (Skipped)
        let missing = 
            results |> Array.choose (function
                | Skipped (idx, r, msg) -> Some (sprintf "%s Run %s (Repl %d) is MISSING: %s" timeStamp ((%idx).ToString()) r msg)
                | _ -> None)

        // 3. Simple counts
        let successCount = results |> Array.filter (function Success _ -> true | _ -> false) |> Array.length
        
        {| 
            Successes = successCount
            IssueLog = issues
            MissingLog = missing 
            Total = results.Length
        |}

