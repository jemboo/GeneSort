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


    member val mergeFillType = mergeSuffixType.NoSuffix

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

    member val mergeFillType = mergeSuffixType.NoSuffix

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






[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 10)>]
type FullBoolEvalBench() =

    [<Params(8)>]
    member val mergeDimension = 0 with get, set
    
    [<Params(12)>]
    member val sortingWidth = 0 with get, set

    [<Params(10, 100)>]
    member val sorterCount = 0 with get, set

    [<Params(1000)>]
    member val ceLength = 0 with get, set

    [<Params(true, false)>]
    member val collectResults = false with get, set


    member val mergeFillType = mergeSuffixType.NoSuffix

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


    [<Benchmark(Baseline = true)>]
    member this.evalStandard() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests 
                            this.sortableBinaryTest 
                            this.ceBlocks 
                            this.collectResults
       ceBlockEval

    
    [<Benchmark>]
    member this.evalUint8v256() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests 
                            this.sortableUint8v256Test 
                            this.ceBlocks 
                            this.collectResults
       ceBlockEval


    [<Benchmark>]
    member this.evalUint8v512() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests 
                            this.sortableUint8v512Test 
                            this.ceBlocks 
                            this.collectResults
       ceBlockEval


    [<Benchmark>]
    member this.evalBitv512() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests 
                            this.sortableBitv512Test 
                            this.ceBlocks 
                            this.collectResults
       ceBlockEval

