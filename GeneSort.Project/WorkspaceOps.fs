
namespace GeneSort.Project

open System
open FSharp.UMX
open GeneSort.Sorter
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System.IO
open GeneSort.Core.Combinatorics
open System.Threading.Tasks


module WorkspaceOps =  

    /// Returns a sequence of Runs made from all possible parameter combinations
    let getRuns (workspace: Workspace) (cycle: int<cycleNumber>) : Run seq =
        workspace.ParameterSets 
        |> cartesianProductMaps
        |> Seq.mapi (fun i paramsMap -> { Index = i; Cycle = cycle; Parameters = paramsMap })


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
                (workspace: Workspace) 
                (cycle: int<cycleNumber>)
                (maxDegreeOfParallelism: int) 
                (executor: Workspace -> int<cycleNumber> -> Run -> Async<unit>)
                : unit =
        let runs = getRuns workspace cycle
        let executeRun (run:Run) = async {
            let filePathRun = OutputData.getOutputFileName workspace.WorkspaceFolder run.Index run.Cycle (outputDataType.Run |> OutputDataType.toString)
            if File.Exists filePathRun then
                        printfn "Skipping Run %d: Output file %s already exists" run.Index filePathRun
            else
                try
                    do! executor workspace cycle run
                    do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (run |> outputData.Run)
                with e ->
                    printfn "Error processing Run %d: %s" run.Index e.Message
        }

        let limitedParallel =
            runs
            |> Seq.map executeRun
            |> Seq.toList
            |> ParallelWithThrottle maxDegreeOfParallelism
        Async.RunSynchronously limitedParallel
