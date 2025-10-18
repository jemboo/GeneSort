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


module ProjectOps =  

    /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    /// Loads a project from the specified folder, expecting exactly one *_Project.msgpack file
    /// The project name is extracted from the file name and must match the name inside the file
    let loadProject (fileFolder: string) : project =
        try
            let files = Directory.GetFiles(fileFolder, "*_Project.msgpack")
            if Array.isEmpty files then
                failwithf "No project file found in %s" fileFolder
            if files.Length > 1 then
                failwithf "Multiple project files found in %s: %A" fileFolder files
            let filePath = files.[0]
            let fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath)
            if not (fileNameWithoutExt.EndsWith "_Project") then
                failwithf "Invalid project file name: %s" filePath
            let underscoreProjectLen = 10
            let extractedName = fileNameWithoutExt.[.. fileNameWithoutExt.Length - underscoreProjectLen - 1]
            use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            let dto = MessagePackSerializer.Deserialize<projectDto>(stream, options)
            let loaded = ProjectDto.toDomain dto
            if loaded.Name <> extractedName then
                failwithf "Project name mismatch: file '%s', loaded '%s'" extractedName loaded.Name
            let newRootDirectory = Path.GetFullPath(Path.Combine(fileFolder, ".."))
            project.create loaded.Name loaded.Description newRootDirectory loaded.RunParametersArray loaded.ReportNames
        with e ->
            printfn "Error loading project from folder %s: %s" fileFolder e.Message
            raise e


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
            (progress: IProgress<string>) = async {

        let filePathRun = OutputData.getOutputDataFileName 
                            projectFolder
                            (Some runParameters)
                            outputDataType.RunParameters

        if File.Exists filePathRun then
                    printfn "Skipping Run %d: Output file %s already exists" (runParameters.GetIndex()) filePathRun
        else
            try
                do! executor projectFolder runParameters cts progress
                do! OutputData.saveToFile projectFolder (Some runParameters) (runParameters |> outputData.RunParameters)
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