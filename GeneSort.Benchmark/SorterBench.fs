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
open GeneSort.Sorting.Sortable.SortableIntArray


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



//| Method        | mergeDimension | sortingWidth | sorterCount | ceLength | collectResults | Mean      | Error      | StdDev    | Ratio | RatioSD | Gen0       | Gen1       | Gen2      | Allocated | Alloc Ratio |
//|-------------- |--------------- |------------- |------------ |--------- |--------------- |----------:|-----------:|----------:|------:|--------:|-----------:|-----------:|----------:|----------:|------------:|
//| evalUint8v256 | 8              | 32           | 100         | 1000     | False          |  35.69 ms |   0.314 ms |  0.187 ms |  1.00 |    0.01 |   400.0000 |   266.6667 |         - |  20.51 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 1000     | False          |  37.35 ms |   1.282 ms |  0.848 ms |  1.05 |    0.02 |   357.1429 |   214.2857 |         - |  19.76 MB |        0.96 |
//|               |                |              |             |          |                |           |            |           |       |         |            |            |           |           |             |
//| evalUint8v256 | 8              | 32           | 100         | 1000     | True           | 453.54 ms |   7.862 ms |  5.200 ms |  1.00 |    0.02 | 16000.0000 | 14000.0000 | 2000.0000 | 709.79 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 1000     | True           | 455.92 ms |  16.187 ms | 10.706 ms |  1.01 |    0.03 | 16000.0000 | 14000.0000 | 2000.0000 | 712.43 MB |        1.00 |
//|               |                |              |             |          |                |           |            |           |       |         |            |            |           |           |             |
//| evalUint8v256 | 8              | 32           | 100         | 5000     | False          | 145.35 ms |  11.455 ms |  7.577 ms |  1.00 |    0.07 |  3000.0000 |  2750.0000 |  750.0000 | 117.04 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 5000     | False          | 149.41 ms |   6.987 ms |  4.158 ms |  1.03 |    0.06 |  2250.0000 |  1750.0000 |  250.0000 | 106.49 MB |        0.91 |
//|               |                |              |             |          |                |           |            |           |       |         |            |            |           |           |             |
//| evalUint8v256 | 8              | 32           | 100         | 5000     | True           | 155.10 ms |   4.620 ms |  3.056 ms |  1.00 |    0.03 |  3250.0000 |  2750.0000 |  750.0000 | 129.23 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 5000     | True           | 147.04 ms |   8.735 ms |  5.778 ms |  0.95 |    0.04 |  2500.0000 |  1750.0000 |  250.0000 | 108.19 MB |        0.84 |
//|               |                |              |             |          |                |           |            |           |       |         |            |            |           |           |             |
//| evalUint8v256 | 8              | 32           | 100         | 10000    | False          | 291.41 ms |  23.665 ms | 15.653 ms |  1.00 |    0.07 |  4500.0000 |  4000.0000 | 1000.0000 | 186.67 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 10000    | False          | 260.50 ms |   4.054 ms |  2.120 ms |  0.90 |    0.05 |  4000.0000 |  3000.0000 | 1000.0000 | 167.56 MB |        0.90 |
//|               |                |              |             |          |                |           |            |           |       |         |            |            |           |           |             |
//| evalUint8v256 | 8              | 32           | 100         | 10000    | True           | 291.89 ms |  19.971 ms | 10.445 ms |  1.00 |    0.05 |  4000.0000 |  3000.0000 | 1000.0000 | 178.82 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 10000    | True           | 272.63 ms |   4.726 ms |  3.126 ms |  0.94 |    0.03 |  4000.0000 |  3000.0000 | 1000.0000 | 182.62 MB |        1.02 |
//|               |                |              |             |          |                |           |            |           |       |         |            |            |           |           |             |
//| evalUint8v256 | 8              | 32           | 100         | 30000    | False          | 918.39 ms | 141.937 ms | 93.882 ms |  1.01 |    0.14 |  3000.0000 |  3000.0000 | 3000.0000 | 610.12 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 30000    | False          | 786.80 ms |  49.501 ms | 29.457 ms |  0.86 |    0.09 |  3000.0000 |  3000.0000 | 3000.0000 | 758.98 MB |        1.24 |
//|               |                |              |             |          |                |           |            |           |       |         |            |            |           |           |             |
//| evalUint8v256 | 8              | 32           | 100         | 30000    | True           | 833.59 ms |  50.072 ms | 29.797 ms |  1.00 |    0.05 |  3000.0000 |  3000.0000 | 3000.0000 | 720.78 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 30000    | True           | 793.48 ms |   8.833 ms |  4.620 ms |  0.95 |    0.03 |  3000.0000 |  3000.0000 | 3000.0000 | 720.76 MB |        1.00 |



