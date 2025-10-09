namespace GeneSort.Project

open System.IO
open System.Threading.Tasks
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open FSharp.UMX


module WorkspaceOps =  

    /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let saveWorkspace (workspace: workspace) = // : Async<unit> =
        let filePath = Path.Combine(workspace.WorkspaceFolder, sprintf "%s_Workspace.msgpack" workspace.Name)
        Async.RunSynchronously (OutputData.saveToFile workspace None (workspace |> outputData.Workspace))

    /// Loads a workspace from the specified folder, expecting exactly one *_Workspace.msgpack file
    /// The workspace name is extracted from the file name and must match the name inside the file
    let loadWorkspace (fileFolder: string) : workspace =
        try
            let files = Directory.GetFiles(fileFolder, "*_Workspace.msgpack")
            if Array.isEmpty files then
                failwithf "No workspace file found in %s" fileFolder
            if files.Length > 1 then
                failwithf "Multiple workspace files found in %s: %A" fileFolder files
            let filePath = files.[0]
            let fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath)
            if not (fileNameWithoutExt.EndsWith "_Workspace") then
                failwithf "Invalid workspace file name: %s" filePath
            let underscoreWorkspaceLen = 10
            let extractedName = fileNameWithoutExt.[.. fileNameWithoutExt.Length - underscoreWorkspaceLen - 1]
            use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            let dto = MessagePackSerializer.Deserialize<workspaceDto>(stream, options)
            let loaded = WorkspaceDto.toDomain dto
            if loaded.Name <> extractedName then
                failwithf "Workspace name mismatch: file '%s', loaded '%s'" extractedName loaded.Name
            let newRootDirectory = Path.GetFullPath(Path.Combine(fileFolder, ".."))
            workspace.create loaded.Name loaded.Description newRootDirectory loaded.RunParametersArray loaded.ReportNames
        with e ->
            printfn "Error loading workspace from folder %s: %s" fileFolder e.Message
            raise e


    /// Returns a sequence of Runs made from all possible parameter combinations
    let getRuns2 (workspace: workspace) : run seq =
        workspace.RunParametersArray 
        |> Seq.mapi (fun i runParams  -> 
                        let indexNumber = UMX.tag<indexNumber> i
                        (runParams.SetIndex indexNumber)
                        run.create indexNumber runParams)


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
    let executeWorkspace2
                (workspace: workspace)
                (maxDegreeOfParallelism: int) 
                (executor: workspace -> run -> Async<unit>)
                (runs: run seq)
                : unit =

        let executeRun (run:run) = async {

            let filePathRun = OutputData.getOutputDataFileName 
                                workspace
                                (Some run.RunParameters)
                                outputDataType.Run

            if File.Exists filePathRun then
                        printfn "Skipping Run %d: Output file %s already exists" run.Index filePathRun
            else
                try
                    do! executor workspace run
                    do! OutputData.saveToFile workspace (Some run.RunParameters) (run |> outputData.Run)
                with e ->
                    printfn "Error processing Run %d: %s" run.Index e.Message
        }

        let limitedParallel =
            runs
            |> Seq.map executeRun
            |> Seq.toList
            |> ParallelWithThrottle maxDegreeOfParallelism

        Async.RunSynchronously limitedParallel