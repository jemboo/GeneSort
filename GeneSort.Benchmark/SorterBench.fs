namespace global

open BenchmarkDotNet.Attributes
open System
open GeneSort.Core
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter


//| Method           | size   | Mean           | Error        | StdDev       | Gen0     | Gen1     | Gen2     | Allocated |
//|----------------- |------- |---------------:|-------------:|-------------:|---------:|---------:|---------:|----------:|
//| createPermSafe   | 10     |       374.3 ns |      7.45 ns |     18.00 ns |   0.0286 |        - |        - |     480 B |
//| createPermUnsafe | 10     |       310.3 ns |      7.52 ns |     21.70 ns |   0.0210 |        - |        - |     352 B |
//| createPermSafe   | 100    |       985.1 ns |     27.57 ns |     81.28 ns |   0.0916 |        - |        - |    1560 B |
//| createPermUnsafe | 100    |       508.9 ns |     15.50 ns |     45.44 ns |   0.0420 |        - |        - |     712 B |
//| createPermSafe   | 1000   |     7,709.3 ns |    152.71 ns |    425.69 ns |   0.7324 |        - |        - |   12360 B |
//| createPermUnsafe | 1000   |     2,111.6 ns |     40.30 ns |     33.66 ns |   0.2556 |   0.0038 |        - |    4312 B |
//| createPermSafe   | 10000  |    83,858.5 ns |  1,668.36 ns |  1,638.55 ns |   7.0801 |   0.6104 |        - |  120362 B |
//| createPermUnsafe | 10000  |    18,781.2 ns |    373.80 ns |    654.67 ns |   2.3804 |        - |        - |   40313 B |
//| createPermSafe   | 100000 | 1,084,067.6 ns | 21,191.29 ns | 41,331.99 ns | 330.0781 | 330.0781 | 330.0781 | 1200535 B |
//| createPermUnsafe | 100000 |   175,756.3 ns |  1,150.11 ns |  1,019.54 ns | 110.8398 | 110.8398 | 110.8398 |  400374 B |

[<MemoryDiagnoser>]
type PermutationBench() =

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


    [<Benchmark>]
    member this.createPermSafe() =
        let yab = permutation.create this.arrayA
        yab.Order


    [<Benchmark>]
    member this.createPermUnsafe() =
        let yab = permutation.createUnsafe this.arrayA
        yab.Order





//SortingWidth    Dimension       EdgeLength      level   LevelSet        LevelSetVV
//12                3               4               6       19              5
//24                3               8               12      61              13
//36                3               12              18      127             25
//48                3               16              24      217             41
//72                3               24              36      469             85
//96                3               32              48      817             145
//16                4               4               8       85              8
//32                4               8               16      489             33
//48                4               12              24      1469            86
//64                4               16              32      3281            177
//96                4               24              48      10425           519
//128               4               32              64      23969           1143
//24                6               4               12      1751            18
//48                6               8               24      32661           151
//72                6               12              36      204763          676
//96                6               16              48      782153          2137
//144               6               24              72      5375005         11963
//192               6               32              96      21533457        42955
//32                8               4               16      38165           33
//64                8               8               32      2306025         526
//96                8               12              48      30162301        3788
//128               8               16              64      197018321       17575





[<MemoryDiagnoser>]
type GetLevelSetBench() =

    [<Params(2, 4, 8)>]
    member val dim = 0 with get, set
    
    [<Params(2, 4)>]
    member val edgeLength = 0 with get, set

    // We can compute a dynamic lattice level in Setup 
    // or just set a default. Usually level = (dim * edge) / 2 is the "densest" part.
    member val latticeLevel = 0 with get, set

    [<GlobalSetup>]
    member this.Setup() =
        // Calculating a mid-point level for the most intensive test
        this.latticeLevel <- (this.dim * this.edgeLength) / 2

    [<Benchmark(Baseline = true)>]
    member this.getLevelSet() =
        let d = UMX.tag<latticeDimension> this.dim
        let e = UMX.tag<latticeDistance> this.edgeLength
        let l = UMX.tag<latticeDistance> this.latticeLevel
        
        LatticePoint.getLevelSet d l e
        |> Seq.iter ignore // Force evaluation of the sequence

    [<Benchmark>]
    member this.getLevelSetNew() =
        let d = UMX.tag<latticeDimension> this.dim
        let e = UMX.tag<latticeDistance> this.edgeLength
        let l = UMX.tag<latticeDistance> this.latticeLevel
        
        LatticePoint.getLevelSet d l e
        |> Seq.iter ignore // Force evaluation of the ResizeArray





