namespace GeneSort.Project

open GeneSort.Runs



type RunResult =
    | Success of runIndex: int<indexNumber> * runRepl: int<replNumber>
    | Failure of runIndex: int<indexNumber> * runRepl: int<replNumber> * error: string
    | Skipped of runIndex: int<indexNumber> * runRepl: int<replNumber> * reason: string



module RunResult = ()

