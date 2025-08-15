namespace GeneSort.Project.Test

open System
open FSharp.UMX
open Xunit
open MessagePack.Resolvers
open MessagePack
open MessagePack.FSharp
open System.IO
open GeneSort.Project
open System.Threading

type WorkspaceTests() =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    // Helper to create a temporary directory for tests
    let createTempDir () =
        let tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        Directory.CreateDirectory tempPath |> ignore
        tempPath

    // Helper to clean up temporary directory with retries
    let cleanupTempDir (path: string) =
        if Directory.Exists path then
            let maxRetries = 5
            let retryDelayMs = 100
            let rec tryDelete retries =
                try
                    Directory.Delete(path, true)
                with
                | :? IOException when retries > 0 ->
                    Thread.Sleep retryDelayMs
                    tryDelete (retries - 1)
                | ex when retries = 0 ->
                    printfn "Failed to delete directory %s after %d retries: %s" path maxRetries ex.Message
            tryDelete maxRetries


    // Helper to create a sample workspace
    let createWorkspace (rootDir: string) (parameterSets: list<string * list<string>>) =
        Workspace.create
            "TestWorkspace"
            "Test Description"
            rootDir
            parameterSets


    [<Fact>]
    let ``getRuns generates correct number of runs`` () =
        let workspace = createWorkspace (createTempDir ()) [("algorithm", ["quick"; "merge"]); ("size", ["small"; "large"])]
        let runs = WorkspaceOps.getRuns workspace |> Seq.toList
        Assert.Equal(4, runs.Length) // 2 algorithms * 2 sizes = 4 combinations
        let indices = runs |> List.map (fun r -> r.Index)
        Assert.Equal<int>([0; 1; 2; 3], indices)
        let parameters = runs |> List.map (fun r -> r.Parameters)
        let expectedParams = [
            Map.ofList [("algorithm", "quick"); ("size", "small")]
            Map.ofList [("algorithm", "quick"); ("size", "large")]
            Map.ofList [("algorithm", "merge"); ("size", "small")]
            Map.ofList [("algorithm", "merge"); ("size", "large")]
        ]
        Assert.All(List.zip parameters expectedParams, fun (actual, expected) -> 
                Assert.Equal<Map<string, string>>(expected, actual))
        cleanupTempDir workspace.RootDirectory

    [<Fact>]
    let ``getRunFileName produces correct file path`` () =
        let tempDir = createTempDir ()
        let workspace = createWorkspace tempDir [("algorithm", ["quick"; "merge"]); ("size", ["small"; "large"])]
        let run = { Index = 3; Parameters = Map.ofList [("algorithm", "quick")] }
        let filePath = workspace.getRunFileName run
        let expectedPath = Path.Combine(tempDir, "TestWorkspace", "Runs", "Run_3.msgpack")
        Assert.Equal(expectedPath, filePath)
        cleanupTempDir tempDir


    [<Fact>]
    let ``executeWorkspace executes runs and saves files`` () =
        let tempDir = createTempDir ()
        let workspace = createWorkspace tempDir [("algorithm", ["quick"; "merge"; "fonzy"; "ralph"; "quick1"; "merge1"; "fonzy1"; "ralph1"; "quick2"; "merge2"; "fonzy2"; "ralph2"])]
        // Clear Runs folder to ensure no existing files
        let runsFolder = workspace.getWorkspaceFolder "Runs"
        if Directory.Exists runsFolder then
            Directory.Delete(runsFolder, true)
        // Log existing files for debugging
        let runs = WorkspaceOps.getRuns workspace |> Seq.toList
        for run in runs do
            let filePath = workspace.getRunFileName run
            if File.Exists filePath then
                printfn "File already exists for Run %d: %s" run.Index filePath
        let mutable executedRuns = []

        let lockObj = obj() // Lock object for thread safety
        let executor _ (run: Run) =
            printfn "Executing Run %d" run.Index
            lock lockObj (fun () ->
                executedRuns <- run.Index :: executedRuns)

        WorkspaceOps.executeWorkspace workspace 2 executor
        Assert.Equal(12, runs.Length) // 12 algorithms
        for run in runs do
            let filePath = workspace.getRunFileName run
            Assert.True(File.Exists filePath)
            use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)
            let runDto = MessagePackSerializer.Deserialize<RunDto>(stream, options)
            let expectedDto = Run.toRunDto run
            Assert.Equal(expectedDto.Index, runDto.Index)
            Assert.Equal<Map<string,string>>(expectedDto.Properties, runDto.Properties)
        cleanupTempDir tempDir
        Assert.Equal(12, executedRuns.Length)
        Assert.Contains(1, executedRuns)
        Assert.Contains(2, executedRuns)
        Assert.Contains(3, executedRuns)


    [<Fact>]
    let ``executeWorkspace skips existing run files`` () =
        let tempDir = createTempDir ()
        let workspace = createWorkspace tempDir [("algorithm", ["quick"])]
        let run = { Index = 0; Parameters = Map.ofList [("algorithm", "quick")] }
        let filePath = workspace.getRunFileName run
        Directory.CreateDirectory (Path.GetDirectoryName filePath) |> ignore
        use stream = new FileStream(filePath, FileMode.Create, FileAccess.Write)
        MessagePackSerializer.Serialize(stream, Run.toRunDto run, options)
        let mutable executedCount = 0
        let executor _ _ = executedCount <- executedCount + 1
        WorkspaceOps.executeWorkspace workspace 2 executor
        Assert.Equal(0, executedCount) // Run should be skipped
        cleanupTempDir tempDir

    [<Fact>]
    let ``executeWorkspace handles executor errors`` () =
        let tempDir = createTempDir ()
        let workspace = createWorkspace tempDir [("algorithm", ["quick"])]
        let executor _ (run: Run) = raise (Exception $"Error in Run {run.Index}")
        WorkspaceOps.executeWorkspace workspace 2 executor
        let run = { Index = 0; Parameters = Map.ofList [("algorithm", "quick")] }
        let filePath = workspace.getRunFileName run
        Assert.False(File.Exists filePath) // File not created due to error
        cleanupTempDir tempDir