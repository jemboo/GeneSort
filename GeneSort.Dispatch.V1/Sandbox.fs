namespace GeneSort.Dispatch.V1
open System
open System.Threading
open GeneSort.Dispatch.V1
open System.Runtime
open System.IO
open GeneSort.Core


module Sandbox = 

    let run()  =

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
        printfn $"**** Sandbox Engine Active: {startTime.ToString()} ****"


        let yab = CommonParams.sorterPoolSelects25_5i |> snd |> List.head
        let essD = yab |> EssData.fromString

        let qua = EssData.getSamplesInOrder essD 10000 |> Seq.toArray


        let duration = DateTime.Now - startTime
        Thread.Sleep(100)
        printfn "********************************************"
        printfn $"Total Time: {duration.ToString()}"
        printfn "********************************************"
        Console.ReadLine() |> ignore