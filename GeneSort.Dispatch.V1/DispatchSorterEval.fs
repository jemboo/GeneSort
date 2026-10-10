namespace GeneSort.Dispatch.V1
open System
open System.IO
open FSharp.UMX
open System.Threading
open GeneSort.Dispatch.V1
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open System.Runtime
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Core


module DispatchSorterEval = 

    let private createThreadSafeProgress () =
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


    let private progress = createThreadSafeProgress()
    let private cts = new CancellationTokenSource()

    let private startTime = DateTime.Now
    printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"



    //********** SorterEval Standard **********
    //let configType = SorterEvalSpecsRs.configType.Rand_Test
    //let executorType = sorterEvalExecutorType.GenStandard
    //let host: runHost = 
    //    let spec = SorterEvalSpecsRs.getRun configType executorType
    //    SorterEvalDbs.createRunHost spec


    //********** SorterEval Merge **********
    //let configType = SorterEvalSpecsRm.configType.Rand_MergeTest_Test
    //let executorType = sorterEvalExecutorType.GenMerge
    //let host: runHost = 
    //    let spec = SorterEvalSpecsRm.getRun configType executorType
    //    SorterEvalDbs.createRunHost spec



    //********** SorterEval Prefix **********
    //let private configType = SorterEvalSpecsTestPrefix.configType.Prefix_32
    //let private executorType = sorterEvalExecutorType.GenPrefix
    //let private host: runHost = 
    //    let run = SorterEvalSpecsTestPrefix.getRun configType executorType
    //    runHost.Create run 8
    //let private executor = SorterEvalExecutor.getExecutor executorType


    let private executorType = sorterEvalExecutorType.GenPrefix
    let private host: runHost = runHost.Create (SorterEvalSpecsTestPrefix.Specs.Prefix_32 executorType) 5
    let private executor = SorterEvalExecutor.getExecutor executorType




    let private minReplica = 0<replNumber>
    let private maxReplica = 1<replNumber>


    let makeParamsAndRun() =

        async {

            printfn "Init Project: %s" %host.Run.DatabaseName
    
            let! initResult = 
                ParamOps.initRunAndParamFiles
                    host.RunDb           
                    (Some progress) 
                    host.Run              
                    minReplica 
                    maxReplica 
                    host.AllowOverwrite 
                    host.ParamMapRefiner      
                    host.Run.ParameterSpans


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


    let makeRunParams() =

        async {
            printfn "Init Run: %s" %host.Run.RunName
    
            let! initResult = 
                ParamOps.initRunAndParamFiles
                    host.RunDb           
                    (Some progress) 
                    host.Run              
                    minReplica 
                    maxReplica 
                    host.AllowOverwrite 
                    host.ParamMapRefiner      
                    host.Run.ParameterSpans

            match initResult with
            | Error e -> printfn "Init Failure: %s" e
            | Ok () -> printfn "Init Success: %s" %host.Run.RunName


        } |> Async.RunSynchronously


    let runRunParams() =

        async {

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