//| Method       | mergeDimension | sortingWidth | sorterCount | ceLength | chunkSize | collectResults | Mean     | Error    | StdDev   | Ratio | Gen0       | Gen1       | Gen2      | Allocated  | Alloc Ratio |
//|------------- |--------------- |------------- |------------ |--------- |---------- |--------------- |---------:|---------:|---------:|------:|-----------:|-----------:|----------:|-----------:|------------:|
//| evalStandard | 6              | 48           | 100         | 1000     | 0         | False          |  2.097 s | 0.0063 s | 0.0042 s |  1.00 |          - |          - |         - |    4.87 MB |        1.00 |
//|              |                |              |             |          |           |                |          |          |          |       |            |            |           |            |             |
//| evalStandard | 6              | 48           | 100         | 1000     | 0         | True           |  3.079 s | 0.0420 s | 0.0278 s |  1.00 | 19000.0000 | 18000.0000 | 5000.0000 | 1066.59 MB |        1.00 |
//|              |                |              |             |          |           |                |          |          |          |       |            |            |           |            |             |
//| evalStandard | 6              | 48           | 100         | 10000    | 0         | False          | 18.968 s | 0.1394 s | 0.0922 s |  1.00 |          - |          - |         - |   17.53 MB |        1.00 |
//|              |                |              |             |          |           |                |          |          |          |       |            |            |           |            |             |
//| evalStandard | 6              | 48           | 100         | 10000    | 0         | True           | 13.958 s | 0.0171 s | 0.0101 s |  1.00 |          - |          - |         - |   23.49 MB |        1.00 |
//|              |                |              |             |          |           |                |          |          |          |       |            |            |           |            |             |
//| evalStandard | 6              | 48           | 100         | 30000    | 0         | False          | 56.824 s | 0.2313 s | 0.1530 s |  1.00 |          - |          - |         - |   40.41 MB |        1.00 |
//|              |                |              |             |          |           |                |          |          |          |       |            |            |           |            |             |
//| evalStandard | 6              | 48           | 100         | 30000    | 0         | True           | 36.680 s | 0.6345 s | 0.3319 s |  1.00 |          - |          - |         - |   40.43 MB |        1.00 |


