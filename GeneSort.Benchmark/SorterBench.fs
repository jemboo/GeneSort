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




//| Method          | mergeDimension | sortingWidth | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
//|---------------- |--------------- |------------- |------------:|---------:|---------:|------:|--------:|----------:|----------:|---------:|----------:|------------:|
//| evalStandard    | 3              | 36           |   102.57 ms | 4.983 ms | 3.296 ms |  1.00 |    0.04 | 1000.0000 |  800.0000 |        - |  17.83 MB |        1.00 |
//| evalUint8v256_2 | 3              | 36           |    20.50 ms | 0.446 ms | 0.295 ms |  0.20 |    0.01 | 1250.0000 |  906.2500 |  62.5000 |  18.61 MB |        1.04 |
//| evalUint8v256_4 | 3              | 36           |    22.04 ms | 0.370 ms | 0.220 ms |  0.22 |    0.01 |  843.7500 |  812.5000 |        - |  13.72 MB |        0.77 |
//| evalUint8v256_8 | 3              | 36           |    22.53 ms | 0.537 ms | 0.355 ms |  0.22 |    0.01 |  843.7500 |  812.5000 |        - |  13.72 MB |        0.77 |
//|                 |                |              |             |          |          |       |         |           |           |          |           |             |
//| evalStandard    | 3              | 48           |   174.25 ms | 0.767 ms | 0.456 ms |  1.00 |    0.00 | 1000.0000 |  666.6667 |        - |  19.48 MB |        1.00 |
//| evalUint8v256_2 | 3              | 48           |    25.61 ms | 0.719 ms | 0.475 ms |  0.15 |    0.00 | 2250.0000 | 1093.7500 | 250.0000 |  30.81 MB |        1.58 |
//| evalUint8v256_4 | 3              | 48           |    32.76 ms | 1.852 ms | 1.225 ms |  0.19 |    0.01 | 1312.5000 |  968.7500 |        - |  21.02 MB |        1.08 |
//| evalUint8v256_8 | 3              | 48           |    38.10 ms | 0.947 ms | 0.563 ms |  0.22 |    0.00 | 1000.0000 |  916.6667 |        - |  16.13 MB |        0.83 |
//|                 |                |              |             |          |          |       |         |           |           |          |           |             |
//| evalStandard    | 4              | 36           |   504.86 ms | 6.513 ms | 3.876 ms |  1.00 |    0.01 | 1000.0000 |         - |        - |  18.77 MB |        1.00 |
//| evalUint8v256_2 | 4              | 36           |    52.60 ms | 1.607 ms | 0.841 ms |  0.10 |    0.00 | 4555.5556 | 1111.1111 | 222.2222 |  66.05 MB |        3.52 |
//| evalUint8v256_4 | 4              | 36           |    39.41 ms | 0.646 ms | 0.427 ms |  0.08 |    0.00 | 2923.0769 |  923.0769 | 230.7692 |  41.57 MB |        2.22 |
//| evalUint8v256_8 | 4              | 36           |    63.15 ms | 4.158 ms | 2.750 ms |  0.13 |    0.01 | 1777.7778 | 1000.0000 | 111.1111 |  26.89 MB |        1.43 |
//|                 |                |              |             |          |          |       |         |           |           |          |           |             |
//| evalStandard    | 4              | 48           | 1,126.28 ms | 1.982 ms | 1.037 ms |  1.00 |    0.00 | 1000.0000 |         - |        - |  20.46 MB |        1.00 |
//| evalUint8v256_2 | 4              | 48           |    45.74 ms | 3.205 ms | 2.120 ms |  0.04 |    0.00 | 9000.0000 | 1166.6667 | 166.6667 | 132.25 MB |        6.46 |
//| evalUint8v256_4 | 4              | 48           |    56.09 ms | 9.173 ms | 5.459 ms |  0.05 |    0.00 | 5200.0000 | 1100.0000 | 200.0000 |  78.41 MB |        3.83 |
//| evalUint8v256_8 | 4              | 48           |    75.75 ms | 3.572 ms | 2.363 ms |  0.07 |    0.00 | 3333.3333 | 1333.3333 | 333.3333 |  49.04 MB |        2.40 |

