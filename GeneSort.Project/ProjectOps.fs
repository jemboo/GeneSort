namespace GeneSort.Project

open System.IO
open System.Threading.Tasks
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open FSharp.UMX
open System
open System.Threading
open GeneSort.Runs.Params
open GeneSort.Runs
open GeneSort.Db


module ProjectOps =  

    /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    /// Executes async computations in parallel, limited to maxDegreeOfParallelism at a time
    let private ParallelWithThrottle (maxDegreeOfParallelism: int) (computations: seq<Async<unit>>) : Async<unit> =
        async {
            use semaphore = new System.Threading.SemaphoreSlim(maxDegreeOfParallelism)
            let tasks =
                computations
                |> Seq.map (fun comp ->
                    async {
                        try
                            do! Async.AwaitTask (semaphore.WaitAsync())
                            do! comp
                        finally
                            semaphore.Release() |> ignore
                    }
                    |> Async.StartAsTask : Task<unit>)
                |> Seq.toArray
            let! _ = Async.AwaitTask (Task.WhenAll(tasks))
            return ()
        }


    let executeRunParameters
            (projectFolder: string)
            (executor: string -> runParameters -> CancellationTokenSource -> IProgress<string> ->Async<unit>)
            (runParameters:runParameters) 
            (cts: CancellationTokenSource)  
            (progress: IProgress<string>) : Async<unit> = async {

        let filePathRun = OutputDataFile.getOutputDataFileName 
                            projectFolder
                            (Some runParameters)
                            outputDataType.RunParameters

        if File.Exists filePathRun then
                    printfn "Skipping Run %d: Output file %s already exists" (runParameters.GetIndex()) filePathRun
        else
            try
                do! executor projectFolder runParameters cts progress
                do! OutputDataFile.saveToFile projectFolder (Some runParameters) (runParameters |> outputData.RunParameters)
            with e ->
                printfn "Error processing Run %d: %s" (runParameters.GetIndex()) e.Message
    }


    /// Executes all runs from the project, running up to atTheSameTime runs concurrently
    /// Skips runs if their output file already exists; saves runs to .msgpack files after execution
    let executeRunParametersSeq
                (project: project)
                (maxDegreeOfParallelism: int) 
                (executor: string -> runParameters -> CancellationTokenSource -> IProgress<string> ->Async<unit>)
                (runParameters: runParameters seq)
                (cts: CancellationTokenSource)
                (progress: IProgress<string>)
                : unit =


        let limitedParallel =
            runParameters
            |> Seq.map (fun rps -> executeRunParameters project.ProjectFolder executor rps cts progress)
            |> Seq.toList
            |> ParallelWithThrottle maxDegreeOfParallelism

        Async.RunSynchronously limitedParallel




    let executeRunParametersSeqAsync
        (project: project)
        (maxDegreeOfParallelism: int) 
        (executor: string -> runParameters -> CancellationTokenSource -> IProgress<string> -> Async<unit>)
        (runParameters: runParameters seq)
        (cts: CancellationTokenSource)
        (progress: IProgress<string>)
        : Async<unit> =

        async {
            cts.Token.ThrowIfCancellationRequested()  // Early cancel check

            let tasks =
                runParameters
                |> Seq.map (fun rps -> executeRunParameters project.ProjectFolder executor rps cts progress)
                |> Seq.toList

            do! ParallelWithThrottle maxDegreeOfParallelism tasks
        }
