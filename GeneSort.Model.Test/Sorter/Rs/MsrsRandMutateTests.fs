namespace GeneSort.Model.Test.Sorter.Rs

open System
open Xunit
open FSharp.UMX
open FsUnit.Xunit
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter

type MsrsRandMutateTests() =

    // Helper function to create a Perm_Rs from an array
    let createPermRs (arr: int array) : Perm_Rs =
        Perm_Rs.create arr

    // Helper to create a Model_Rs
    let createModelRs (id: Guid<sortingModelID>) (width: int<sortingWidth>) (permRss: Perm_Rs array) : msrs =
        msrs.create id width permRss

    // Helper to check if a permutation is self-inverse
    let isSelfInverse (perm: Perm_Rs) : bool =
        let arr = perm.Array
        arr |> Array.indexed |> Array.forall (fun (i, x) -> arr.[x] = i)

    // Helper function to create a mock random number generator
    let createMockRando (indices: int list) (floats: float list) =
        fun _ _ -> new MockRando(floats, indices) :> IRando

    [<Fact>]
    let ``NoAction mode preserves all Perm_Rss`` () =
        let id = Guid.NewGuid() |> UMX.tag<sortingModelID>
        let width = 4<sortingWidth>
        let permRss = [| [| 3; 2; 1; 0 |]; [| 1; 0; 3; 2 |] |] |> Array.map createPermRs
        let modelRs = createModelRs id width permRss
        let opsActionRates = OpsActionRates.create (0.0, 0.0, 0.0) // NoActionRate = 1.0
        let ratesArray = OpsActionRatesArray.create [| opsActionRates; opsActionRates |]
        let modelRsMutate = msrsRandMutate.create rngType.Lcg modelRs ratesArray

        let randoGen = createMockRando [ 0; 1; 0; 1 ] [ 0.1; 0.1 ] // Should pick NoAction
        let result = modelRsMutate.MakeSorterModel randoGen 0
        Assert.Equal(width, result.SortingWidth)
        Assert.Equal(2<stageLength>, result.StageLength)
        Assert.Equal<Perm_Rs array>(permRss, result.Perm_Rss)

    [<Fact>]
    let ``SelfSym mode mutates Perm_Rss correctly`` () =
        let id = Guid.NewGuid() |> UMX.tag<sortingModelID>
        let width = 4<sortingWidth>
        let permRss = [| [| 1; 0; 3; 2 |]; [| 1; 0; 3; 2 |] |] |> Array.map createPermRs
        let expectedPermRss = [| [| 3; 2; 1; 0 |]; [| 3; 2; 1; 0 |] |] |> Array.map createPermRs
        let modelRs = createModelRs id width permRss
        let opsActionRates = OpsActionRates.create (0.0, 0.0, 1.0) // SelfSymRate = 1.0
        let ratesArray = OpsActionRatesArray.create [| opsActionRates; opsActionRates |]
        let modelRsMutate = msrsRandMutate.create rngType.Lcg modelRs ratesArray

        let randoGen = createMockRando [ 0; 1; 0; 1 ] [ 0.1; 0.1 ] // Should pick SelfSym
        let result = modelRsMutate.MakeSorterModel randoGen 0
        Assert.Equal(width, result.SortingWidth)
        Assert.Equal(2<stageLength>, result.StageLength)
        Assert.Equal<Perm_Rs array>(expectedPermRss, result.Perm_Rss)
        Assert.True(result.Perm_Rss |> Array.forall isSelfInverse)

    [<Fact>]
    let ``Ortho mode mutates Perm_Rss correctly`` () =
        let id = Guid.NewGuid() |> UMX.tag<sortingModelID>
        let width = 4<sortingWidth>
        let permRss = [| [| 1; 0; 3; 2 |]; [| 1; 0; 3; 2 |] |] |> Array.map createPermRs
        let expectedPermRss = [| [| 1; 0; 3; 2 |]; [| 1; 0; 3; 2 |] |] |> Array.map createPermRs
        let modelRs = createModelRs id width permRss
        let opsActionRates = OpsActionRates.create (1.0, 0.0, 0.0) // OrthoRate = 1.0
        let ratesArray = OpsActionRatesArray.create [| opsActionRates; opsActionRates |]
        let modelRsMutate = msrsRandMutate.create rngType.Lcg modelRs ratesArray

        let randoGen = createMockRando [ 0; 1; 0; 1 ] [ 0.1; 0.1 ] // Should pick Ortho
        let result = modelRsMutate.MakeSorterModel randoGen 0
        Assert.Equal(width, result.SortingWidth)
        Assert.Equal(2<stageLength>, result.StageLength)
        Assert.Equal<Perm_Rs array>(expectedPermRss, result.Perm_Rss)
        Assert.True(result.Perm_Rss |> Array.forall isSelfInverse)

    [<Fact>]
    let ``Para mode mutates Perm_Rss correctly`` () =
        let id = Guid.NewGuid() |> UMX.tag<sortingModelID>
        let width = 4<sortingWidth>
        let permRss = [| [| 1; 0; 3; 2 |]; [| 1; 0; 3; 2 |] |] |> Array.map createPermRs
        let expectedPermRss = [| [| 2; 3; 0; 1 |]; [| 2; 3; 0; 1 |] |] |> Array.map createPermRs
        let modelRs = createModelRs id width permRss
        let opsActionRates = OpsActionRates.create (0.0, 1.0, 0.0) // ParaRate = 1.0
        let ratesArray = OpsActionRatesArray.create [| opsActionRates; opsActionRates |]
        let modelRsMutate = msrsRandMutate.create rngType.Lcg modelRs ratesArray

        let randoGen = createMockRando [ 0; 1; 0; 1 ] [ 0.1; 0.1 ] // Should pick Para
        let result = modelRsMutate.MakeSorterModel randoGen 0
        Assert.Equal(width, result.SortingWidth)
        Assert.Equal(2<stageLength>, result.StageLength)
        Assert.Equal<Perm_Rs array>(expectedPermRss, result.Perm_Rss)
        Assert.True(result.Perm_Rss |> Array.forall isSelfInverse)

    [<Fact>]
    let ``Mixed mutation modes with varying probabilities`` () =
        let id = Guid.NewGuid() |> UMX.tag<sortingModelID>
        let width = 4<sortingWidth>
        let permRss = [| [| 1; 0; 3; 2 |]; [| 1; 0; 3; 2 |] |] |> Array.map createPermRs
        let modelRs = createModelRs id width permRss
        let opsActionRates1 = OpsActionRates.create (0.0, 0.0, 1.0) // SelfSymRate = 1.0
        let opsActionRates2 = OpsActionRates.create (0.0, 1.0, 0.0) // ParaRate = 1.0
        let ratesArray = OpsActionRatesArray.create [| opsActionRates1; opsActionRates2 |]
        let modelRsMutate = msrsRandMutate.create rngType.Lcg modelRs ratesArray

        let randoGen = createMockRando [ 0; 1; 0; 1 ] [ 0.1; 0.1 ] // Picks SelfSym for first, Para for second
        let result = modelRsMutate.MakeSorterModel randoGen 0
        let expectedPermRss = [| [| 3; 2; 1; 0 |]; [| 2; 3; 0; 1 |] |] |> Array.map createPermRs
        Assert.Equal(width, result.SortingWidth)
        Assert.Equal(2<stageLength>, result.StageLength)
        Assert.Equal<Perm_Rs array>(expectedPermRss, result.Perm_Rss)
        Assert.True(result.Perm_Rss |> Array.forall isSelfInverse)

    [<Fact>]
    let ``Throws on mismatched stage count`` () =
        let id = Guid.NewGuid() |> UMX.tag<sortingModelID>
        let width = 4<sortingWidth>
        let permRss = [| [| 1; 0; 3; 2 |] |] |> Array.map createPermRs // StageLength = 1
        let modelRs = createModelRs id width permRss
        let opsActionRates = OpsActionRates.create (0.0, 0.0, 0.0)
        let ratesArray = OpsActionRatesArray.create [| opsActionRates; opsActionRates |] // Length = 2
        Assert.ThrowsAny<exn>(fun () ->
            msrsRandMutate.create rngType.Lcg modelRs ratesArray|> ignore
        )

    [<Fact>]
    let ``Generates new unique ID`` () =
        let id = Guid.NewGuid() |> UMX.tag<sortingModelID>
        let width = 4<sortingWidth>
        let permRss = [| [| 1; 0; 3; 2 |] |] |> Array.map createPermRs
        let modelRs = createModelRs id width permRss
        let opsActionRates = OpsActionRates.create (0.0, 0.0, 0.0) // NoActionRate = 1.0
        let ratesArray = OpsActionRatesArray.create [| opsActionRates |]
        let modelRsMutate = msrsRandMutate.create rngType.Lcg modelRs ratesArray

        let randoGen = createMockRando [ 0; 1 ] [ 0.9 ]
        let result = modelRsMutate.MakeSorterModel randoGen 0
        Assert.NotEqual(%id, %result.Id)
        Assert.NotEqual(Guid.Empty, %result.Id)