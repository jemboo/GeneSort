module Program

open System
open System.Threading
open GeneSort.Core.Mp
open MessagePack
open GeneSort.Dispatch.V1

[<EntryPoint>]
let main argv =
    // 1. Force MessagePack setup FIRST before opening or executing any dispatch logic
    MessagePackSetup.configure ()

    let startTime = DateTime.Now
    printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"

    // 2. Call your dispatch logic strictly AFTER configuration
    //GeneSort.Dispatch.V1.DispatchSorterSgd.makeParamsAndRun()

    DispatchSorterMutate.makeParamsAndRun()

    let duration = DateTime.Now - startTime
    Thread.Sleep(100)
    printfn "********************************************"
    printfn $"Total Time: {duration.ToString()}"
    printfn "********************************************"
    Console.ReadLine() |> ignore
    0


//[<EntryPoint>]
//let main argv =
//    MessagePackSetup.configure()

//    // Test resolution explicitly to catch the offending DTO type
//    try
//        let options = MessagePackSerializer.DefaultOptions
//        let resolver = options.Resolver
        
//        // Force resolver lookup on your primary DTOs
//        printfn "Testing RunParametersDto resolver..."
//        resolver.GetFormatter<GeneSort.Project.Mp.V1.runParametersDto>() |> ignore
        
//        printfn "Testing RunDto resolver..."
//        resolver.GetFormatter<GeneSort.Project.Mp.V1.runDto>() |> ignore
        
//        printfn "All DTO resolvers resolved successfully!"
//    with ex ->
//        printfn "FAILED RESOLUTION: %s" ex.Message
//        printfn "Stack Trace:\n%s" ex.StackTrace

//    // Continue execution...
//    GeneSort.Dispatch.V1.DispatchSorterSgd.makeParamsAndRun()
//    0