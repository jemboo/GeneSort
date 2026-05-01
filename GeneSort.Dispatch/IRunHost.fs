namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1

// --- 1. Infrastructure Interface ---

/// The central interface allowing the Program to run any host implementation
type IRunHost =
    abstract member ProjectDb: IGeneSortDb
    abstract member Project: run
    abstract member ParameterSpans: (string * string list) list
    abstract member AllowOverwrite: bool<allowOverwrite>
    abstract member MakeQueryParamsFromRunParams: runParameters -> outputDataType -> queryParams
    abstract member ParamMapRefiner: runParameters seq -> runParameters seq
    abstract member Executor: 
                    runParameters -> bool<allowOverwrite> -> 
                    CancellationTokenSource -> IProgress<string> option 
                    -> Async<Result<runParameters, string>>
