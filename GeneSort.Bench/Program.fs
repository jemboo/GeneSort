
namespace GeneSort.Bench
open System
open BenchmarkDotNet.Running

module Program =
    [<EntryPoint>]
    let main argv =
        let summary = BenchmarkRunner.Run<PermutationBench>()

        printfn "%A" summary
        Console.Read() |> ignore
        0
