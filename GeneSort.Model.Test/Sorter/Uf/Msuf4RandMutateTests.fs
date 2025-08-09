namespace GeneSort.Model.Test.Sorter.Uf

open System
open FSharp.UMX
open Xunit
open EvoMergeSort.Core
open EvoMergeSort.Sorter
open Uf4Seeds
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter

type Msuf4RandMutateTests() =

    // Helper function to create a mock random number generator
    let createMockRando (indices: int list) (floats: float list) =
        fun _ _ -> new MockRando(floats, indices) :> IRando

    // Helper function to create a valid TwoOrbitUnfolder4
    let createTestTwoOrbitUnfolder4 (order: int) (seedType: SeedTypeUf4) =
        let genRates = Uf4GenRates.makeUniform order
        TwoOrbitUf4Ops.makeTwoOrbitUf4 (fun () -> 0.5) genRates

    // Helper function to create a valid Msuf4
    let createTestMsuf4 (id: Guid<sorterModelID>) (width: int<sortingWidth>) (count: int) (seedType: SeedTypeUf4) =
        let tou = createTestTwoOrbitUnfolder4 (%width) seedType
        let touArray = Array.create count tou
        Msuf4.create id width touArray

    // Helper function to create a valid Msuf4MutationModeProbabilities
    let createTestMutationModeProbabilities (order: int) (otp: float) (ots: float) (pto: float) (pts: float) (sto: float) (stp: float) =
        Msuf4MutationModeProbabilities.create 
            order 
            (otp |> LanguagePrimitives.FloatWithMeasure<uf4OrthoToParaMutationRate>)
            (ots |> LanguagePrimitives.FloatWithMeasure<uf4OrthoToSelfReflMutationRate>)
            (pto |> LanguagePrimitives.FloatWithMeasure<uf4ParaToOrthoMutationRate>)
            (pts |> LanguagePrimitives.FloatWithMeasure<uf4ParaToSelfReflMutationRate>)
            (sto |> LanguagePrimitives.FloatWithMeasure<uf4SelfReflToOrthoMutationRate>)
            (stp |> LanguagePrimitives.FloatWithMeasure<uf4SelfReflToParaMutationRate>)

    // Helper function to create a valid Msuf4RandMutate
    let createTestMsuf4RandMutate (rngType: rngType) (stageCount: int<stageCount>) (order: int) (otp: float) (ots: float) (pto: float) (pts: float) (sto: float) (stp: float) =
        let rates = Array.init (%stageCount) (fun _ -> otp, ots, pto, pts, sto, stp)
        let orthoToParaRates = rates |> Array.map (fun (otp, _, _, _, _, _) -> otp |> LanguagePrimitives.FloatWithMeasure)
        let orthoToSelfReflRates = rates |> Array.map (fun (_, ots, _, _, _, _) -> ots |> LanguagePrimitives.FloatWithMeasure)
        let paraToOrthoRates = rates |> Array.map (fun (_, _, pto, _, _, _) -> pto |> LanguagePrimitives.FloatWithMeasure)
        let paraToSelfReflRates = rates |> Array.map (fun (_, _, _, pts, _, _) -> pts |> LanguagePrimitives.FloatWithMeasure)
        let selfReflToOrthoRates = rates |> Array.map (fun (_, _, _, _, sto, _) -> sto |> LanguagePrimitives.FloatWithMeasure)
        let selfReflToParaRates = rates |> Array.map (fun (_, _, _, _, _, stp) -> stp |> LanguagePrimitives.FloatWithMeasure)
        Msuf4RandMutateOld.create rngType stageCount order orthoToParaRates orthoToSelfReflRates paraToOrthoRates paraToSelfReflRates selfReflToOrthoRates selfReflToParaRates

    [<Fact>]
    let ``create succeeds with valid input`` () =
        let rngType = rngType.Lcg
        let stageCount = 2<stageCount>
        let order = 4
        let otp = 0.2
        let ots = 0.2
        let pto = 0.2
        let pts = 0.2
        let sto = 0.1
        let stp = 0.1
        let probabilities = [| createTestMutationModeProbabilities order otp ots pto pts sto stp; createTestMutationModeProbabilities order otp ots pto pts sto stp |]
        let msuf4Mutate = createTestMsuf4RandMutate rngType stageCount order otp ots pto pts sto stp
        Assert.Equal(rngType, msuf4Mutate.RngType)
        Assert.Equal(stageCount, msuf4Mutate.StageCount)

    [<Fact>]
    let ``createFromSingleRate succeeds with valid input`` () =
        let rngTyp = rngType.Lcg
        let stageCount = 2<stageCount>
        let order = 4
        let otp = 0.2
        let ots = 0.2
        let pto = 0.2
        let pts = 0.2
        let sto = 0.1
        let stp = 0.1
        let msuf4Mutate = Msuf4RandMutateOld.createFromSingleRate 
                            rngTyp stageCount order 
                            (otp |> LanguagePrimitives.FloatWithMeasure)
                            (ots |> LanguagePrimitives.FloatWithMeasure)
                            (pto |> LanguagePrimitives.FloatWithMeasure)
                            (pts |> LanguagePrimitives.FloatWithMeasure)
                            (sto |> LanguagePrimitives.FloatWithMeasure)
                            (stp |> LanguagePrimitives.FloatWithMeasure)
        Assert.Equal(rngTyp, msuf4Mutate.RngType)
        Assert.Equal(stageCount, msuf4Mutate.StageCount)
        Assert.All(msuf4Mutate.OrthoToParaRates, fun rate -> Assert.Equal(otp, %rate))
        Assert.All(msuf4Mutate.OrthoToSelfReflRates, fun rate -> Assert.Equal(ots, %rate))
        Assert.All(msuf4Mutate.ParaToOrthoRates, fun rate -> Assert.Equal(pto, %rate))
        Assert.All(msuf4Mutate.ParaToSelfReflRates, fun rate -> Assert.Equal(pts, %rate))
        Assert.All(msuf4Mutate.SelfReflToOrthoRates, fun rate -> Assert.Equal(sto, %rate))
        Assert.All(msuf4Mutate.SelfReflToParaRates, fun rate -> Assert.Equal(stp, %rate))


    [<Fact>]
    let ``equality based on all fields`` () =
        let rngType = rngType.Lcg
        let stageCount = 2<stageCount>
        let order = 4
        let otp = 0.2
        let ots = 0.2
        let pto = 0.2
        let pts = 0.2
        let sto = 0.1
        let stp = 0.1
        let msuf4Mutate1 = createTestMsuf4RandMutate rngType stageCount order otp ots pto pts sto stp
        let msuf4Mutate2 = createTestMsuf4RandMutate rngType stageCount order otp ots pto pts sto stp
        Assert.Equal(msuf4Mutate1, msuf4Mutate2)
        Assert.Equal(msuf4Mutate1.GetHashCode(), msuf4Mutate2.GetHashCode())

    [<Fact>]
    let ``inequality with different fields`` () =
        let rngType1 = rngType.Lcg
        let rngType2 = rngType.Net
        let stageCount = 1<stageCount>
        let order = 4
        let otp1 = 0.02
        let ots1 = 0.2
        let pto1 = 0.2
        let pts1 = 0.2
        let sto1 = 0.1
        let stp1 = 0.1
        let otp2 = 0.03
        let ots2 = 0.3
        let pto2 = 0.3
        let pts2 = 0.03
        let sto2 = 0.15
        let stp2 = 0.15
        let msuf4Mutate1 = createTestMsuf4RandMutate rngType1 stageCount order otp1 ots1 pto1 pts1 sto1 stp1
        let msuf4Mutate2 = createTestMsuf4RandMutate rngType2 stageCount order otp1 ots1 pto1 pts1 sto1 stp1
        let msuf4Mutate3 = createTestMsuf4RandMutate rngType1 stageCount order otp2 ots2 pto2 pts2 sto2 stp2
        Assert.NotEqual(msuf4Mutate1, msuf4Mutate2)
        Assert.NotEqual(msuf4Mutate1, msuf4Mutate3)
        Assert.NotEqual(msuf4Mutate1.GetHashCode(), msuf4Mutate2.GetHashCode())
        Assert.NotEqual(msuf4Mutate1.GetHashCode(), msuf4Mutate3.GetHashCode())

    [<Fact>]
    let ``makeId generates unique IDs`` () =
        let rngType = rngType.Lcg
        let stageCount = 1<stageCount>
        let order = 4<sortingWidth>
        let msuf4Mutate = createTestMsuf4RandMutate rngType stageCount %order 0.2 0.2 0.2 0.2 0.1 0.1
        let msuf4Id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let msuf4 = createTestMsuf4 msuf4Id order 1 SeedTypeUf4.Ortho
        let id1 = Msuf4RandMutateOld.makeId msuf4Mutate msuf4 0
        let id2 = Msuf4RandMutateOld.makeId msuf4Mutate msuf4 1
        Assert.NotEqual(id1, id2)

    [<Fact>]
    let ``mutate produces Msuf4 with correct properties for order 4`` () =
        let rngType = rngType.Lcg
        let stageCount = 2<stageCount>
        let order = 4<sortingWidth>
        let msuf4Id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let msuf4 = createTestMsuf4 msuf4Id order (%stageCount) SeedTypeUf4.Ortho
        let msuf4Mutate = createTestMsuf4RandMutate rngType stageCount %order 0.1 0.1 0.1 0.1 0.05 0.05 // Sum = 0.5, NoAction = 0.5
        let mockRando = createMockRando [0] [0.6; 0.6; 0.6; 0.6; 0.6; 0.6; 0.6; 0.6] // Selects None (no mutation)
        let mutatedMsuf4 = Msuf4RandMutateOld.mutate mockRando msuf4Mutate msuf4 0
        Assert.Equal(%mutatedMsuf4.Id |> UMX.tag<sorterId>, Msuf4.makeSorter(mutatedMsuf4).SorterId)
        Assert.Equal(order, mutatedMsuf4.SortingWidth)
        Assert.Equal(%stageCount, mutatedMsuf4.StageCount)
        Assert.Equal(2, mutatedMsuf4.TwoOrbitUnfolder4s.Length)
        // SeedType.Ortho ([0;2], [1;3]), picker selects None (0.6 > 0.5) -> no mutation
        let expectedCes = [| Ce.create 0 2; Ce.create 1 3; Ce.create 0 2; Ce.create 1 3 |]
        Assert.Equal<Ce>(expectedCes, Msuf4.makeSorter(mutatedMsuf4).Ces)

    [<Fact>]
    let ``mutate produces Msuf4 with mutated SeedType for order 4`` () =
        let rngType = rngType.Lcg
        let stageCount = 1<stageCount>
        let order = 4<sortingWidth>
        let msuf4Id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let msuf4 = createTestMsuf4 msuf4Id order (%stageCount) SeedTypeUf4.Ortho
        let msuf4Mutate = createTestMsuf4RandMutate rngType stageCount %order 0.8 0.1 0.0 0.0 0.0 0.0 // Sum = 0.9, NoAction = 0.1
        let mockRando = createMockRando [0] [0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5] // Selects Some (0.5 < 0.9)
        let mutatedMsuf4 = Msuf4RandMutateOld.mutate mockRando msuf4Mutate msuf4 0
        // SeedType.Ortho, mutation rates: OrthoToPara=0.8, OrthoToSelfRefl=0.1, Ortho=0.1
        // NextFloat = 0.5 selects Para ([0.1, 0.9)) -> [0;2], [1;3]
        let expectedCes = [| Ce.create 0 2; Ce.create 1 3 |]
        let rSorter = Msuf4.makeSorter(mutatedMsuf4)
        Assert.Equal<Ce>(expectedCes, rSorter.Ces)
        Assert.Equal(%mutatedMsuf4.Id |> UMX.tag<sorterId>, Msuf4.makeSorter(mutatedMsuf4).SorterId)
        Assert.Equal(order, mutatedMsuf4.SortingWidth)
        Assert.Equal(%stageCount, mutatedMsuf4.StageCount)

    [<Fact>]
    let ``mutate produces Msuf4 with mutated SeedType for order 8`` () =
        let rngType = rngType.Lcg
        let stageCount = 1<stageCount>
        let order = 8<sortingWidth>
        let msuf4Id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let msuf4 = createTestMsuf4 msuf4Id order (%stageCount) SeedTypeUf4.Ortho
        let msuf4Mutate = createTestMsuf4RandMutate rngType stageCount %order 0.08 0.1 0.0 0.0 0.0 0.0
        let mockRando = createMockRando [0] [0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5] // Selects Some (0.5 < 0.9)
        let mutatedMsuf4 = Msuf4RandMutateOld.mutate mockRando msuf4Mutate msuf4 0
        // SeedType.Ortho, mutation to Para
        let expectedCes = [| Ce.create 0 5; Ce.create 1 4; Ce.create 2 7; Ce.create 3 6 |]
        let rSorter = Msuf4.makeSorter(mutatedMsuf4)
        Assert.Equal<Ce>(expectedCes, rSorter.Ces)
        Assert.Equal(%mutatedMsuf4.Id |> UMX.tag<sorterId>, Msuf4.makeSorter(mutatedMsuf4).SorterId)
        Assert.Equal(order, mutatedMsuf4.SortingWidth)
        Assert.Equal(%stageCount, mutatedMsuf4.StageCount)

    [<Fact>]
    let ``mutate fails with mismatched stageCount`` () =
        let rngType = rngType.Lcg
        let stageCount = 2<stageCount>
        let order = 4<sortingWidth>
        let msuf4Id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let msuf4 = createTestMsuf4 msuf4Id order 1 SeedTypeUf4.Ortho // Count = 1
        let msuf4Mutate = createTestMsuf4RandMutate rngType stageCount %order 0.2 0.2 0.2 0.2 0.1 0.1
        let mockRando = createMockRando [0] [0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5]
        let ex = Assert.Throws<Exception>(fun () -> Msuf4RandMutateOld.mutate mockRando msuf4Mutate msuf4 0 |> ignore)
        Assert.Equal("Stage count of Msuf4 1 must match Msuf4RandMutate 2", ex.Message)

    [<Fact>]
    let ``mutate fails with mismatched mutationRates order`` () =
        let rngType = rngType.Lcg
        let stageCount = 1<stageCount>
        let order = 4<sortingWidth>
        let msuf4Id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let msuf4 = createTestMsuf4 msuf4Id order (%stageCount) SeedTypeUf4.Ortho
        let msuf4Mutate = createTestMsuf4RandMutate rngType stageCount 8 0.2 0.2 0.2 0.2 0.1 0.1 // Order 8, should be 4
        let mockRando = createMockRando [0] [0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5; 0.5]
        let ex = Assert.Throws<Exception>(fun () -> Msuf4RandMutateOld.mutate mockRando msuf4Mutate msuf4 0 |> ignore)
        Assert.Equal("All mutationRates must have order 4", ex.Message)