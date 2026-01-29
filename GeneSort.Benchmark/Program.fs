open System
open BenchmarkDotNet.Running

[<EntryPoint>]
let main argv =
    let summary = BenchmarkRunner.Run<FullBoolEvalBench>()

    printfn "%A" summary
    Console.Read() |> ignore
    0


    //SIMD (Vectorization) via System.Runtime.Intrinsics

    //dotnet run -c Release --project GeneSort.Benchmark/GeneSort.Benchmark.fsproj