//| Method          | mergeDimension | sortingWidth | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0        | Gen1        | Gen2     | Allocated  | Alloc Ratio |
//|---------------- |--------------- |------------- |--------------:|------------:|------------:|------:|--------:|------------:|------------:|---------:|-----------:|------------:|
//| evalStandard    | 2              | 36           |     18.260 ms |   0.2200 ms |   0.1309 ms |  1.00 |    0.01 |   1031.2500 |    937.5000 |        - |   16.96 MB |        1.00 |
//| evalUint8v256_2 | 2              | 36           |      8.623 ms |   0.0591 ms |   0.0309 ms |  0.47 |    0.00 |    765.6250 |    750.0000 |        - |   12.42 MB |        0.73 |
//| evalUint8v256_4 | 2              | 36           |      8.553 ms |   0.0932 ms |   0.0555 ms |  0.47 |    0.00 |    765.6250 |    750.0000 |        - |   12.42 MB |        0.73 |
//| evalUint8v256_8 | 2              | 36           |      9.788 ms |   1.8856 ms |   1.2472 ms |  0.54 |    0.07 |    765.6250 |    750.0000 |        - |   12.42 MB |        0.73 |
//|                 |                |              |               |             |             |       |         |             |             |          |            |             |
//| evalStandard    | 2              | 48           |     25.030 ms |   0.5866 ms |   0.3880 ms |  1.00 |    0.02 |   1156.2500 |    968.7500 |        - |   18.66 MB |        1.00 |
//| evalUint8v256_2 | 2              | 48           |     10.350 ms |   0.7308 ms |   0.4349 ms |  0.41 |    0.02 |    875.0000 |    781.2500 |        - |   14.16 MB |        0.76 |
//| evalUint8v256_4 | 2              | 48           |     10.035 ms |   0.0963 ms |   0.0573 ms |  0.40 |    0.01 |    875.0000 |    796.8750 |        - |   14.16 MB |        0.76 |
//| evalUint8v256_8 | 2              | 48           |     10.031 ms |   0.0661 ms |   0.0437 ms |  0.40 |    0.01 |    875.0000 |    796.8750 |        - |   14.16 MB |        0.76 |
//|                 |                |              |               |             |             |       |         |             |             |          |            |             |
//| evalStandard    | 6              | 36           |  6,888.583 ms | 118.5248 ms |  78.3968 ms |  1.00 |    0.02 |   1000.0000 |           - |        - |   19.11 MB |        1.00 |
//| evalUint8v256_2 | 6              | 36           |    186.666 ms |   6.1066 ms |   3.1939 ms |  0.03 |    0.00 |  54000.0000 |  48000.0000 | 333.3333 |     766 MB |       40.08 |
//| evalUint8v256_4 | 6              | 36           |    183.413 ms |   7.9008 ms |   5.2259 ms |  0.03 |    0.00 |  28000.0000 |  21333.3333 | 333.3333 |  408.77 MB |       21.39 |
//| evalUint8v256_8 | 6              | 36           |    201.480 ms |  21.3392 ms |  12.6986 ms |  0.03 |    0.00 |  15666.6667 |  10666.6667 | 333.3333 |  232.57 MB |       12.17 |
//|                 |                |              |               |             |             |       |         |             |             |          |            |             |
//| evalStandard    | 6              | 48           | 24,764.043 ms | 344.0317 ms | 204.7277 ms |  1.00 |    0.01 |   2000.0000 |   1000.0000 |        - |   32.87 MB |        1.00 |
//| evalUint8v256_2 | 6              | 48           |  1,200.379 ms |  74.4519 ms |  49.2453 ms |  0.05 |    0.00 | 188000.0000 | 149000.0000 |        - | 2718.93 MB |       82.72 |
//| evalUint8v256_4 | 6              | 48           |  1,017.601 ms |  52.6780 ms |  34.8432 ms |  0.04 |    0.00 |  98000.0000 |  92000.0000 |        - |  1471.1 MB |       44.76 |
//| evalUint8v256_8 | 6              | 48           |    810.953 ms |  61.1617 ms |  40.4547 ms |  0.03 |    0.00 |  56000.0000 |  49000.0000 |        - |  844.75 MB |       25.70 |

