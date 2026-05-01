module Program

open System
open FSharp.UMX
open System.Threading
open GeneSort.Dispatch.V1
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1

let createThreadSafeProgress() =
    let agent = MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()
                printfn "%s" msg
        })
    { new IProgress<string> with member _.Report(msg) = agent.Post(msg) }

let progress = createThreadSafeProgress()
let cts = new CancellationTokenSource()
let maxParallel = Environment.ProcessorCount

let startTime = DateTime.Now
printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"

let configKey = "P1" 

// The Program now works purely against the IRunHost interface
let host: IRunHost = 
    match SorterEvalBins.RandomStandardSorters.Configs |> Map.tryFind configKey with
    | Some s -> SorterEvalBins.RandomStandardSorters.CreateHost s
    | None -> failwithf "Config key '%s' not found." configKey

let minReplica = 0<replNumber>
let maxReplica = 10<replNumber>

async {
    printfn "Running Project: %s" %host.Project.ProjectName
    
    let! initResult = 
        ParamOps.initProjectAndRunFiles
            host.ProjectDb           
            host.MakeQueryParamsFromRunParams 
            cts 
            (Some progress) 
            host.Project              
            minReplica 
            maxReplica 
            host.AllowOverwrite 
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
                host.Project.RunName 
                host.AllowOverwrite 
                cts 
                (Some progress) 
                host.Executor
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