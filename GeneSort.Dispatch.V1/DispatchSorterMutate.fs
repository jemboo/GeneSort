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
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.SorterMutate.Msce
open GeneSort.Dispatch.V1.SorterMutate.Mssi
open GeneSort.Dispatch.V1.SorterMutate.Msrs
open GeneSort.Dispatch.V1.SorterMutate.Msuf4



module DispatchSorterMutate =


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




    //********** MsceMutateSpecsRs **********
    //let configType = MsceMutateSpecsRs.configType.Rand_Test
    //let executorType = sorterMutateExecutorType.GenStandard
    //let host: IRunHost = 
    //    let spec = MsceMutateSpecsRs.getRunHostSpec configType executorType
    //    MsceMutateDbs.createRunHost spec


    //********** MsceMutateSpecsRm **********
    //let configType = MsceMutateSpecsRm.configType.Rand_Test
    //let executorType = sorterMutateExecutorType.MergeReport
    //let host: IRunHost = 
    //    let spec = MsceMutateSpecsRm.getRunHostSpec configType executorType
    //    MsceMutateDbs.createRunHost spec


    //********** MssiMutateSpecsRs **********
    //let configType = MssiMutateSpecsRs.configType.Rand_Test
    //let executorType = sorterMutateExecutorType.StandardReport
    //let host: IRunHost = 
    //    let spec = MssiMutateSpecsRs.getRunHostSpec configType executorType
    //    MssiMutateDbs.createRunHost spec


    //********** MssiMutateSpecsRm **********
    let configType = MssiMutateSpecsRm.configType.Rand_Test
    let executorType = sorterMutateExecutorType.MergeReport
    let host: IRunHost = 
        let spec = MssiMutateSpecsRm.getRunHostSpec configType executorType
        MssiMutateDbs.createRunHost spec


    //********** MsrsMutateSpecsRs **********
    //let configType = MsrsMutateSpecsRs.configType.Rand_Test
    //let executorType = sorterMutateExecutorType.GenStandard
    //let host: IRunHost = 
    //    let spec = MsrsMutateSpecsRs.getRunHostSpec configType executorType
    //    MsrsMutateDbs.createRunHost spec


    //********** MsrsMutateSpecsRm **********
    //let configType = MsrsMutateSpecsRm.configType.Rand_Test
    //let executorType = sorterMutateExecutorType.MutantReport
    //let host: IRunHost = 
    //    let spec = MsrsMutateSpecsRm.getRunHostSpec configType executorType
    //    MsrsMutateDbs.createRunHost spec


    //********** Msuf4MutateSpecsRs **********
    //let configType = Msuf4MutateSpecsRs.configType.Rand_Test
    //let executorType = sorterMutateExecutorType.StandardReport
    //let host: IRunHost = 
    //    let spec = Msuf4MutateSpecsRs.getRunHostSpec configType executorType
    //    Msuf4MutateDbs.createRunHost spec


    //********** Msuf4MutateSpecsRm **********
    //let configType = Msuf4MutateSpecsRm.configType.Rand_Test
    //let executorType = sorterMutateExecutorType.GenMerge
    //let host: IRunHost = 
    //    let spec = Msuf4MutateSpecsRm.getRunHostSpec configType executorType
    //    Msuf4MutateDbs.createRunHost spec



    let executor = Msuf4MutateExecutor.getExecutor executorType
    let minReplica = 0<replNumber>
    let maxReplica = 1<replNumber>



    let runBoth() =

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


    let MakeRunParams() =

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


    let runEm() =

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