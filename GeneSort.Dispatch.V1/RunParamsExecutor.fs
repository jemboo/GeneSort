namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1

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
