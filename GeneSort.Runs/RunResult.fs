namespace GeneSort.Runs

open FSharp.UMX
open GeneSort.Runs


type RunResult =
    | Success of id: string<idValue> * repl: int<replNumber> * message: string
    | Failure of id: string<idValue> * repl: int<replNumber> * message: string
    | Skipped of id: string<idValue> * repl: int<replNumber> * message: string
    | Cancelled of id: string<idValue> * repl: int<replNumber>



module RunResult =

/// Analyzes results to identify what needs attention
    let analyze (results: RunResult[]) =
        // 1. Extract specifically which ones failed or were cancelled
        let issues = 
            results |> Array.choose (function
                | Failure (idx, r, msg) -> Some (sprintf "Run %s (Repl %d) FAILED: %s" %idx r msg)
                | Cancelled (idx, r)    -> Some (sprintf "Run %s (Repl %d) was CANCELLED" %idx r)
                | _ -> None)

        // 2. Extract specifically which ones are "Missing" (Skipped)
        let missing = 
            results |> Array.choose (function
                | Skipped (idx, r, msg) -> Some (sprintf "Run %s (Repl %d) is MISSING: %s" %idx r msg)
                | _ -> None)

        // 3. Simple counts
        let successCount = results |> Array.filter (function Success _ -> true | _ -> false) |> Array.length
        
        {| 
            Successes = successCount
            IssueLog = issues
            MissingLog = missing 
            Total = results.Length
        |}

