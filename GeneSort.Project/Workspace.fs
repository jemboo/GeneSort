
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



[<MessagePackObject>]
type WorkspaceDTO = 
    { 
        [<MessagePack.Key("Name")>] Name: string
        [<MessagePack.Key("Description")>]  Description: string
        [<MessagePack.Key("RootDirectory")>] RootDirectory: string
        [<MessagePack.Key("ParameterSets")>] ParameterSets: list<string * list<string>>
    }

// Workspace type
type Workspace = 
    { 
      Name: string
      Description: string
      RootDirectory: string
      ParameterSets: list<string * list<string>>
    }


module Workspace =  
    /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    let toWorkspaceDTO (workspace: Workspace) : WorkspaceDTO =
        { Name = workspace.Name
          Description = workspace.Description
          RootDirectory = workspace.RootDirectory
          ParameterSets = workspace.ParameterSets }

    let fromWorkspaceDTO (dto: WorkspaceDTO) : Workspace =
        { Workspace.Name = dto.Name
          Description = dto.Description
          RootDirectory = dto.RootDirectory
          ParameterSets = dto.ParameterSets }

    let getWorkspaceFolder (workspace:Workspace) (folderName:string) : string =
        Path.Combine(workspace.RootDirectory, workspace.Name, folderName)

    /// Returns the file path for a Run, using the Runs folder and Run_<index>.msgpack naming
    let getRunFileName (workspace: Workspace) (run: Run) : string =
        let folder = getWorkspaceFolder workspace "Runs"
        let fileName = sprintf "Run_%d.msgpack" run.Index
        Path.Combine(folder, fileName)


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
            let filePath = getRunFileName workspace run
            if File.Exists filePath then
                () // Skip if file exists
            else
                try
                    executor workspace run
                    let runDto = Run.toRunDTO run
                    let directory = Path.GetDirectoryName filePath
                    Directory.CreateDirectory directory |> ignore
                    use stream = new FileStream(filePath, FileMode.Create, FileAccess.Write)
                    do! Async.AwaitTask (MessagePackSerializer.SerializeAsync(stream, runDto, options))
                with e ->
                    printfn "Error processing Run %d: %s" run.Index e.Message
        }
        let limitedParallel =
            runs
            |> Seq.map executeRun
            |> ParallelWithThrottle maxDegreeOfParallelism
        Async.RunSynchronously limitedParallel