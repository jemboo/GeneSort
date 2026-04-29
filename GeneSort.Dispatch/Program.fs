module Program

open System
open FSharp.UMX
open System.Threading
open GeneSort.Dispatch.V1
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1

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
printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"

// --- Configuration Selection ---

let configKey = "P1" 

let spec = 
    match RandomSorterBins.Configs |> Map.tryFind configKey with
    | Some s -> s
    | None -> failwithf "Config key '%s' not found." configKey

let host = RandomSorterBins.CreateHost spec

let minReplica = 0<replNumber>
let maxReplica = 10<replNumber>

// --- Execution ---

async {
    printfn "Running Project: %s" host.Spec.ProjectName
    
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
    | Error e -> printfn "Init Failure: %s" e
    | Ok () ->
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
        | Ok results -> printfn "Success: %d records processed." results.Length
        | Error e -> printfn "Runtime Error: %s" e

} |> Async.RunSynchronously

let duration = DateTime.Now - startTime
printfn "********************************************"
printfn $"Total Time: {duration.ToString()}"
printfn "********************************************"
Console.ReadLine() |> ignore