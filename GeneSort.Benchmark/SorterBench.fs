namespace global

open BenchmarkDotNet.Attributes
open System.Security.Cryptography
open System


//|          Method |   size |         Mean |        Error |       StdDev | Allocated |
//|---------------- |------- |-------------:|-------------:|-------------:|----------:|
//| arrayProductInt |     10 |     13.53 ns |     0.143 ns |     0.134 ns |         - |
//|    arrayProduct |     10 |     12.79 ns |     0.287 ns |     0.269 ns |         - |
//| arrayProductInt |    100 |     89.23 ns |     1.315 ns |     1.098 ns |         - |
//|    arrayProduct |    100 |     93.98 ns |     0.660 ns |     0.585 ns |         - |
//| arrayProductInt |   1000 |    760.74 ns |     8.464 ns |     7.917 ns |         - |
//|    arrayProduct |   1000 |    768.17 ns |     7.140 ns |     6.679 ns |         - |
//| arrayProductInt |  10000 |  7,553.64 ns |    57.999 ns |    51.414 ns |         - |
//|    arrayProduct |  10000 |  7,713.10 ns |   151.992 ns |   162.630 ns |         - |
//| arrayProductInt | 100000 | 75,923.11 ns | 1,458.919 ns | 1,996.985 ns |         - |
//|    arrayProduct | 100000 | 77,145.02 ns |   619.630 ns |   549.286 ns |         - |

[<MemoryDiagnoser>]
type ArrayCompBench() =

    [<Params(10, 100, 1000, 10000, 100000)>]
    member val size = 0 with get, set

    member val arrayA = [||] with get, set
    member val arrayB = [||] with get, set
    member val arrayC = [||] with get, set

    [<GlobalSetup>]
    member this.Setup() =
        this.arrayA <- Array.init this.size (fun dex -> (dex + 2) % this.size)
        this.arrayB <- Array.init this.size (fun dex -> (dex + 7) % this.size)
        this.arrayC <- Array.zeroCreate this.size


    //[<Benchmark>]
    //member this.createPermSafe() =
    //    let yab = Permutation


    //[<Benchmark>]
    //member this.createPermUnsafe() =
    //    CollectionOps.arrayProduct this.arrayA this.arrayB this.arrayC
    //    |> ignore
