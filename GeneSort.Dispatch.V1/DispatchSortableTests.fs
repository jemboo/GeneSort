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
open GeneSort.Dispatch.V1.SortableTests
open System.IO
open GeneSort.Core


module DispatchSortableTests = 

    let private createThreadSafeProgress () =
        let sessionTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
        let logFolder = Path.Combine(RunHost.projectsRoot (), "Logs")
        Directory.CreateDirectory(logFolder) |> ignore
        let logFileName = Path.Combine(logFolder, sprintf "session_%s_%s.log" sessionTimestamp (Guid.NewGuid().ToString("N")))
    
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



    let private host: runHost = runHost.Create (SortableTestsSpecsPrefix.Specs.Prefix_32 ()) 1
    let private minReplica = 0<replNumber>
    let private maxReplica = 1<replNumber>

    let private initializeParameters() =
        ParamOps.initRunAndParamFiles host.RunDb (Some progress) host.Run
            minReplica maxReplica host.AllowOverwrite host.MakeRunParameters

    let private showResult result =
        match result with
        | Ok results ->
            let summary = RunResult.analyze results
            printfn "Complete: %d succeeded, %d failed or cancelled." summary.Successes summary.IssueLog.Length
            if summary.IssueLog.Length > 0 then
                failwith (String.concat Environment.NewLine summary.IssueLog)
        | Error error -> failwith error

    let makeRunParams() =
        async {
            let! result = initializeParameters()
            match result with
            | Ok () -> printfn "Parameters initialized."
            | Error error -> failwithf "Init failure: %s" error
        } |> Async.RunSynchronously

    let setupRunParams() =
        ProjectOps.executeSetupRuns minReplica maxReplica host.AllowOverwrite cts
            (Some progress) host host.MaxParallel
        |> Async.RunSynchronously |> showResult

    let runRunParams() =
        ProjectOps.executeRuns minReplica maxReplica host.AllowOverwrite cts
            (Some progress) host host.MaxParallel
        |> Async.RunSynchronously |> showResult

    /// Local phase: initialize parameters and materialize Setup dependencies only.
    let prepareRun() =
        async {
            let! initialized = initializeParameters()
            match initialized with
            | Error error -> failwithf "Init failure: %s" error
            | Ok () ->
                let! result =
                    ProjectOps.executeSetupRuns minReplica maxReplica host.AllowOverwrite cts
                        (Some progress) host host.MaxParallel
                showResult result
        } |> Async.RunSynchronously

    /// Runs a named report against saved parameters, including completed runs.
    let runReport name =
        ReportOps.executeReport name minReplica maxReplica host.AllowOverwrite cts
            (Some progress) host host.MaxParallel
        |> Async.RunSynchronously |> showResult