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
open GeneSort.Dispatch.V1.SortableTest
open System.IO



let createThreadSafeProgress () =
    // 1. Create a unique file name for this application session
    let sessionTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
    let logFileName = sprintf "session_%s.log" sessionTimestamp
    
    let agent = MailboxProcessor.Start(fun inbox ->
        async {
            // 2. Open the file stream inside the agent loop
            use writer = new StreamWriter(logFileName, append = true)
            // Ensure edits flush immediately so logs are saved even if a crash happens
            writer.AutoFlush <- true 

            while true do
                let! msg = inbox.Receive()
                
                // Print to console
                printfn "%s" msg
                
                // Append to text file
                do! writer.WriteLineAsync(msg) |> Async.AwaitTask
        })
    { new IProgress<string> with member _.Report(msg) = agent.Post(msg) }



let isServer = GCSettings.IsServerGC
let mode = GCSettings.LatencyMode





let progress = createThreadSafeProgress()
let cts = new CancellationTokenSource()
let maxParallel = 4 // Environment.ProcessorCount

let startTime = DateTime.Now
printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"


//let configType = SortableTetsMergeSpecs.configType.MergeTest_Test
//let executorType = SortableTest.executorType.Merge_Gen
//let host: IRunHost = 
//    let spec = SortableTetsMergeSpecs.getConfig configType executorType
//    SortableTest.RunHostSortableTest.createRunHost spec

//let executor = SortableTest.SortableTestExecutor.getExecutor executorType


let configType = EvalBinsRandomMergeSpecs.configType.EvalBins_Merge_single
let executorType = evalExecutorType.MergeSortables
let host: IRunHost = 
    let spec = EvalBinsRandomMergeSpecs.getConfig configType executorType
    SorterEvalBins.RunHostEvalBins.createRunHost spec

let executor = EvalBinsExecutor.getExecutor executorType




//let configType = EvalBinsRandomStandardSpecs.configType.EvalBins_Standard_Test
//let executorType = evalExecutorType.StandardBins
//let host: IRunHost = 
//    let spec = EvalBinsRandomStandardSpecs.getConfig configType executorType
//    SorterEvalBins.RunHostEvalBins.createRunHost spec

//let executor = EvalBinsExecutor.getExecutor executorType


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