//| Method          | mergeDimension | sortingWidth | Mean       | Error    | StdDev   | Gen0        | Gen1        | Allocated  |
//|---------------- |--------------- |------------- |-----------:|---------:|---------:|------------:|------------:|-----------:|
//| evalUint8v256_2 | 6              | 48           | 1,325.8 ms | 89.19 ms | 58.99 ms | 169000.0000 | 138000.0000 |  2703.8 MB |
//| evalUint8v256_4 | 6              | 48           |   568.5 ms | 33.06 ms | 21.87 ms |  91000.0000 |  85000.0000 | 1457.95 MB |
//| evalUint8v256_8 | 6              | 48           |   597.1 ms | 73.81 ms | 43.93 ms |  52000.0000 |  46000.0000 |   832.6 MB |

//| Method          | mergeDimension | sortingWidth | Mean       | Error    | StdDev   | Gen0        | Gen1        | Allocated  |
//|---------------- |--------------- |------------- |-----------:|---------:|---------:|------------:|------------:|-----------:|
//| evalUint8v256_2 | 6              | 48           | 1,406.6 ms | 74.22 ms | 49.09 ms | 187000.0000 | 153000.0000 | 2718.94 MB |
//| evalUint8v256_4 | 6              | 48           |   640.0 ms | 45.04 ms | 29.79 ms |  98000.0000 |  93000.0000 |  1471.1 MB |
//| evalUint8v256_8 | 6              | 48           |   593.2 ms | 24.96 ms | 14.85 ms |  55000.0000 |  44000.0000 |  844.69 MB |

//| Method          | mergeDimension | sortingWidth | Mean       | Error     | StdDev   | Gen0       | Gen1       | Gen2      | Allocated |
//|---------------- |--------------- |------------- |-----------:|----------:|---------:|-----------:|-----------:|----------:|----------:|
//| evalUint8v256_2 | 6              | 48           | 1,212.4 ms |  49.02 ms | 32.42 ms | 37000.0000 | 35000.0000 | 2000.0000 | 570.89 MB |
//| evalUint8v256_4 | 6              | 48           |   691.8 ms | 149.09 ms | 98.62 ms | 19000.0000 | 17000.0000 | 1000.0000 | 287.09 MB |
//| evalUint8v256_8 | 6              | 48           |   641.7 ms |  64.64 ms | 42.75 ms | 17000.0000 | 15000.0000 | 1000.0000 | 257.75 MB |


// orig:
//| Method          | mergeDimension | sortingWidth | Mean    | Error    | StdDev   | Gen0        | Gen1        | Gen2      | Allocated  |
//|---------------- |--------------- |------------- |--------:|---------:|---------:|------------:|------------:|----------:|-----------:|
//| evalUint8v256_2 | 6              | 48           | 1.544 s | 0.0703 s | 0.0465 s | 187000.0000 | 151000.0000 |         - | 2718.94 MB |
//| evalUint8v256_4 | 6              | 48           | 1.052 s | 0.2458 s | 0.1626 s |  99000.0000 |  80000.0000 | 1000.0000 | 1471.17 MB |
//| evalUint8v256_8 | 6              | 48           | 1.044 s | 0.2335 s | 0.1389 s |  56000.0000 |  43000.0000 | 1000.0000 |  844.76 MB |

//2:
//| Method          | mergeDimension | sortingWidth | Mean       | Error    | StdDev   | Gen0       | Gen1       | Gen2      | Allocated |
//|---------------- |--------------- |------------- |-----------:|---------:|---------:|-----------:|-----------:|----------:|----------:|
//| evalUint8v256_2 | 6              | 48           | 1,190.1 ms | 48.34 ms | 31.98 ms | 35000.0000 | 33000.0000 | 2000.0000 | 526.81 MB |
//| evalUint8v256_4 | 6              | 48           |   608.8 ms | 49.26 ms | 32.58 ms | 19000.0000 | 17000.0000 | 1000.0000 | 301.77 MB |
//| evalUint8v256_8 | 6              | 48           |   687.9 ms | 46.23 ms | 30.58 ms | 27000.0000 | 24000.0000 | 1000.0000 | 424.05 MB |

