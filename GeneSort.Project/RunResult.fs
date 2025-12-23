namespace GeneSort.Project

open FSharp.UMX
open GeneSort.Runs



type RunResult =
    | Success of runId: string<idValue> * runRepl: int<replNumber>
    | Failure of runId: string<idValue> * runRepl: int<replNumber> * error: string
    | Skipped of runId: string<idValue> * runRepl: int<replNumber> * reason: string



module RunResult = ()

