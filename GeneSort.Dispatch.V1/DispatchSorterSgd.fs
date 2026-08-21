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


//********** MsceSgdSpecsRs **********
//let configType = MsceSgdSpecsRs.configType.Rand_Pool
//let executorType = sorterSgdExecutorType.GenStandard
//let host: IRunHost = 
//    let spec = MsceSgdSpecsRs.getRunHostSpec configType executorType
//    MsceSgdDbs.createRunHost spec
//let executor = MsceSgdExecutor.getExecutor executorType


//********** MsceSgdSpecsRm **********
//let configType = MsceSgdSpecsRm.configType.Rand_Test
//let executorType = sorterSgdExecutorType.GenMerge
//let host: IRunHost = 
//    let spec = MsceSgdSpecsRm.getRunHostSpec configType executorType
//    MsceSgdDbs.createRunHost spec


//********** MssiSgdSpecsRs **********
//let configType = MssiSgdSpecsRs.configType.Rand_Pool
//let executorType = sorterSgdExecutorType.FullReport
//let host: IRunHost = 
//    let spec = MssiSgdSpecsRs.getRunHostSpec configType executorType
//    MssiSgdDbs.createRunHost spec


//********** MssiSgdSpecsRm **********
//let configType = MssiSgdSpecsRm.configType.Rand_Medium
//let executorType = sorterSgdExecutorType.GenMerge
//let host: IRunHost = 
//    let spec = MssiSgdSpecsRm.getRunHostSpec configType executorType
//    MssiSgdDbs.createRunHost spec


//********** MsrsSgdSpecsRs **********
//let configType = MsrsSgdSpecsRs.configType.Rand_Pool
//let executorType = sorterSgdExecutorType.GenStandard
//let host: IRunHost = 
//    let spec = MsrsSgdSpecsRs.getRunHostSpec configType executorType
//    MsrsSgdDbs.createRunHost spec


//********** MsrsSgdSpecsRm **********
//let configType = MsrsSgdSpecsRm.configType.Rand_Test
//let executorType = sorterSgdExecutorType.GenMerge
//let host: IRunHost = 
//    let spec = MsrsSgdSpecsRm.getRunHostSpec configType executorType
//    MsrsSgdDbs.createRunHost spec


    ////********** MsrsSgdSpecsPrefix **********
    //let configType = MsrsSgdSpecsTestPrefix.configType.Test
    //let executorType = sorterSgdExecutorTypeOld.GenPrefix
    //let host: IRunHost = 
    //    let spec = MsrsSgdSpecsTestPrefix.getRunHostSpec configType executorType
    //    MsrsSgdDbs.createRunHost spec


////********** Msuf4SgdSpecsRs **********
//let configType = Msuf4SgdSpecsRs.configType.Rand_Pool
//let executorType = sorterSgdExecutorType.GenStandard
//let host: IRunHost = 
//    let spec = Msuf4SgdSpecsRs.getRunHostSpec configType executorType
//    Msuf4SgdDbs.createRunHost spec


//********** Msuf4SgdSpecsRm **********
//let configType = Msuf4SgdSpecsRm.configType.Rand_Test
//let executorType = sorterSgdExecutorType.SnapshotReport
//let host: IRunHost = 
//    let spec = Msuf4SgdSpecsRm.getRunHostSpec configType executorType
//    Msuf4SgdDbs.createRunHost spec


//********** Msuf4SgdSpecsPrefix **********
//let configType = Msuf4SgdSpecsTestPrefix.configType.Rand_Test
//let executorType = sorterSgdExecutorType.GenPrefix
//let host: IRunHost = 
//    let spec = Msuf4SgdSpecsTestPrefix.getRunHostSpec configType executorType
//    Msuf4SgdDbs.createRunHost spec



    ////********** Msrs24p3a **********
    let private executorType = sorterSgdExecutorType.GenPrefix
    let private host: IRunHost = Msrs24p3a.PoolSzComp.createRunHost (Msrs24p3a.PoolSzComp.Specs.PoolSz_1n512 executorType)

    let private executor = SorterSgdExecutorType.getExecutor executorType
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