//| Method        | mergeDimension | sortingWidth | sorterCount | ceLength | collectResults | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0        | Gen1        | Gen2      | Allocated  | Alloc Ratio |
//|-------------- |--------------- |------------- |------------ |--------- |--------------- |------------:|-----------:|-----------:|------:|--------:|------------:|------------:|----------:|-----------:|------------:|
//| evalUint8v256 | 8              | 32           | 10          | 10000    | False          |    26.73 ms |   0.247 ms |   0.147 ms |  1.00 |    0.01 |    343.7500 |    281.2500 |         - |   16.65 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 10          | 10000    | False          |    29.51 ms |   1.525 ms |   1.009 ms |  1.10 |    0.04 |    343.7500 |    250.0000 |         - |   16.84 MB |        1.01 |
//|               |                |              |             |          |                |             |            |            |       |         |             |             |           |            |             |
//| evalUint8v256 | 8              | 32           | 10          | 10000    | True           |    32.62 ms |   1.020 ms |   0.675 ms |  1.00 |    0.03 |    312.5000 |    187.5000 |         - |   17.48 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 10          | 10000    | True           |    31.49 ms |   1.710 ms |   1.131 ms |  0.97 |    0.04 |    312.5000 |    187.5000 |         - |   17.72 MB |        1.01 |
//|               |                |              |             |          |                |             |            |            |       |         |             |             |           |            |             |
//| evalUint8v256 | 8              | 32           | 100         | 10000    | False          |   296.62 ms |  24.285 ms |  16.063 ms |  1.00 |    0.07 |   4500.0000 |   4000.0000 | 1000.0000 |  190.48 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 10000    | False          |   297.62 ms |  23.635 ms |  15.633 ms |  1.01 |    0.07 |   4500.0000 |   4000.0000 | 1000.0000 |  173.28 MB |        0.91 |
//|               |                |              |             |          |                |             |            |            |       |         |             |             |           |            |             |
//| evalUint8v256 | 8              | 32           | 100         | 10000    | True           |   306.14 ms |  57.291 ms |  37.895 ms |  1.01 |    0.17 |   4000.0000 |   3000.0000 | 1000.0000 |  178.81 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 100         | 10000    | True           |   309.80 ms |  16.433 ms |   9.779 ms |  1.03 |    0.12 |   4500.0000 |   4000.0000 | 1000.0000 |   178.9 MB |        1.00 |
//|               |                |              |             |          |                |             |            |            |       |         |             |             |           |            |             |
//| evalUint8v256 | 8              | 32           | 1000        | 10000    | False          | 3,138.19 ms | 449.260 ms | 297.158 ms |  1.01 |    0.13 | 109000.0000 | 108000.0000 | 6000.0000 | 4957.67 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 1000        | 10000    | False          | 3,034.39 ms | 520.314 ms | 344.155 ms |  0.97 |    0.14 | 110000.0000 | 109000.0000 | 5000.0000 | 5072.37 MB |        1.02 |
//|               |                |              |             |          |                |             |            |            |       |         |             |             |           |            |             |
//| evalUint8v256 | 8              | 32           | 1000        | 10000    | True           | 3,023.54 ms | 157.021 ms | 103.859 ms |  1.00 |    0.05 | 106000.0000 | 105000.0000 | 3000.0000 | 4955.83 MB |        1.00 |
//| evalUint8v512 | 8              | 32           | 1000        | 10000    | True           | 3,136.35 ms | 333.573 ms | 220.638 ms |  1.04 |    0.08 | 105000.0000 | 104000.0000 | 3000.0000 |  4917.6 MB |        0.99 |


