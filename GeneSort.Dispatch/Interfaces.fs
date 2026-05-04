namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1


/// The central interface allowing the Program to run any host implementation
type IRunHost =
    abstract member ProjectDb: IGeneSortDb
    abstract member Project: run
    abstract member ParameterSpans: (string * string list) list
    abstract member AllowOverwrite: bool<allowOverwrite>
    abstract member MakeQueryParamsFromRunParams: runParameters -> outputDataType -> queryParams
    abstract member ParamMapRefiner: runParameters seq -> runParameters seq
    abstract member Execute: 
                    runParameters -> bool<allowOverwrite> -> 
                    CancellationTokenSource -> IProgress<string> option 
                    -> Async<Result<runParameters, string>>



/// Define the contract for an Executor
type IRunParamsExecutor =
    abstract member Execute : 
        host: IRunHost -> 
        rp: runParameters -> 
        allowOverwrite: bool<allowOverwrite> -> 
        cts: CancellationTokenSource -> 
        progress: IProgress<string> option -> 
        Async<Result<runParameters, string>>


module RunParamsExecutor =
    let execute (executor: IRunParamsExecutor) 
                (host: IRunHost) 
                (rp: runParameters) 
                (allowOverwrite: bool<allowOverwrite>) 
                (cts: CancellationTokenSource) 
                (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        executor.Execute host rp allowOverwrite cts progress
