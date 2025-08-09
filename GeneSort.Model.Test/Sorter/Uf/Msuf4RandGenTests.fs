namespace GeneSort.Model.Test.Sorter.Uf


open System
open FSharp.UMX
open Xunit
open EvoMergeSort.Core
open EvoMergeSort.Core.Uf4Seeds
open EvoMergeSort.Sorter
open GeneSort.Model.Sorter.Uf4
open MathUtils

type Msuf4RandGenTests() =

    // Helper function to create a mock random number generator
    let createMockRando (indices: int list) (floats: float list) =
        fun _ _ -> new MockRando(floats, indices) :> IRando

    // Helper function to create uniform TwoOrbitUnfolder4GenRates
    let createTestGenRates (order: int) =
        Uf4GenRates.makeUniform order

    // Helper function to create non-uniform TwoOrbitUnfolder4GenRates
    let createBiasedGenRates (order: int) (twoOrbitType:TwoOrbitType) (baseAmt:float) (biasAmt:float) =
        Uf4GenRates.biasTowards order twoOrbitType baseAmt biasAmt

    // Helper function to create a valid Msuf4RandGen
    let createTestMsuf4RandGen (rngType: rngType) (sortingWidth: int<sortingWidth>) (stageCount: int<stageCount>) =
        let genRates = Array.init (%stageCount) (fun _ -> createTestGenRates (%sortingWidth))
        Msuf4RandGenOld.create rngType sortingWidth stageCount genRates

    [<Fact>]
    let ``create succeeds with valid input`` () =
        let rngType = rngType.Lcg
        let width = 4<sortingWidth>
        let stageCount = 2<stageCount>
        let genRates = [| createTestGenRates 4; createTestGenRates 4 |]
        let msuf4Gen = Msuf4RandGenOld.create rngType width stageCount genRates
        Assert.Equal(rngType, msuf4Gen.RngType)
        Assert.Equal(width, msuf4Gen.SortingWidth)
        Assert.Equal(stageCount, msuf4Gen.StageCount)
        Assert.Equal<Uf4GenRates array>(genRates, msuf4Gen.GenRates)

    [<Fact>]
    let ``create fails with stageCount less than 1`` () =
        let rngType = rngType.Lcg
        let width = 4<sortingWidth>
        let stageCount = 0<stageCount>
        let genRates = [||]
        let ex = Assert.Throws<Exception>(fun () -> Msuf4RandGenOld.create rngType width stageCount genRates |> ignore)
        Assert.Equal("StageCount must be at least 1, got 0", ex.Message)

    [<Fact>]
    let ``create fails with mismatched genRates length`` () =
        let rngType = rngType.Lcg
        let width = 4<sortingWidth>
        let stageCount = 2<stageCount>
        let genRates = [| createTestGenRates 4 |] // Length 1, should be 2
        let ex = Assert.Throws<Exception>(fun () -> Msuf4RandGenOld.create rngType width stageCount genRates |> ignore)
        Assert.Equal("genRates array length (1) must equal stageCount (2)", ex.Message)

    [<Fact>]
    let ``create fails with genRates order mismatch`` () =
        let rngType = rngType.Lcg
        let width = 4<sortingWidth>
        let stageCount = 1<stageCount>
        let genRates = [| createTestGenRates 8 |] // Order 8, should be 4
        let ex = Assert.Throws<Exception>(fun () -> Msuf4RandGenOld.create rngType width stageCount genRates |> ignore)
        Assert.Equal("All genRates must have order 4", ex.Message)

    [<Fact>]
    let ``create fails with invalid seedGenRates sum`` () =
        let rngType = rngType.Lcg
        let width = 4<sortingWidth>
        let stageCount = 1<stageCount>
        let invalidGenRates = 
            { Uf4GenRates.order = 4
              seedGenRatesUf4 = { Ortho = 0.5; Para = 0.5; SelfRefl = 0.5 } // Sum = 1.5
              opsGenRatesList = [ TwoOrbitPairGenRates.makeUniform() ] }
        let genRates = [| invalidGenRates |]
        let ex = Assert.Throws<Exception>(fun () -> Msuf4RandGenOld.create rngType width stageCount genRates |> ignore)
        Assert.Equal("Sum of seedGenRates must not exceed 1.0", ex.Message)

    [<Fact>]
    let ``equality based on all fields`` () =
        let rngType = rngType.Lcg
        let width = 4<sortingWidth>
        let stageCount = 2<stageCount>
        let genRates = [| createTestGenRates 4; createTestGenRates 4 |]
        let msuf4Gen1 = Msuf4RandGenOld.create rngType width stageCount genRates
        let msuf4Gen2 = Msuf4RandGenOld.create rngType width stageCount genRates
        Assert.Equal(msuf4Gen1, msuf4Gen2)
        Assert.Equal(msuf4Gen1.GetHashCode(), msuf4Gen2.GetHashCode())

    [<Fact>]
    let ``inequality with different fields`` () =
        let rngType1 = rngType.Lcg
        let rngType2 = rngType.Net
        let width = 8<sortingWidth>
        let stageCount = 1<stageCount>
        let genRates1 = [| createTestGenRates 8 |]
        let genRates2 = [| Uf4GenRates.biasTowards 8 TwoOrbitType.Para (1.0/ 3.0)  0.1 |]
        let msuf4Gen1 = Msuf4RandGenOld.create rngType1 width stageCount genRates1
        let msuf4Gen2 = Msuf4RandGenOld.create rngType2 width stageCount genRates1
        let msuf4Gen3 = Msuf4RandGenOld.create rngType1 width stageCount genRates2
        Assert.NotEqual(msuf4Gen1, msuf4Gen2)
        Assert.NotEqual(msuf4Gen1, msuf4Gen3)
        Assert.NotEqual(msuf4Gen1.GetHashCode(), msuf4Gen2.GetHashCode())
        Assert.NotEqual(msuf4Gen1.GetHashCode(), msuf4Gen3.GetHashCode())


    [<Fact>]
    let ``makeId generates unique IDs`` () =
        let rngType = rngType.Lcg
        let width = 4<sortingWidth>
        let stageCount = 1<stageCount>
        let genRates = [| createTestGenRates 4 |]
        let msuf4Gen = Msuf4RandGenOld.create rngType width stageCount genRates
        let id1 = Msuf4RandGenOld.makeId msuf4Gen 0
        let id2 = Msuf4RandGenOld.makeId msuf4Gen 1
        Assert.NotEqual(id1, id2)

    [<Fact>]
    let ``makeModelUniform produces Msuf4 with correct properties`` () =
        let rngType = rngType.Lcg
        let width = 4<sortingWidth>
        let stageCount = 2<stageCount>
        let genRates = [| createTestGenRates 4; createTestGenRates 4 |]
        let msuf4Gen = Msuf4RandGenOld.create rngType width stageCount genRates
        let mockRando = createMockRando [0] [0.5; 0.5]
        let msuf4 = Msuf4RandGenOld.makeModel msuf4Gen mockRando 0
        Assert.Equal(%msuf4.Id |> UMX.tag<sorterId>, Msuf4.makeSorter(msuf4).SorterId)
        Assert.Equal(width, msuf4.SortingWidth)
        Assert.Equal(%stageCount, msuf4.StageCount)
        Assert.Equal(2, msuf4.TwoOrbitUnfolder4s.Length)
        let rSorter = Msuf4.makeSorter(msuf4)
        // With NextFloat = 0.5 and uniform rates (1/3 each), SeedType.Para is selected ([1/3, 2/3))
        let expectedCes = [| Ce.create 0 2; Ce.create 1 3; Ce.create 0 2; Ce.create 1 3 |]
        Assert.Equal<Ce>(expectedCes, rSorter.Ces)

    [<Fact>]
    let ``makeModelBiased produces Msuf4 with biased last stage`` () =
        let rngType = rngType.Lcg
        let width = 4<sortingWidth>
        let stageCount = 2<stageCount>
        let baseAmt = 1.0 / 3.0
        let biasAmt = 0.1
        let genRates = [| createBiasedGenRates 4 TwoOrbitType.Ortho baseAmt biasAmt; createBiasedGenRates 4 TwoOrbitType.Ortho baseAmt biasAmt;|]
        let msuf4Gen = Msuf4RandGenOld.create rngType width stageCount genRates

        let mockRando = createMockRando [0] [0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5]
        let msuf4 = Msuf4RandGenOld.makeModel msuf4Gen mockRando 0
        Assert.Equal(%msuf4.Id |> UMX.tag<sorterId>, Msuf4.makeSorter(msuf4).SorterId)
        Assert.Equal(width, msuf4.SortingWidth)
        Assert.Equal(%stageCount, msuf4.StageCount)
        Assert.Equal(2, msuf4.TwoOrbitUnfolder4s.Length)
        // First stage: uniform rates, NextFloat = 0.5 -> SeedType.Para ([0;1], [2;3])
        // Last stage: uniform rates, NextFloat = 0.5 -> SeedType.Para ([0;1], [2;3])
        let rSorter = Msuf4.makeSorter(msuf4)
        let expectedCes = [| Ce.create 0 2; Ce.create 1 3; Ce.create 0 2; Ce.create 1 3 |]
        Assert.Equal<Ce>(expectedCes, rSorter.Ces)

    [<Fact>]
    let ``makeModelBiased produces Msuf4 with biased last stage for order 8`` () =
        let rngType = rngType.Lcg
        let width = 8<sortingWidth>
        let stageCount = 1<stageCount>
        let baseAmt = 1.0 / 3.0
        let biasAmt = 0.1
        let genRates = [| createBiasedGenRates 8 TwoOrbitType.Ortho baseAmt biasAmt; |] // createTestGenRates 8; createTestGenRates 8; createTestGenRates 8 |]
        let msuf4Gen = Msuf4RandGenOld.create rngType width stageCount genRates

        let mockRando = createMockRando [0] [0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5] 
        let msuf4 = Msuf4RandGenOld.makeModel msuf4Gen mockRando 0
        let expectedCes = [| Ce.create 0 5; Ce.create 1 4; Ce.create 2 7; Ce.create 3 6 |]
        let rSorter = Msuf4.makeSorter(msuf4)

        Assert.Equal<Ce>(expectedCes, rSorter.Ces)
        Assert.Equal(%msuf4.Id |> UMX.tag<sorterId>, Msuf4.makeSorter(msuf4).SorterId)
        Assert.Equal(width, msuf4.SortingWidth)
        Assert.Equal(%stageCount, msuf4.StageCount)