//| Method        | mergeDimension | sortingWidth | sorterCount | ceLength | chunkSize | collectResults | Mean        | Error      | StdDev     | Gen0        | Gen1        | Gen2        | Allocated   |
//|-------------- |--------------- |------------- |------------ |--------- |---------- |--------------- |------------:|-----------:|-----------:|------------:|------------:|------------:|------------:|
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 1         | False          |    59.21 ms |   1.564 ms |   1.035 ms |  16666.6667 |    111.1111 |           - |   547.93 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 1         | True           |    56.01 ms |   4.105 ms |   2.715 ms |   1333.3333 |   1000.0000 |    333.3333 |    64.13 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 2         | False          |    37.25 ms |   0.256 ms |   0.152 ms |   8428.5714 |     71.4286 |           - |    352.2 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 2         | True           |    60.82 ms |   2.156 ms |   1.283 ms |   2000.0000 |   1875.0000 |    625.0000 |    71.89 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 4         | False          |    39.66 ms |   0.284 ms |   0.188 ms |   5846.1538 |     76.9231 |           - |   254.33 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 4         | True           |    60.08 ms |   3.180 ms |   2.104 ms |   1750.0000 |   1625.0000 |    500.0000 |    63.91 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 1         | False          |   311.28 ms |   5.113 ms |   3.382 ms |  84000.0000 |   1000.0000 |           - |  4059.03 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 1         | True           |   236.62 ms |   6.559 ms |   3.903 ms |   6000.0000 |   5500.0000 |   1000.0000 |   258.81 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 2         | False          |   246.69 ms |   6.174 ms |   4.083 ms |  44000.0000 |   1000.0000 |           - |  2112.38 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 2         | True           |   235.69 ms |   5.424 ms |   3.588 ms |   6000.0000 |   5500.0000 |   1000.0000 |   253.11 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 4         | False          |   225.68 ms |   2.042 ms |   1.350 ms |  23666.6667 |   1000.0000 |           - |  1139.01 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 4         | True           |   244.48 ms |   5.921 ms |   3.916 ms |   6000.0000 |   5500.0000 |   1000.0000 |   255.06 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 1         | False          | 1,435.99 ms | 157.462 ms | 104.152 ms | 210000.0000 | 207000.0000 | 207000.0000 | 11856.24 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 1         | True           |   736.19 ms |  18.306 ms |  12.108 ms |   3000.0000 |   3000.0000 |   3000.0000 |   647.96 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 2         | False          |   869.40 ms |  24.480 ms |  14.568 ms | 118000.0000 | 115000.0000 | 115000.0000 |   6018.7 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 2         | True           |   702.80 ms |  14.752 ms |   9.757 ms |   3000.0000 |   3000.0000 |   3000.0000 |  1060.37 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 4         | False          |   766.06 ms |  28.751 ms |  19.017 ms |  35000.0000 |  32000.0000 |  32000.0000 |  3099.79 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 4         | True           |   736.13 ms |  32.600 ms |  21.563 ms |   3000.0000 |   3000.0000 |   3000.0000 |   636.57 MB |


//| Method        | mergeDimension | sortingWidth | sorterCount | ceLength | chunkSize | collectResults | Mean        | Error     | StdDev    | Gen0        | Gen1        | Gen2        | Allocated  |
//|-------------- |--------------- |------------- |------------ |--------- |---------- |--------------- |------------:|----------:|----------:|------------:|------------:|------------:|-----------:|
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 1         | False          |    59.61 ms |  3.113 ms |  2.059 ms |  16666.6667 |    111.1111 |           - |  547.92 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 1         | True           |    59.90 ms |  2.059 ms |  1.362 ms |   1750.0000 |   1625.0000 |    500.0000 |   64.36 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 2         | False          |    37.14 ms |  0.583 ms |  0.386 ms |   8428.5714 |     71.4286 |           - |  352.19 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 2         | True           |    59.33 ms |  1.649 ms |  1.090 ms |   1666.6667 |   1555.5556 |    444.4444 |   67.43 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 4         | False          |    38.28 ms |  0.295 ms |  0.195 ms |   5857.1429 |     71.4286 |           - |  254.32 MB |
//| evalUint8v256 | 6              | 48           | 100         | 1000     | 4         | True           |    58.50 ms |  2.503 ms |  1.655 ms |   1888.8889 |   1777.7778 |    555.5556 |      70 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 1         | False          |   293.64 ms |  6.770 ms |  4.478 ms |  84500.0000 |   1000.0000 |           - | 4059.05 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 1         | True           |   240.36 ms |  7.942 ms |  5.253 ms |   6000.0000 |   5500.0000 |   1000.0000 |  254.99 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 2         | False          |   242.07 ms |  4.580 ms |  2.726 ms |  44000.0000 |   1000.0000 |           - | 2112.38 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 2         | True           |   235.83 ms |  3.957 ms |  2.355 ms |   6000.0000 |   5500.0000 |   1000.0000 |  254.99 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 4         | False          |   225.42 ms |  3.435 ms |  2.272 ms |  23500.0000 |   1000.0000 |           - | 1139.04 MB |
//| evalUint8v256 | 6              | 48           | 100         | 10000    | 4         | True           |   248.10 ms |  8.730 ms |  5.195 ms |   4333.3333 |   4000.0000 |   1000.0000 |  174.73 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 1         | False          | 1,367.76 ms | 36.589 ms | 21.774 ms | 197000.0000 | 194000.0000 | 194000.0000 | 11856.3 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 1         | True           |   682.49 ms | 14.913 ms |  9.864 ms |   3000.0000 |   3000.0000 |   3000.0000 |  693.93 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 2         | False          |   855.75 ms | 22.081 ms | 14.605 ms | 120000.0000 | 117000.0000 | 117000.0000 | 6018.74 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 2         | True           |   684.02 ms |  7.062 ms |  4.671 ms |   3000.0000 |   3000.0000 |   3000.0000 |  625.23 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 4         | False          |   752.50 ms | 34.540 ms | 22.846 ms |  35000.0000 |  32000.0000 |  32000.0000 | 3099.81 MB |
//| evalUint8v256 | 6              | 48           | 100         | 30000    | 4         | True           |   697.42 ms | 24.470 ms | 14.562 ms |   3000.0000 |   3000.0000 |   3000.0000 |  716.79 MB |


