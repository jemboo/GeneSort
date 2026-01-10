open System
open BenchmarkDotNet.Running

[<EntryPoint>]
let main argv =
    let summary = BenchmarkRunner.Run<SorterEvalBench2Blocks>()

    printfn "%A" summary
    Console.Read() |> ignore
    0
