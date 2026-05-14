module Program

open System
open FSharp.UMX
open System.Threading
open GeneSort.Dispatch.V1
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open System.Runtime
open GeneSort.Dispatch.V1.SorterEvalBins

let createThreadSafeProgress() =
    let agent = MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()
                printfn "%s" msg
        })
    { new IProgress<string> with member _.Report(msg) = agent.Post(msg) }


let isServer = GCSettings.IsServerGC
let mode = GCSettings.LatencyMode





let progress = createThreadSafeProgress()
let cts = new CancellationTokenSource()
let maxParallel = 1 // Environment.ProcessorCount

let startTime = DateTime.Now
printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"


//let configKey = "Merge_dev" 
//let executorType = SortableTest.executorType.Merge
//let host: IRunHost = 
//    match SortableTest.SortableMergeSpecs.Configs |> Map.tryFind configKey with
//    | Some s -> SortableTest.RunHostSortableTest.createRunHost (s executorType)
//    | None -> failwithf "Config key '%s' not found." configKey

//let executor = SortableTest.SortableTestExecutor.getExecutor executorType


//let configKey = "Small_dev" 
//let executorType = evalExecutorType.Merge
//let host: IRunHost = 
//    match SorterEvalBins.SorterEvalMergeSpecs.Configs |> Map.tryFind configKey with
//    | Some s -> RunHostEvalBins.createRunHost (s executorType)
//    | None -> failwithf "Config key '%s' not found." configKey

//let executor = EvalBinsExecutor.getExecutor executorType



let configKey = "Small_dev" 
let executorType = evalExecutorType.Standard
let host: IRunHost = 
    match SorterEvalBins.SorterEvalSimpleSpecs.Configs |> Map.tryFind configKey with
    | Some s -> RunHostEvalBins.createRunHost (s executorType)
    | None -> failwithf "Config key '%s' not found." configKey

let executor = EvalBinsExecutor.getExecutor executorType




let minReplica = 0<replNumber>
let maxReplica = 1<replNumber>



async {
    printfn "Init Project: %s" %host.Run.ProjectName
    
    let! initResult = 
        ParamOps.initProjectAndRunFiles
            host.ProjectDb           
            (Some progress) 
            host.Run              
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
                host.ProjectDb.MakeQueryParamsFromRunParams 
                host.Run.RunName 
                host.AllowOverwrite 
                cts 
                (Some progress)
                host
                executor
                maxParallel

        match execResult with
        | Ok results -> printfn "Success: %d records processed." results.Length
        | Error e -> printfn "Runtime Error: %s" e

} |> Async.RunSynchronously



//async {
//    printfn "Init Run: %s" %host.Run.RunName
    
//    let! initResult = 
//        ParamOps.initProjectAndRunFiles
//            host.ProjectDb           
//            host.ProjectDb.MakeQueryParamsFromRunParams 
//            cts 
//            (Some progress) 
//            host.Run              
//            minReplica 
//            maxReplica 
//            host.AllowOverwrite 
//            host.ParamMapRefiner      
//            host.ParameterSpans

//    match initResult with
//    | Error e -> printfn "Init Failure: %s" e
//    | Ok () -> printfn "Init Success: %s" %host.Run.RunName


//} |> Async.RunSynchronously


//async {

//    let! execResult = 
//        ProjectOps.executeRuns 
//            host.ProjectDb      
//            minReplica 
//            maxReplica 
//            host.ProjectDb.MakeQueryParamsFromRunParams 
//            host.Run.RunName 
//            host.AllowOverwrite 
//            cts 
//            (Some progress) 
//            host
//            executor
//            maxParallel

//    match execResult with
//    | Ok results -> printfn "Success: %d records processed." results.Length
//    | Error e -> printfn "Runtime Error: %s" e

//} |> Async.RunSynchronously





let duration = DateTime.Now - startTime
Thread.Sleep(100)
printfn "********************************************"
printfn $"Total Time: {duration.ToString()}"
printfn "********************************************"
Console.ReadLine() |> ignore