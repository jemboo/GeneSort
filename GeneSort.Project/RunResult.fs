namespace GeneSort.Project

open System.Threading.Tasks
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System
open System.Threading
open GeneSort.Runs.Params
open GeneSort.Db



type RunResult =
    | Success of runIndex: int<indexNumber> * runRepl: int<replNumber>
    | Failure of runIndex: int<indexNumber> * runRepl: int<replNumber> * error: string
    | Skipped of runIndex: int<indexNumber> * runRepl: int<replNumber> * reason: string



module RunResult = ()

