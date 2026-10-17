namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1

/// Define the contract for an Executor
type IRunParamsExecutor =
    abstract member Execute : 
        host: runHost -> 
        rp: runParameters -> 
        allowOverwrite: bool<allowOverwrite> -> 
        cts: CancellationTokenSource -> 
        progress: IProgress<string> option -> 
        Async<Result<runParameters, string>>


module RunParamsExecutor =
    /// Run execution is selected by ExecuteFunctionName when the host is created.
    let hostExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                host.ExecuteFunction host.ExecutionContext rp allowOverwrite cts progress }

    let execute (executor: IRunParamsExecutor) 
                (host: runHost) 
                (rp: runParameters) 
                (allowOverwrite: bool<allowOverwrite>) 
                (cts: CancellationTokenSource) 
                (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        executor.Execute host rp allowOverwrite cts progress
