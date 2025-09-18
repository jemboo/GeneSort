
namespace GeneSort.Project

open System.IO
open System.Threading.Tasks


module WorkspaceOps =  

    /// Returns a sequence of Runs made from all possible parameter combinations
    let getRuns (workspace: workspace) (repl: int<replNumber>) : run seq =
        workspace.RunParametersArray 
        |> Seq.mapi (fun i runParams  -> 
                        (runParams.SetRepl repl )
                        run.create i repl runParams)

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


    /// Executes all runs from the workspace, running up to atTheSameTime runs concurrently
    /// Skips runs if their output file already exists; saves runs to .msgpack files after execution
    let executeWorkspace 
                (workspace: workspace) 
                (repl: int<replNumber>)
                (maxDegreeOfParallelism: int) 
                (executor: workspace -> int<replNumber> -> run -> Async<unit>)
                : unit =

        let runs = getRuns workspace repl

        let executeRun (run:run) = async {

            let filePathRun = OutputData.getOutputFileName 
                                workspace.WorkspaceFolder
                                run.Index 
                                run.Repl 
                                (outputDataType.Run |> OutputDataType.toString) 

            if File.Exists filePathRun then
                        printfn "Skipping Run %d: Output file %s already exists" run.Index filePathRun
            else
                try
                    do! executor workspace repl run
                    do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Repl (run |> outputData.Run)
                with e ->
                    printfn "Error processing Run %d: %s" run.Index e.Message
        }

        let limitedParallel =
            runs
            |> Seq.map executeRun
            |> Seq.toList
            |> ParallelWithThrottle maxDegreeOfParallelism
        Async.RunSynchronously limitedParallel
