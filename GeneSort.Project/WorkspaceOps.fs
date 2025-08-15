
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
    /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let saveRunDto (workspace: Workspace) (run: Run) : System.Threading.Tasks.Task =
        let filePath = workspace.getRunFileName run
        let runDto = Run.toRunDto run
        let directory = Path.GetDirectoryName filePath
        Directory.CreateDirectory directory |> ignore
        use stream = new FileStream(filePath, FileMode.Create, FileAccess.Write)
        MessagePackSerializer.SerializeAsync(stream, runDto, options) // |> Async.AwaitTask |> Async.RunSynchronously


    /// Returns a sequence of Runs made from all possible parameter combinations
    let getRuns (workspace: Workspace) : Run seq = 
        workspace.ParameterSets
        |> cartesianProductMaps
        |> Seq.mapi (fun i paramsMap -> { Index = i; Parameters = paramsMap })


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
    let executeWorkspace (workspace: Workspace) (maxDegreeOfParallelism: int) (executor: Workspace -> Run -> unit) : unit =
        let runs = getRuns workspace
        let executeRun run = async {
            let filePath = workspace.getRunFileName run
            if File.Exists filePath then
                () // Skip if file exists
            else
                try
                    executor workspace run
                    do! Async.AwaitTask (saveRunDto workspace run)
                with e ->
                    printfn "Error processing Run %d: %s" run.Index e.Message
        }
        let limitedParallel =
            runs
            |> Seq.map executeRun
            |> ParallelWithThrottle maxDegreeOfParallelism
        Async.RunSynchronously limitedParallel