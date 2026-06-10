module Program

open System
open FSharp.UMX
open System.Threading
open GeneSort.Dispatch.V1
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open System.Runtime
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Dispatch.V1.SortableTest
open System.IO
open GeneSort.Dispatch.V1.SorterMutate



//let createThreadSafeProgress () =
//    // 1. Create a unique file name for this application session
//    let sessionTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
//    let logFileName = sprintf @"c:\Projects\session_%s.log" sessionTimestamp
    
//    let agent = MailboxProcessor.Start(fun inbox ->
//        async {
//            // 2. Open the file stream inside the agent loop
//            use writer = new StreamWriter(logFileName, append = true)
//            // Ensure edits flush immediately so logs are saved even if a crash happens
//            writer.AutoFlush <- true 

//            while true do
//                let! msg = inbox.Receive()
                
//                // Print to console
//                printfn "%s" msg
                
//                // Append to text file
//                do! writer.WriteLineAsync(msg) |> Async.AwaitTask
//        })
//    { new IProgress<string> with member _.Report(msg) = agent.Post(msg) }

let createThreadSafeProgress () =
    let sessionTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
    let logFileName = sprintf @"c:\Projects\session_%s.log" sessionTimestamp
    
    // Explicit synchronization object to protect file access across threads
    let lockObj = obj()

    { new IProgress<string> with 
        member _.Report(msg) = 
            // Thread-safe lock ensures concurrent evaluations don't step on each other
            lock lockObj (fun () ->
                // 1. Output to console immediately
                printfn "%s" msg
                
                // 2. Open, append, and force-flush to disk blocks instantly
                // The 'use' keyword guarantees disposal and stream closure immediately after writing
                use writer = new StreamWriter(logFileName, append = true)
                writer.WriteLine(msg)
                
                // 3. Force the OS kernel to flush its internal file cache to physical storage
                writer.Flush() 
            )
    }

let isServer = GCSettings.IsServerGC
let mode = GCSettings.LatencyMode





let progress = createThreadSafeProgress()
let cts = new CancellationTokenSource()

let startTime = DateTime.Now
printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"


//let configType = SortableTestMergeSpecs.configType.Merge_Test
//let executorType = SortableTest.executorType.Generator
//let host: IRunHost = 
//    let spec = SortableTestMergeSpecs.getConfig configType executorType
//    SortableTest.SortableTestRunHost.createRunHost spec

//let executor = SortableTest.SortableTestExecutor.getExecutor executorType




//let configType = SorterEvalSpecsRandom.configType.Rand_Test
//let executorType = sorterEvalExecutorType.StageStatsReport
//let host: IRunHost = 
//    let spec = SorterEvalSpecsRandom.getConfig configType executorType
//    SorterEval.SorterEvalRunHost.createRunHost spec

//let executor = SorterEvalExecutor.getExecutor executorType




//let configType = SorterEvalSpecsRandomMerge.configType.RandMerge_Test
//let executorType = sorterEvalExecutorType.FullReport
//let host: IRunHost = 
//    let spec = SorterEvalSpecsRandomMerge.getConfig configType executorType
//    SorterEval.SorterEvalRunHost.createRunHost spec

//let executor = SorterEvalExecutor.getExecutor executorType



//let configType = SorterMutateSpecsRandom.configType.Rand_Test
//let executorType = sorterMutateExecutorType.MutantReport
//let host: IRunHost = 
//    let spec = SorterMutateSpecsRandom.getConfig configType executorType
//    SorterMutate.SorterMutateRunHost.createRunHost spec

//let executor = SorterMutateExecutor.getExecutor executorType


let configType = SorterMutateSpecsRandomMerge.configType.Rand_Test
let executorType = sorterMutateExecutorType.GenMerge
let host: IRunHost = 
    let spec = SorterMutateSpecsRandomMerge.getConfig configType executorType
    SorterMutate.SorterMutateRunHost.createRunHost spec

let executor = SorterMutateExecutor.getExecutor executorType


let minReplica = 0<replNumber>
let maxReplica = 1<replNumber>



async {
    printfn "Init Project: %s" %host.Run.DatabaseName
    
    let! initResult = 
        ParamOps.initProjectAndRunFiles
            host.RunDb           
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
                minReplica 
                maxReplica
                host.AllowOverwrite 
                cts 
                (Some progress)
                host
                executor
                host.MaxParallel

        match execResult with
        | Ok results -> printfn "Success: %d records processed." results.Length
        | Error e -> printfn "Runtime Error: %s" e

} |> Async.RunSynchronously



//async {
//    printfn "Init Run: %s" %host.Run.RunName
    
//    let! initResult = 
//        ParamOps.initProjectAndRunFiles
//            host.RunDb           
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
//                minReplica 
//                maxReplica
//                host.AllowOverwrite 
//                cts 
//                (Some progress)
//                host
//                executor
//                host.MaxParallel

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