// 2; IsBlockSorted3
//| Method          | mergeDimension | sortingWidth | Mean       | Error     | StdDev   | Gen0       | Gen1       | Gen2      | Allocated |
//|---------------- |--------------- |------------- |-----------:|----------:|---------:|-----------:|-----------:|----------:|----------:|
//| evalUint8v256_2 | 6              | 48           | 1,320.7 ms | 118.08 ms | 78.10 ms | 31000.0000 | 29000.0000 | 2000.0000 | 468.18 MB |
//| evalUint8v256_4 | 6              | 48           |   701.1 ms |  86.51 ms | 57.22 ms | 16000.0000 | 14000.0000 | 1000.0000 | 247.95 MB |
//| evalUint8v256_8 | 6              | 48           |   720.8 ms |  89.66 ms | 59.30 ms | 26000.0000 | 24000.0000 | 1000.0000 | 409.43 MB |



// 2; IsBlockSorted2
//| Method          | mergeDimension | sortingWidth | Mean       | Error    | StdDev   | Gen0       | Gen1       | Gen2      | Allocated |
//|---------------- |--------------- |------------- |-----------:|---------:|---------:|-----------:|-----------:|----------:|----------:|
//| evalUint8v256_2 | 6              | 48           | 1,198.9 ms | 91.89 ms | 60.78 ms | 37000.0000 | 35000.0000 | 2000.0000 | 556.34 MB |
//| evalUint8v256_4 | 6              | 48           |   652.8 ms | 60.41 ms | 39.96 ms | 17000.0000 | 15000.0000 | 1000.0000 | 267.52 MB |
//| evalUint8v256_8 | 6              | 48           |   685.7 ms | 75.16 ms | 44.73 ms | 19000.0000 | 16000.0000 | 1000.0000 | 287.07 MB |




[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 10)>]
type SorterEvalBench() =

    [<Params(6)>]
    member val mergeDimension = 0 with get, set
    
    [<Params(48)>]
    member val sortingWidth = 0 with get, set

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

        let sorterCount = 128 |> UMX.tag<sorterCount>
        let firstIndex = 0 |> UMX.tag<sorterCount>
        let ceCount = 10000 |> UMX.tag<ceLength>


        let smm =
             MsceRandGen.create 
                        randomType 
                        (this.sortingWidth |> UMX.tag<sortingWidth>) 
                        true 
                        ceCount
                        |> sorterModelMaker.SmmMsceRandGen

        let sorterModelSetMaker = sorterModelSetMaker.create smm firstIndex sorterCount
        let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
        let sorterSet = SorterModelSet.makeSorterSet sorterModelSet
        this.ceBlocks <- sorterSet.Sorters |> Array.map (CeBlock.fromSorter)


        //let bitonics = BitonicSorter.bitonicSort1
        //                    (this.sortingWidth |> UMX.tag<sortingWidth>)

        //let ceBlock = bitonics |> Array.concat 
        //                       |> ceBlock.create  
        //                            (Guid.NewGuid() |> UMX.tag<ceBlockId>) 
        //                            (this.sortingWidth |> UMX.tag<sortingWidth>)

        //this.ceBlocks <- Array.init 128 (fun _ -> ceBlock)




    //[<Benchmark(Baseline = true)>]
    //member this.evalStandard() =
    //   let ceBlockEval = CeBlockOps.evalWithSorterTests this.sortableIntTests this.ceBlocks None
    //   ceBlockEval


    [<Benchmark>]
    member this.evalUint8v256_2() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests this.sortableUint8v256Test this.ceBlocks (Some 2)
       ceBlockEval

    [<Benchmark>]
    member this.evalUint8v256_4() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests this.sortableUint8v256Test this.ceBlocks (Some 4)
       ceBlockEval


    [<Benchmark>]
    member this.evalUint8v256_8() =
       let ceBlockEval = CeBlockOps.evalWithSorterTests this.sortableUint8v256Test this.ceBlocks (Some 8)
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