//| Method        | mergeDimension | sortingWidth | sorterCount | ceLength | chunkSize | collectResults | Mean      | Error     | StdDev    | Gen0      | Gen1      | Gen2      | Allocated |
//|-------------- |--------------- |------------- |------------ |--------- |---------- |--------------- |----------:|----------:|----------:|----------:|----------:|----------:|----------:|
//| evalUint8v256 | 4              | 32           | 100         | 1000     | 1         | False          |  2.748 ms | 0.0198 ms | 0.0131 ms |  308.5938 |  296.8750 |         - |  10.17 MB |
//| evalUint8v256 | 4              | 32           | 100         | 1000     | 1         | True           |  2.951 ms | 0.1086 ms | 0.0718 ms |  394.5313 |  371.0938 |         - |  18.63 MB |
//| evalUint8v256 | 4              | 32           | 100         | 1000     | 2         | False          |  3.069 ms | 0.0153 ms | 0.0101 ms |  179.6875 |  167.9688 |         - |   7.09 MB |
//| evalUint8v256 | 4              | 32           | 100         | 1000     | 2         | True           |  2.955 ms | 0.1654 ms | 0.1094 ms |  398.4375 |  378.9063 |         - |  18.71 MB |
//| evalUint8v256 | 4              | 32           | 100         | 1000     | 4         | False          |  5.750 ms | 0.0389 ms | 0.0257 ms |  117.1875 |  101.5625 |         - |   5.55 MB |
//| evalUint8v256 | 4              | 32           | 100         | 1000     | 4         | True           |  3.020 ms | 0.0717 ms | 0.0474 ms |  375.0000 |  339.8438 |         - |  17.65 MB |
//| evalUint8v256 | 4              | 32           | 100         | 10000    | 1         | False          |  8.593 ms | 0.2296 ms | 0.1519 ms | 1546.8750 |  734.3750 |  125.0000 |  68.74 MB |
//| evalUint8v256 | 4              | 32           | 100         | 10000    | 1         | True           | 32.731 ms | 3.6862 ms | 2.4382 ms | 5066.6667 | 5000.0000 | 1333.3333 | 179.65 MB |
//| evalUint8v256 | 4              | 32           | 100         | 10000    | 2         | False          |  9.599 ms | 0.1754 ms | 0.1160 ms |  796.8750 |  718.7500 |         - |  38.19 MB |
//| evalUint8v256 | 4              | 32           | 100         | 10000    | 2         | True           | 32.259 ms | 1.8848 ms | 1.2467 ms | 5214.2857 | 5142.8571 | 1357.1429 | 185.89 MB |
//| evalUint8v256 | 4              | 32           | 100         | 10000    | 4         | False          | 14.106 ms | 0.1185 ms | 0.0783 ms |  468.7500 |  421.8750 |         - |  22.92 MB |
//| evalUint8v256 | 4              | 32           | 100         | 10000    | 4         | True           | 35.011 ms | 2.4271 ms | 1.4443 ms | 4933.3333 | 4866.6667 | 1333.3333 | 173.01 MB |
//| evalUint8v256 | 4              | 32           | 100         | 30000    | 1         | False          | 24.755 ms | 2.3717 ms | 1.5687 ms | 5656.2500 | 5593.7500 | 5593.7500 | 198.38 MB |
//| evalUint8v256 | 4              | 32           | 100         | 30000    | 1         | True           | 56.892 ms | 3.5056 ms | 2.3187 ms | 1400.0000 | 1400.0000 | 1400.0000 | 515.53 MB |
//| evalUint8v256 | 4              | 32           | 100         | 30000    | 2         | False          | 25.624 ms | 0.9328 ms | 0.6170 ms | 3156.2500 | 3093.7500 | 3093.7500 | 106.78 MB |
//| evalUint8v256 | 4              | 32           | 100         | 30000    | 2         | True           | 67.974 ms | 5.7662 ms | 3.8140 ms | 1111.1111 | 1111.1111 | 1111.1111 | 561.36 MB |
//| evalUint8v256 | 4              | 32           | 100         | 30000    | 4         | False          | 36.249 ms | 0.9466 ms | 0.5633 ms | 1857.1429 | 1785.7143 | 1785.7143 |  60.99 MB |
//| evalUint8v256 | 4              | 32           | 100         | 30000    | 4         | True           | 77.951 ms | 6.6942 ms | 4.4278 ms | 1571.4286 | 1571.4286 | 1571.4286 | 517.14 MB |





