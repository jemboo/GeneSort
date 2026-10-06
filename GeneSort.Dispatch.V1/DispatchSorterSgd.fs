namespace GeneSort.Dispatch.V1
open System
open System.IO
open System.Runtime
open System.Threading

open FSharp.UMX

open GeneSort.Dispatch.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1.SorterSgd

module DispatchSorterSgd = 

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

    let private isServer = GCSettings.IsServerGC
    let private mode = GCSettings.LatencyMode

    let private progress = createThreadSafeProgress()
    let private cts = new CancellationTokenSource()

    let private startTime = DateTime.Now
    printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"


    ////********** Msuf6SgdSpecsPrefix **********
    //let private executorType = sorterSgdExecutorType.SummaryReport
    //let private host: runHost = runHost.Create (Msuf624p3b.MutationRate.VarModR_32.Test executorType) 8

    //let private executor = SorterSgdExecutorType.getExecutor executorType
    //let private minReplica = 0<replNumber>
    //let private maxReplica = 1<replNumber>

    //********** Msuf4SgdSpecsPrefix **********
    let private executorType = sorterSgdExecutorType.SummaryReport
    let private host: runHost = runHost.Create (Msuf32p4a.MutationRate.VarModR_32.Pool_32_Test executorType) 8

    let private executor = SorterSgdExecutorType.getExecutor executorType
    let private minReplica = 0<replNumber>
    let private maxReplica = 1<replNumber>

    //********** MsrsSgdSpecsPrefix **********
    //let private executorType = sorterSgdExecutorType.GenPrefix
    //let private host: runHost = runHost.Create (Msrs32p4a.MutationRate.VarModR_64.NarrowTest executorType) 16

    //let private executor = SorterSgdExecutorType.getExecutor executorType
    //let private minReplica = 1<replNumber>
    //let private maxReplica = 5<replNumber>


    //********** MssiSgdSpecsPrefix **********
    //let private executorType = sorterSgdExecutorType.SummaryReport
    //let private host: runHost = runHost.Create (Mssi24p3b.OrthoPara.Specs64.SymForceDiff1 executorType) 8

    //let private executor = SorterSgdExecutorType.getExecutor executorType
    //let private minReplica = 0<replNumber>
    //let private maxReplica = 1<replNumber>



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


    let runRunParameters() =

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
