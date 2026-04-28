module Program

open System
open FSharp.UMX
open System.Threading
open GeneSort.Dispatch.V1
open GeneSort.Project.V1

// --- Infrastructure ---

let createThreadSafeProgress() =
    let agent = MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()
                printfn "%s" msg
        })
    { new IProgress<string> with member _.Report(msg) = agent.Post(msg) }

// --- Global Setup ---

let progress = createThreadSafeProgress()
let cts = new CancellationTokenSource()
let maxParallel = Environment.ProcessorCount

let startTime = DateTime.Now
printfn $"**** GeneSort Execution Started: {startTime.ToString()} ****"

// --- Configuration Selection ---

let configKey = "P1" 

let spec = 
    match RandomSorterBins.Configs |> Map.tryFind configKey with
    | Some s -> s
    | None -> failwithf "Configuration key '%s' not found." configKey

let host = RandomSorterBins.CreateHost spec

let minReplica = 0<replNumber>
let maxReplica = 10<replNumber>

// --- Execution ---

async {
    printfn "Config:      %s" configKey
    printfn "Project:     %s" host.Spec.ProjectName
    printfn "Folder:      %s" (host.ProjectDb.ToString())
    
    // 1. Initialize
    let! initResult = 
        ParamOps.initProjectAndRunFiles
            host.ProjectDb           
            host.MakeQueryParamsFromRunParams 
            cts 
            (Some progress) 
            host.Project              
            minReplica 
            maxReplica 
            host.Spec.AllowOverwrite 
            host.ParamMapRefiner      
            host.ParameterSpans

    match initResult with
    | Error e -> printfn "Initialization Failed: %s" e
    | Ok () ->
        printfn "Initialization Success. Running tests..."
        
        // 2. Execute
        let! execResult = 
            ProjectOps.executeRuns 
                host.ProjectDb      
                minReplica 
                maxReplica 
                host.MakeQueryParamsFromRunParams 
                host.Project.ProjectName 
                host.Spec.AllowOverwrite 
                cts 
                (Some progress) 
                (RandomSorterBins.executor host)
                maxParallel

        match execResult with
        | Ok results -> printfn "Execution complete. %d results processed." results.Length
        | Error e -> printfn "Execution Failed: %s" e

} |> Async.RunSynchronously

// --- Finalize ---

let endTime = DateTime.Now
let duration = endTime - startTime
Thread.Sleep(100) 

printfn "********************************************"
printfn $"Total Duration: {duration.ToString()}"
printfn "********************************************"

Console.WriteLine("Press Enter to exit...")
Console.ReadLine() |> ignore