[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 10)>]
type MergeEvalBench() =

    [<Params(8)>]
    member val mergeDimension = 0 with get, set
    
    [<Params(32)>]
    member val sortingWidth = 0 with get, set

    [<Params(10, 100, 1000)>]
    member val sorterCount = 0 with get, set

    [<Params(10000)>]
    member val ceLength = 0 with get, set

    [<Params(true, false)>]
    member val collectResults = false with get, set


    member val mergeFillType = mergeFillType.NoFill

    member val id = Guid.NewGuid() with get, set

    member val sortableIntTests = sortableIntTest.Empty |> sortableTest.Ints with get, set
    member val sortablePackedIntTests = sortableIntTest.Empty |> sortableTest.Ints with get, set
    member val sortableUint8v256Test = sortableUint8v256Test.Empty |> sortableTest.Uint8v256 with get, set
    member val sortableUint8v512Test = sortableUint8v512Test.Empty |> sortableTest.Uint8v512 with get, set

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

        this.sortableUint8v512Test <- SortableUint8v512Test.fromIntArrays
                                        (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        intArrays
                                  |> sortableTest.Uint8v512

        
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

    
    [<Benchmark(Baseline = true)>]
    member this.evalUint8v256() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests 
                            this.sortableUint8v256Test 
                            this.ceBlocks 
                            this.collectResults
       ceBlockEval

    [<Benchmark>]
    member this.evalUint8v512() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests 
                            this.sortableUint8v256Test 
                            this.ceBlocks 
                            this.collectResults
       ceBlockEval



[<MemoryDiagnoser>]
type Eval2Rounds() =

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


//| Method        | mergeDimension | sortingWidth | sorterCount | ceLength | collectResults | Mean            | Error         | StdDev        | Ratio | RatioSD | Gen0        | Gen1        | Gen2       | Allocated   | Alloc Ratio |
//|-------------- |--------------- |------------- |------------ |--------- |--------------- |----------------:|--------------:|--------------:|------:|--------:|------------:|------------:|-----------:|------------:|------------:|
//| evalUint8v256 | 16           | 10          | 1000     | False          |      4,995.7 us |       6.92 us |       4.12 us |  1.00 |    0.00 |     31.2500 |           - |          - |      1.8 MB |        1.00 |
//| evalUint8v512 | 16           | 10          | 1000     | False          |      3,701.8 us |      26.35 us |      15.68 us |  0.74 |    0.00 |     31.2500 |      7.8125 |          - |     1.77 MB |        0.99 |
//| evalBitv512   | 16           | 10          | 1000     | False          |        703.5 us |       5.15 us |       3.41 us |  0.14 |    0.00 |     38.0859 |     17.5781 |          - |     1.74 MB |        0.97 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 10          | 1000     | True           |      5,276.4 us |      24.86 us |      16.44 us |  1.00 |    0.00 |     78.1250 |     31.2500 |          - |     3.97 MB |        1.00 |
//| evalUint8v512 | 16           | 10          | 1000     | True           |      4,317.5 us |      83.80 us |      55.43 us |  0.82 |    0.01 |     78.1250 |     31.2500 |          - |     3.71 MB |        0.94 |
//| evalBitv512   | 16           | 10          | 1000     | True           |        717.9 us |       8.86 us |       5.86 us |  0.14 |    0.00 |     41.9922 |     17.5781 |          - |     1.99 MB |        0.50 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 10          | 10000    | False          |     43,249.7 us |     397.61 us |     262.99 us |  1.00 |    0.01 |    333.3333 |    250.0000 |          - |    16.41 MB |        1.00 |
//| evalUint8v512 | 16           | 10          | 10000    | False          |     40,403.7 us |     932.81 us |     617.00 us |  0.93 |    0.01 |    307.6923 |    230.7692 |          - |    16.58 MB |        1.01 |
//| evalBitv512   | 16           | 10          | 10000    | False          |      5,763.3 us |     111.67 us |      73.86 us |  0.13 |    0.00 |    335.9375 |    265.6250 |          - |    15.92 MB |        0.97 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 10          | 10000    | True           |     47,104.8 us |   1,400.84 us |     926.57 us |  1.00 |    0.03 |    363.6364 |    181.8182 |          - |    17.52 MB |        1.00 |
//| evalUint8v512 | 16           | 10          | 10000    | True           |     38,768.9 us |     974.21 us |     644.38 us |  0.82 |    0.02 |    307.6923 |    230.7692 |          - |    16.99 MB |        0.97 |
//| evalBitv512   | 16           | 10          | 10000    | True           |      5,743.4 us |      95.30 us |      63.03 us |  0.12 |    0.00 |    335.9375 |    257.8125 |          - |    15.97 MB |        0.91 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 100         | 1000     | False          |     46,587.8 us |     555.74 us |     367.59 us |  1.00 |    0.01 |    363.6364 |    181.8182 |          - |    19.67 MB |        1.00 |
//| evalUint8v512 | 16           | 100         | 1000     | False          |     41,307.3 us |     681.94 us |     451.06 us |  0.89 |    0.01 |    384.6154 |    230.7692 |          - |    18.75 MB |        0.95 |
//| evalBitv512   | 16           | 100         | 1000     | False          |      7,903.1 us |     164.88 us |     109.06 us |  0.17 |    0.00 |    390.6250 |    265.6250 |          - |    18.68 MB |        0.95 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 100         | 1000     | True           |     50,341.4 us |     831.47 us |     494.79 us |  1.00 |    0.01 |    727.2727 |    545.4545 |          - |    36.28 MB |        1.00 |
//| evalUint8v512 | 16           | 100         | 1000     | True           |     43,050.3 us |     917.76 us |     607.04 us |  0.86 |    0.01 |    583.3333 |    416.6667 |          - |    30.66 MB |        0.85 |
//| evalBitv512   | 16           | 100         | 1000     | True           |      7,813.5 us |      93.75 us |      62.01 us |  0.16 |    0.00 |    390.6250 |    234.3750 |          - |    18.98 MB |        0.52 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 100         | 10000    | False          |    462,833.2 us |  47,470.80 us |  31,398.99 us |  1.00 |    0.09 |   4000.0000 |   3000.0000 |  1000.0000 |   184.58 MB |        1.00 |
//| evalUint8v512 | 16           | 100         | 10000    | False          |    386,355.7 us |  33,349.33 us |  22,058.51 us |  0.84 |    0.07 |   4000.0000 |   3000.0000 |  1000.0000 |   173.01 MB |        0.94 |
//| evalBitv512   | 16           | 100         | 10000    | False          |     69,383.3 us |   4,000.44 us |   2,646.04 us |  0.15 |    0.01 |   4625.0000 |   4500.0000 |  1125.0000 |   172.98 MB |        0.94 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 100         | 10000    | True           |    446,189.0 us |   3,373.90 us |   1,764.62 us |  1.00 |    0.01 |   5000.0000 |   4000.0000 |  1000.0000 |   201.02 MB |        1.00 |
//| evalUint8v512 | 16           | 100         | 10000    | True           |    378,191.7 us |  25,697.92 us |  16,997.58 us |  0.85 |    0.04 |   4000.0000 |   3000.0000 |  1000.0000 |   179.46 MB |        0.89 |
//| evalBitv512   | 16           | 100         | 10000    | True           |     68,412.5 us |   4,179.89 us |   2,764.74 us |  0.15 |    0.01 |   5500.0000 |   5375.0000 |  1125.0000 |   211.38 MB |        1.05 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 1000        | 1000     | False          |    524,709.2 us |  30,678.20 us |  18,256.10 us |  1.00 |    0.05 |   5000.0000 |   4000.0000 |  1000.0000 |   195.66 MB |        1.00 |
//| evalUint8v512 | 16           | 1000        | 1000     | False          |    423,669.6 us |  19,386.07 us |  11,536.34 us |  0.81 |    0.03 |   4000.0000 |   3000.0000 |  1000.0000 |   180.23 MB |        0.92 |
//| evalBitv512   | 16           | 1000        | 1000     | False          |     93,589.6 us |   6,531.35 us |   4,320.09 us |  0.18 |    0.01 |   5000.0000 |   4833.3333 |  1333.3333 |   176.46 MB |        0.90 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 1000        | 1000     | True           |    489,491.2 us |  11,729.88 us |   7,758.59 us |  1.00 |    0.02 |   9000.0000 |   7000.0000 |  2000.0000 |   338.48 MB |        1.00 |
//| evalUint8v512 | 16           | 1000        | 1000     | True           |    399,777.7 us |  16,204.15 us |  10,718.04 us |  0.82 |    0.02 |   6000.0000 |   5000.0000 |  1000.0000 |   278.28 MB |        0.82 |
//| evalBitv512   | 16           | 1000        | 1000     | True           |     98,401.9 us |   5,919.00 us |   3,915.05 us |  0.20 |    0.01 |   5166.6667 |   5000.0000 |  1500.0000 |   180.02 MB |        0.53 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 1000        | 10000    | False          |  4,537,577.9 us |  58,722.94 us |  38,841.59 us |  1.00 |    0.01 | 108000.0000 | 107000.0000 |  5000.0000 |  4936.39 MB |        1.00 |
//| evalUint8v512 | 16           | 1000        | 10000    | False          |  4,062,481.6 us |  89,551.58 us |  59,232.82 us |  0.90 |    0.01 |  74000.0000 |  73000.0000 |  5000.0000 |  3332.72 MB |        0.68 |
//| evalBitv512   | 16           | 1000        | 10000    | False          |    595,606.8 us |  25,992.38 us |  17,192.35 us |  0.13 |    0.00 |  39000.0000 |  38000.0000 |  3000.0000 |  1728.98 MB |        0.35 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 16           | 1000        | 10000    | True           |  4,768,896.1 us |  62,807.01 us |  32,849.28 us |  1.00 |    0.01 | 117000.0000 | 116000.0000 |  5000.0000 |  5368.64 MB |        1.00 |
//| evalUint8v512 | 16           | 1000        | 10000    | True           |  3,914,545.6 us | 244,197.44 us | 161,521.49 us |  0.82 |    0.03 |  88000.0000 |  87000.0000 |  5000.0000 |   4007.9 MB |        0.75 |
//| evalBitv512   | 16           | 1000        | 10000    | True           |    602,734.5 us |  30,054.42 us |  19,879.14 us |  0.13 |    0.00 |  37000.0000 |  36000.0000 |  2000.0000 |  1692.35 MB |        0.32 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 10          | 1000     | False          |     17,536.2 us |     108.23 us |      71.59 us |  1.00 |    0.01 |     31.2500 |           - |          - |      2.1 MB |        1.00 |
//| evalUint8v512 | 18           | 10          | 1000     | False          |     13,439.3 us |     109.08 us |      72.15 us |  0.77 |    0.00 |     31.2500 |     15.6250 |          - |     1.95 MB |        0.93 |
//| evalBitv512   | 18           | 10          | 1000     | False          |      2,112.3 us |      35.64 us |      23.57 us |  0.12 |    0.00 |     35.1563 |     11.7188 |          - |     1.79 MB |        0.85 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 10          | 1000     | True           |     17,467.3 us |      45.03 us |      26.79 us |  1.00 |    0.00 |    156.2500 |     31.2500 |          - |     7.86 MB |        1.00 |
//| evalUint8v512 | 18           | 10          | 1000     | True           |     14,114.8 us |     149.51 us |      98.89 us |  0.81 |    0.01 |    109.3750 |     46.8750 |          - |     5.69 MB |        0.72 |
//| evalBitv512   | 18           | 10          | 1000     | True           |      2,238.2 us |      24.98 us |      16.52 us |  0.13 |    0.00 |     42.9688 |     19.5313 |          - |     1.98 MB |        0.25 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 10          | 10000    | False          |    168,934.9 us |   2,642.22 us |   1,747.67 us |  1.00 |    0.01 |    250.0000 |           - |          - |     19.3 MB |        1.00 |
//| evalUint8v512 | 18           | 10          | 10000    | False          |    130,962.9 us |     972.81 us |     643.45 us |  0.78 |    0.01 |    250.0000 |           - |          - |    18.21 MB |        0.94 |
//| evalBitv512   | 18           | 10          | 10000    | False          |     20,480.5 us |     626.40 us |     414.33 us |  0.12 |    0.00 |    343.7500 |    250.0000 |          - |    16.25 MB |        0.84 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 10          | 10000    | True           |    165,597.1 us |     339.92 us |     202.28 us |  1.00 |    0.00 |    333.3333 |           - |          - |    21.93 MB |        1.00 |
//| evalUint8v512 | 18           | 10          | 10000    | True           |    129,307.7 us |   1,579.38 us |   1,044.66 us |  0.78 |    0.01 |    333.3333 |           - |          - |    18.91 MB |        0.86 |
//| evalBitv512   | 18           | 10          | 10000    | True           |     20,869.7 us |     569.69 us |     376.81 us |  0.13 |    0.00 |    343.7500 |    218.7500 |          - |    16.32 MB |        0.74 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 100         | 1000     | False          |    172,671.4 us |     467.19 us |     309.01 us |  1.00 |    0.00 |    333.3333 |           - |          - |    18.23 MB |        1.00 |
//| evalUint8v512 | 18           | 100         | 1000     | False          |    130,048.3 us |   1,884.54 us |   1,246.50 us |  0.75 |    0.01 |    250.0000 |           - |          - |    19.19 MB |        1.05 |
//| evalBitv512   | 18           | 100         | 1000     | False          |     23,249.1 us |     426.17 us |     281.89 us |  0.13 |    0.00 |    375.0000 |    218.7500 |          - |    18.49 MB |        1.01 |
//|               |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 100         | 1000     | True           |    181,103.5 us |     869.37 us |     454.70 us |  1.00 |    0.00 |   2666.6667 |   1333.3333 |   333.3333 |   113.33 MB |        1.00 |
//| evalUint8v512 | 18           | 100         | 1000     | True           |    140,923.2 us |     918.90 us |     607.80 us |  0.78 |    0.00 |   2250.0000 |   1250.0000 |   250.0000 |   101.26 MB |        0.89 |
//| evalBitv512   | 18           | 100         | 1000     | True           |     24,364.7 us |     778.95 us |     515.22 us |  0.13 |    0.00 |    625.0000 |    531.2500 |          - |    29.91 MB |        0.26 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 100         | 10000    | False          |  1,656,669.6 us |  11,635.59 us |   6,924.16 us |  1.00 |    0.01 |   7000.0000 |   6000.0000 |  1000.0000 |   326.45 MB |        1.00 |
//| evalUint8v512 | 18           | 100         | 10000    | False          |  1,308,080.9 us |  27,220.82 us |  18,004.89 us |  0.79 |    0.01 |   6000.0000 |   5000.0000 |  1000.0000 |   272.91 MB |        0.84 |
//| evalBitv512   | 18           | 100         | 10000    | False          |    201,463.6 us |   7,717.75 us |   4,592.71 us |  0.12 |    0.00 |   4333.3333 |   4000.0000 |  1000.0000 |   161.96 MB |        0.50 |
//|               |               |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 100         | 10000    | True           |  1,723,116.4 us |  28,465.54 us |  18,828.19 us |  1.00 |    0.01 |  12000.0000 |  11000.0000 |  2000.0000 |   506.73 MB |        1.00 |
//| evalUint8v512 | 18           | 100         | 10000    | True           |  1,332,208.3 us |  27,127.16 us |  17,942.94 us |  0.77 |    0.01 |  10000.0000 |   9000.0000 |  2000.0000 |   405.25 MB |        0.80 |
//| evalBitv512   | 18           | 100         | 10000    | True           |    215,518.2 us |   4,597.84 us |   3,041.19 us |  0.13 |    0.00 |   4333.3333 |   4000.0000 |  1000.0000 |   163.39 MB |        0.32 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 1000        | 1000     | False          |  1,812,456.5 us |  17,734.22 us |  11,730.09 us |  1.00 |    0.01 |  10000.0000 |   9000.0000 |  2000.0000 |   384.69 MB |        1.00 |
//| evalUint8v512 | 18           | 1000        | 1000     | False          |  1,382,436.2 us |  20,062.55 us |  13,270.14 us |  0.76 |    0.01 |   9000.0000 |   8000.0000 |  2000.0000 |   334.41 MB |        0.87 |
//| evalBitv512   | 18           | 1000        | 1000     | False          |    248,223.0 us |   5,854.29 us |   3,872.25 us |  0.14 |    0.00 |   4666.6667 |   4333.3333 |  1000.0000 |   178.62 MB |        0.46 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 1000        | 1000     | True           |  1,930,369.6 us |  10,218.46 us |   6,080.84 us |  1.00 |    0.00 |  35000.0000 |  28000.0000 |  4000.0000 |  1491.09 MB |        1.00 |
//| evalUint8v512 | 18           | 1000         | 1000     | True           |  1,512,879.6 us |  20,735.48 us |  13,715.24 us |  0.78 |    0.01 |  29000.0000 |  20000.0000 |  2000.0000 |  1284.15 MB |        0.86 |
//| evalBitv512   | 18           | 1000        | 1000     | True           |    244,807.3 us |   8,853.24 us |   5,855.87 us |  0.13 |    0.00 |   7500.0000 |   5000.0000 |  1000.0000 |   308.81 MB |        0.21 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 1000        | 10000    | False          | 18,675,264.7 us | 204,883.82 us | 135,517.96 us |  1.00 |    0.01 | 445000.0000 | 444000.0000 | 14000.0000 | 20634.05 MB |        1.00 |
//| evalUint8v512 | 18           | 1000        | 10000    | False          | 14,270,149.6 us | 272,613.68 us | 180,317.07 us |  0.76 |    0.01 | 283000.0000 | 282000.0000 |  8000.0000 | 13188.33 MB |        0.64 |
//| evalBitv512   | 18           | 1000        | 10000    | False          |  1,968,659.8 us |  80,585.79 us |  53,302.51 us |  0.11 |    0.00 |  59000.0000 |  58000.0000 |  3000.0000 |  2687.68 MB |        0.13 |
//|               |                |              |             |          |                |                 |               |               |       |         |             |             |            |             |             |
//| evalUint8v256 | 18           | 1000        | 10000    | True           | 19,567,723.1 us | 482,197.38 us | 318,943.71 us |  1.00 |    0.02 | 389000.0000 | 388000.0000 | 17000.0000 | 17853.64 MB |        1.00 |
//| evalUint8v512 | 18           | 1000        | 10000    | True           | 14,409,718.4 us | 473,505.17 us | 313,194.35 us |  0.74 |    0.02 | 293000.0000 | 292000.0000 |  9000.0000 | 13593.39 MB |        0.76 |
//| evalBitv512   | 18           | 1000        | 10000    | True           |  2,087,246.9 us |  63,725.50 us |  42,150.47 us |  0.11 |    0.00 |  60000.0000 |  59000.0000 |  3000.0000 |  2765.61 MB |        0.15 |

//| Method      | sortingWidth | sorterCount | ceLength | collectResults | Mean       | Error    | StdDev   | Gen0       | Gen1       | Gen2      | Allocated |
//|------------ |------------- |------------ |--------- |--------------- |-----------:|---------:|---------:|-----------:|-----------:|----------:|----------:|
//| evalBitv512 | 20           | 100         | 10000    | False          |   683.6 ms | 31.13 ms | 20.59 ms |  3000.0000 |  2000.0000 |         - | 181.39 MB |
//| evalBitv512 | 20           | 100         | 10000    | True           |   697.2 ms | 30.73 ms | 20.33 ms |  4000.0000 |  3000.0000 |         - | 212.11 MB |
//| evalBitv512 | 22           | 100         | 10000    | False          | 2,527.4 ms | 45.48 ms | 30.08 ms | 15000.0000 | 14000.0000 | 1000.0000 | 705.42 MB |
//| evalBitv512 | 22           | 100         | 10000    | True           | 2,625.7 ms | 37.15 ms | 24.57 ms | 13000.0000 | 12000.0000 | 1000.0000 | 590.92 MB |


//| Method      | sortingWidth | sorterCount | ceLength | collectResults | Mean     | Error    | StdDev   | Gen0       | Gen1       | Gen2      | Allocated |
//|------------ |------------- |------------ |--------- |--------------- |---------:|---------:|---------:|-----------:|-----------:|----------:|----------:|
//| evalBitv512 | 24           | 100         | 10000    | False          | 10.118 s | 0.1474 s | 0.0975 s | 43000.0000 | 42000.0000 | 1000.0000 |   1.98 GB |
//| evalBitv512 | 24           | 100         | 10000    | True           |  9.790 s | 0.1669 s | 0.1104 s | 57000.0000 | 56000.0000 | 1000.0000 |   2.63 GB |

//| Method      | sortingWidth | sorterCount | ceLength | collectResults | Mean    | Error   | StdDev  | Gen0        | Gen1        | Gen2      | Allocated |
//|------------ |------------- |------------ |--------- |--------------- |--------:|--------:|--------:|------------:|------------:|----------:|----------:|
//| evalBitv512 | 26           | 100         | 10000    | False          | 40.68 s | 0.301 s | 0.199 s | 289000.0000 | 288000.0000 | 2000.0000 |  13.27 GB |
//| evalBitv512 | 26           | 100         | 10000    | True           | 41.05 s | 0.213 s | 0.111 s | 375000.0000 | 374000.0000 | 2000.0000 |  17.18 GB |


[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 10)>]
type FullBoolEvalBench() =
    
    [<Params(26)>]
    member val sortingWidth = 0 with get, set

    [<Params(100)>]
    member val sorterCount = 0 with get, set

    [<Params(10000)>]
    member val ceLength = 0 with get, set

    [<Params(true, false)>]
    member val collectResults = false with get, set


    member val mergeFillType = mergeFillType.NoFill

    member val id = Guid.NewGuid() with get, set

    member val sortableBinaryTest = sortableBinaryTest.Empty |> sortableTest.Bools with get, set
    member val sortableUint8v256Test = sortableUint8v256Test.Empty |> sortableTest.Uint8v256 with get, set
    member val sortableUint8v512Test = sortableUint8v512Test.Empty |> sortableTest.Uint8v512 with get, set
    member val sortableBitv512Test = sortableBitv512Test.Empty |> sortableTest.Bitv512 with get, set

    member val ceBlocks = [||] with get, set

    [<GlobalSetup>]
    member this.Setup() =

        let sortableBoolArrays = SortableBoolArray.getAllSortableBoolArrays 
                                    (this.sortingWidth |> UMX.tag<sortingWidth>)

        let sortableIntArrays =  BinaryArrayUtils.getAllSortableBinaryArrays 
                                    (this.sortingWidth |> UMX.tag<sortingWidth>)

        
        let grayBlocks = Sortable.GrayVectorGenerator.getAllSortBlockBitv512ForSortingWidth 
                                    (this.sortingWidth |> UMX.tag<sortingWidth>) |> Seq.toArray


        this.sortableBinaryTest <- sortableBinaryTest.create
                                        (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        sortableBoolArrays
                                  |> sortableTest.Bools

        this.sortableUint8v256Test <- SortableUint8v256Test.fromIntArrays
                                        (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        sortableIntArrays
                                  |> sortableTest.Uint8v256

        this.sortableUint8v512Test <- SortableUint8v512Test.fromIntArrays
                                        (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        sortableIntArrays
                                  |> sortableTest.Uint8v512


        this.sortableBitv512Test <- sortableBitv512Test.create
                                        (Guid.NewGuid() |> UMX.tag<sorterTestId>)
                                        (this.sortingWidth |> UMX.tag<sortingWidth>)
                                        grayBlocks
                                    |> sortableTest.Bitv512

        
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
    //   let ceBlockEval = CeBlockOps.evalWithSorterTests 
    //                        this.sortableBinaryTest 
    //                        this.ceBlocks 
    //                        this.collectResults
    //   ceBlockEval

    
    //[<Benchmark(Baseline = true)>]
    //member this.evalUint8v256() =
    //   let ceBlockEval = CeBlockOps.evalWithSorterTests 
    //                        this.sortableUint8v256Test 
    //                        this.ceBlocks 
    //                        this.collectResults
    //   ceBlockEval


    //[<Benchmark>]
    //member this.evalUint8v512() =
    //   let ceBlockEval = CeBlockOps.evalWithSorterTests 
    //                        this.sortableUint8v512Test 
    //                        this.ceBlocks 
    //                        this.collectResults
    //   ceBlockEval


    [<Benchmark>]
    member this.evalBitv512() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests 
                            this.sortableBitv512Test 
                            this.ceBlocks 
                            this.collectResults
       ceBlockEval