[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 10)>]
type SorterEvalBench() =

    [<Params(4)>]
    member val mergeDimension = 0 with get, set
    
    [<Params(32)>]
    member val sortingWidth = 0 with get, set

    [<Params(100)>]
    member val sorterCount = 0 with get, set

    [<Params(1000, 10000, 30000)>]
    member val ceLength = 0 with get, set

    [<Params(1, 2, 4)>]
    member val chunkSize = 0 with get, set

    [<Params(true, false)>]
    member val collectResults = false with get, set


    member val mergeFillType = mergeFillType.NoFill

    member val id = Guid.NewGuid() with get, set

    member val sortableIntTests = sortableIntTest.Empty |> sortableTest.Ints with get, set
    member val sortablePackedIntTests = sortableIntTest.Empty |> sortableTest.Ints with get, set
    member val sortableUint8v256Test = sortableUint8v256Test.Empty |> sortableTest.Uint8v256 with get, set

    member val ceBlocks = [||] with get, set

    [<GlobalSetup>]
    member this.Setup() =

        let intArrays = SortableIntArray.getMergeTestCases 
                            (this.sortingWidth |> UMX.tag<sortingWidth>)
                            (this.mergeDimension |> UMX.tag<mergeDimension>)
                            this.mergeFillType

        this.sortableIntTests <- sortableIntTest.create
                                        (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        intArrays
                                  |> sortableTest.Ints


        this.sortablePackedIntTests <- packedSortableIntTests.create
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        intArrays
                                  |> sortableTest.PackedInts

        this.sortableUint8v256Test <- SortableUint8v256Test.fromIntArrays
                                        (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        intArrays
                                  |> sortableTest.Uint8v256

        
        let randomType = rngType.Lcg

        let firstIndex = 0 |> UMX.tag<sorterCount>

        let smm =
             MsceRandGen.create 
                        randomType 
                        (this.sortingWidth |> UMX.tag<sortingWidth>) 
                        true 
                        (this.ceLength |> UMX.tag<ceLength>)
                        |> sorterModelMaker.SmmMsceRandGen

        let sorterModelSetMaker = sorterModelSetMaker.create smm firstIndex (this.sorterCount |> UMX.tag<sorterCount>)
        let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
        let sorterSet = SorterModelSet.makeSorterSet sorterModelSet
        this.ceBlocks <- sorterSet.Sorters |> Array.map (CeBlock.fromSorter)


    //[<Benchmark(Baseline = true)>]
    //member this.evalStandard() =
    //   let ceBlockEval = CeBlockOps.evalWithSorterTests this.sortableIntTests this.ceBlocks this.collectResults None
    //   ceBlockEval


    [<Benchmark>]
    member this.evalUint8v256() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests 
                            this.sortableUint8v256Test 
                            this.ceBlocks 
                            this.collectResults 
                            (Some this.chunkSize)
       ceBlockEval


[<MemoryDiagnoser>]
type SorterEvalBench2Blocks() =

    [<Params(2, 4, 8)>]
    member val mergeDimension = 0 with get, set
    
    [<Params(32, 64)>]
    member val sortingWidth = 0 with get, set

    member val mergeFillType = mergeFillType.NoFill

    member val id = Guid.NewGuid() with get, set

    member val sortableIntTests = sortableIntTest.Empty |> sortableTest.Ints with get, set

    member val sortablePackedIntTests = sortableIntTest.Empty |> sortableTest.Ints with get, set

    member val ceBlock1 = ceBlock.Empty with get, set

    member val ceBlock2 = ceBlock.Empty with get, set

    [<GlobalSetup>]
    member this.Setup() =
        let intArrays = SortableIntArray.getMergeTestCases 
                            (this.sortingWidth |> UMX.tag<sortingWidth>)
                            (this.mergeDimension |> UMX.tag<mergeDimension>)
                            this.mergeFillType

        this.sortableIntTests <- sortableIntTest.create
                                        (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        intArrays
                                  |> sortableTest.Ints

        this.sortablePackedIntTests <- packedSortableIntTests.create
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        intArrays
                                  |> sortableTest.PackedInts

        let bitonics = BitonicSorter.bitonicSort1
                            (this.sortingWidth |> UMX.tag<sortingWidth>)
        let halfLength = bitonics.Length / 2
        let firstHalf = bitonics |> Array.take(halfLength) |> Array.concat
        let secondHalf = bitonics |> Array.skip(halfLength) |> Array.take(halfLength) |> Array.concat
        let blockMaker = ceBlock.create (Guid.NewGuid() |> UMX.tag<ceBlockId>) (this.sortingWidth |> UMX.tag<sortingWidth>)
        this.ceBlock1 <- blockMaker firstHalf
        this.ceBlock2 <- blockMaker secondHalf


    [<Benchmark(Baseline = true)>]
    member this.evalBranchy() =
       let ceBlockEval1 = CeBlockOps.evalWithSorterTest this.sortableIntTests this.ceBlock1
       let ceBlockEval2 = CeBlockOps.evalWithSorterTest ceBlockEval1.SortableTest.Value this.ceBlock2
       ceBlockEval2










//| Method         | mergeDimension | sortingWidth | Mean             | Error          | StdDev          | Ratio | RatioSD | Gen0       | Gen1       | Gen2      | Allocated    | Alloc Ratio |
//|--------------- |--------------- |------------- |-----------------:|---------------:|----------------:|------:|--------:|-----------:|-----------:|----------:|-------------:|------------:|
//| evalBranchy    | 2              | 32           |         5.192 us |      0.1022 us |       0.1293 us |  1.00 |    0.03 |     0.4883 |     0.0076 |         - |      8.05 KB |        1.00 |
//| evalBranchless | 2              | 32           |         9.218 us |      0.1711 us |       0.1681 us |  1.78 |    0.05 |     0.4883 |          - |         - |      8.05 KB |        1.00 |
//|                |                |              |                  |                |                 |       |         |            |            |           |              |             |
//| evalBranchy    | 2              | 64           |        22.467 us |      0.2795 us |       0.2615 us |  1.00 |    0.02 |     1.2512 |     0.0610 |         - |     20.93 KB |        1.00 |
//| evalBranchless | 2              | 64           |        43.468 us |      0.8597 us |       0.8042 us |  1.93 |    0.04 |     1.2207 |          - |         - |     20.93 KB |        1.00 |
//|                |                |              |                  |                |                 |       |         |            |            |           |              |             |
//| evalBranchy    | 4              | 32           |       162.766 us |      3.2547 us |       3.4825 us |  1.00 |    0.03 |     9.0332 |     2.4414 |         - |     151.3 KB |        1.00 |
//| evalBranchless | 4              | 32           |       273.127 us |      5.3388 us |       7.3079 us |  1.68 |    0.06 |     8.7891 |     1.9531 |         - |     151.3 KB |        1.00 |
//|                |                |              |                  |                |                 |       |         |            |            |           |              |             |
//| evalBranchy    | 4              | 64           |     2,777.822 us |     54.4455 us |      50.9283 us |  1.00 |    0.03 |    93.7500 |    89.8438 |   31.2500 |   1407.58 KB |        1.00 |
//| evalBranchless | 4              | 64           |     4,825.695 us |     95.4907 us |     139.9690 us |  1.74 |    0.06 |    93.7500 |    85.9375 |   31.2500 |   1407.56 KB |        1.00 |
//|                |                |              |                  |                |                 |       |         |            |            |           |              |             |
//| evalBranchy    | 8              | 32           |    22,730.048 us |    447.2844 us |     747.3117 us |  1.00 |    0.05 |   968.7500 |   937.5000 |  562.5000 |   11587.3 KB |        1.00 |
//| evalBranchless | 8              | 32           |    30,434.105 us |    600.3424 us |   1,067.1081 us |  1.34 |    0.06 |   875.0000 |   812.5000 |  500.0000 |  11587.19 KB |        1.00 |
//|                |                |              |                  |                |                 |       |         |            |            |           |              |             |
//| evalBranchy    | 8              | 64           | 2,649,137.090 us | 51,371.1999 us | 104,937.7562 us |  1.00 |    0.06 | 42000.0000 | 40000.0000 | 1000.0000 | 984724.84 KB |        1.00 |
//| evalBranchless | 8              | 64           | 3,990,541.733 us | 79,295.3638 us |  84,845.1510 us |  1.51 |    0.07 | 42000.0000 | 40000.0000 | 1000.0000 | 984724.84 KB |        1.00 |


//| Method        | mergeDimension | sortingWidth | Mean             | Error          | StdDev         | Ratio | RatioSD | Gen0       | Gen1       | Gen2      | Allocated    | Alloc Ratio |
//|-------------- |--------------- |------------- |-----------------:|---------------:|---------------:|------:|--------:|-----------:|-----------:|----------:|-------------:|------------:|
//| evalBranchy   | 2              | 32           |         5.134 us |      0.1022 us |      0.1707 us |  1.00 |    0.05 |     0.4883 |     0.0076 |         - |      8.05 KB |        1.00 |
//| evalOptimized | 2              | 32           |         4.360 us |      0.0862 us |      0.0764 us |  0.85 |    0.03 |     0.5722 |          - |         - |      9.47 KB |        1.18 |
//|               |                |              |                  |                |                |       |         |            |            |           |              |             |
//| evalBranchy   | 2              | 64           |        22.233 us |      0.2810 us |      0.2629 us |  1.00 |    0.02 |     1.2512 |     0.0610 |         - |     20.93 KB |        1.00 |
//| evalOptimized | 2              | 64           |        18.265 us |      0.1906 us |      0.1690 us |  0.82 |    0.01 |     1.5564 |     0.0916 |         - |     25.59 KB |        1.22 |
//|               |                |              |                  |                |                |       |         |            |            |           |              |             |
//| evalBranchy   | 4              | 32           |       157.004 us |      2.4636 us |      2.3044 us |  1.00 |    0.02 |     9.0332 |     2.4414 |         - |     151.3 KB |        1.00 |
//| evalOptimized | 4              | 32           |       133.065 us |      2.6387 us |      3.7843 us |  0.85 |    0.03 |    10.4980 |     2.6855 |         - |    171.86 KB |        1.14 |
//|               |                |              |                  |                |                |       |         |            |            |           |              |             |
//| evalBranchy   | 4              | 64           |     2,721.565 us |     53.9424 us |     64.2146 us |  1.00 |    0.03 |    93.7500 |    89.8438 |   31.2500 |   1407.56 KB |        1.00 |
//| evalOptimized | 4              | 64           |     2,119.772 us |     40.4159 us |     43.2446 us |  0.78 |    0.02 |    78.1250 |    74.2188 |   39.0625 |   1410.43 KB |        1.00 |
//|               |                |              |                  |                |                |       |         |            |            |           |              |             |
//| evalBranchy   | 8              | 32           |    22,748.909 us |    453.8329 us |    504.4340 us |  1.00 |    0.03 |   875.0000 |   843.7500 |  468.7500 |  11587.17 KB |        1.00 |
//| evalOptimized | 8              | 32           |    20,804.560 us |    407.1794 us |    583.9646 us |  0.91 |    0.03 |  1062.5000 |  1031.2500 |  656.2500 |  10725.05 KB |        0.93 |
//|               |                |              |                  |                |                |       |         |            |            |           |              |             |
//| evalBranchy   | 8              | 64           | 2,488,788.783 us | 41,806.2696 us | 54,359.9640 us |  1.00 |    0.03 | 42000.0000 | 40000.0000 | 1000.0000 | 984724.84 KB |        1.00 |
//| evalOptimized | 8              | 64           | 2,359,207.631 us | 41,158.3683 us | 34,369.1016 us |  0.95 |    0.02 | 42000.0000 | 41000.0000 | 1000.0000 | 988031.61 KB |        1.00 |


//| Method      | mergeDimension | sortingWidth | Mean             | Error          | StdDev         | Ratio | RatioSD | Gen0       | Gen1       | Gen2      | Allocated     | Alloc Ratio |
//|------------ |--------------- |------------- |-----------------:|---------------:|---------------:|------:|--------:|-----------:|-----------:|----------:|--------------:|------------:|
//| evalBranchy | 2              | 32           |         6.433 us |      0.1228 us |      0.1365 us |  1.00 |    0.03 |     0.8240 |     0.0076 |         - |      13.58 KB |        1.00 |
//| evalPool    | 2              | 32           |         5.636 us |      0.0981 us |      0.0918 us |  0.88 |    0.02 |     0.8621 |     0.0076 |         - |      14.13 KB |        1.04 |
//|             |                |              |                  |                |                |       |         |            |            |           |               |             |
//| evalBranchy | 2              | 64           |        26.286 us |      0.4910 us |      0.5253 us |  1.00 |    0.03 |     2.1362 |     0.0916 |         - |      35.32 KB |        1.00 |
//| evalPool    | 2              | 64           |        22.751 us |      0.3251 us |      0.2882 us |  0.87 |    0.02 |     2.3499 |     0.1221 |         - |       38.6 KB |        1.09 |
//|             |                |              |                  |                |                |       |         |            |            |           |               |             |
//| evalBranchy | 4              | 32           |       207.857 us |      4.1063 us |      3.8410 us |  1.00 |    0.03 |    18.3105 |     5.1270 |         - |     299.78 KB |        1.00 |
//| evalPool    | 4              | 32           |       167.201 us |      2.6917 us |      2.3861 us |  0.80 |    0.02 |    20.5078 |     7.5684 |         - |     338.66 KB |        1.13 |
//|             |                |              |                  |                |                |       |         |            |            |           |               |             |
//| evalBranchy | 4              | 64           |     3,776.004 us |     72.4502 us |     71.1558 us |  1.00 |    0.03 |   195.3125 |   183.5938 |   66.4063 |    2808.35 KB |        1.00 |
//| evalPool    | 4              | 64           |     2,912.515 us |     45.1121 us |     37.6707 us |  0.77 |    0.02 |   156.2500 |   152.3438 |   78.1250 |    2808.04 KB |        1.00 |
//|             |                |              |                  |                |                |       |         |            |            |           |               |             |
//| evalBranchy | 8              | 32           |    39,697.260 us |    788.9195 us |  2,008.0521 us |  1.00 |    0.07 |  1461.5385 |  1384.6154 |  692.3077 |   22489.07 KB |        1.00 |
//| evalPool    | 8              | 32           |    35,979.950 us |    687.9808 us |    736.1318 us |  0.91 |    0.05 |  2066.6667 |  2000.0000 | 1266.6667 |   21104.78 KB |        0.94 |
//|             |                |              |                  |                |                |       |         |            |            |           |               |             |
//| evalBranchy | 8              | 64           | 3,786,698.979 us | 34,145.4561 us | 30,269.0574 us |  1.00 |    0.01 | 84000.0000 | 79000.0000 | 1000.0000 | 1964760.91 KB |        1.00 |
//| evalPool    | 8              | 64           | 3,355,995.086 us | 35,679.2938 us | 31,628.7647 us |  0.89 |    0.01 | 84000.0000 | 83000.0000 | 1000.0000 | 1972066.33 KB |        1.00 |
