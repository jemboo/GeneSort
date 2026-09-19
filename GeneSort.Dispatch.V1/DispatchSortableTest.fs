namespace GeneSort.Dispatch.V1
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
open GeneSort.Core


module DispatchSortableTest = 

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



    //********** SortableTest Merge **********
    //let configType = SortableTestSpecsMerge.configType.Merge_Test
    //let executorType = SortableTest.sortableTestExecutorType.GenMerge
    //let host: IRunHost = 
    //    let spec = SortableTestSpecsMerge.getRunHostSpec configType executorType
    //    SortableTestDbs.createRunHost spec


    //********** SortableTest Prefix **********
    let private configType = SortableTestSpecsPrefix.configType.Prefix_24s
    let private executorType = SortableTest.sortableTestExecutorType.GenPrefix
    let private host: IRunHost = 
        let spec = SortableTestSpecsPrefix.getRunHostSpec configType executorType
        SortableTestDbs.createRunHost spec



    let private executor = SortableTestExecutor.getExecutor executorType
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
                    host.ParameterSpans

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