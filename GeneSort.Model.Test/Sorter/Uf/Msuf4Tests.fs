namespace GeneSort.Model.Test.Sorter.Uf

open System
open FSharp.UMX
open Xunit
open GeneSort.Model.Sorter.Uf4
open GeneSort.Component
open GeneSort.Component.Sorter
open GeneSort.Core
open GeneSort.Model.Sorter

type Msuf4Tests() =

    // Helper function to convert string to SeedType
    let toTwoOrbitPairType (twoOrbitPairTypeStr: string) : TwoOrbitPairType =
        match twoOrbitPairTypeStr with
        | "Ortho" -> TwoOrbitPairType.Ortho
        | "Para" -> TwoOrbitPairType.Para
        | "SelfRefl" -> TwoOrbitPairType.SelfRefl
        | _ -> failwith $"Invalid TwoOrbitPairType string: {twoOrbitPairTypeStr}"

    // Mock floatPicker for deterministic testing
    let mockFloatPicker () = 0.5

    // Helper function to create a TwoOrbitUf4GenRates for a given order
    let createTestGenRates (order: int) :uf4GenRates =
        Uf4GenRates.makeUniform order

    // Helper function that uses mockFloatPicker to create a random TwoOrbitUnfolder4 for a given order, 
    // SeedType, and optional TwoOrbitType override
    let createRandomTestTwoOrbitUf4 
            (order: int) 
            (twoOrbitPairType: TwoOrbitPairType) 
            (twoOrbitPairTypeOverride: TwoOrbitPairType option) 
            : TwoOrbitUf4  =
        let baseGenRates : uf4GenRates = createTestGenRates order
        let orthoRate = if twoOrbitPairType = TwoOrbitPairType.Ortho then 1.0 else 0.0
        let paraRate = if twoOrbitPairType = TwoOrbitPairType.Para then 1.0 else 0.0
        let selfSyymRate = if twoOrbitPairType = TwoOrbitPairType.SelfRefl then 1.0 else 0.0

        let ratesArray = 
                match twoOrbitPairTypeOverride with
                | Some tot -> 
                    let orthoRate = if twoOrbitPairType = TwoOrbitPairType.Ortho then 1.0 else 0.0
                    let paraRate = if twoOrbitPairType = TwoOrbitPairType.Para then 1.0 else 0.0
                    let selfSyymRate = if twoOrbitPairType = TwoOrbitPairType.SelfRefl then 1.0 else 0.0
                    Array.init baseGenRates.OpsGenRatesArray.RatesArray.Length (
                        fun _ -> OpsGenRates.create(orthoRate, paraRate, selfSyymRate))
                | None -> baseGenRates.OpsGenRatesArray.RatesArray

        let genRates : uf4GenRates = uf4GenRates.create 
                                        order
                                        (OpsGenRates.create(orthoRate, paraRate, selfSyymRate))
                                        (OpsGenRatesArray.create ratesArray)

        RandomUnfolderOps4.makeRandomTwoOrbitUf4 mockFloatPicker genRates


    [<Fact>]
    let ``create succeeds with valid input`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 4<sortingWidth>
        let tou = createRandomTestTwoOrbitUf4 4 TwoOrbitPairType.Ortho None
        let msuf4 = msuf4.create id width [| tou |]
        Assert.Equal(id, msuf4.Id)
        Assert.Equal(width, msuf4.SortingWidth)
        Assert.Equal(1, %msuf4.StageLength)
        Assert.Equal<TwoOrbitUf4 array>([| tou |], msuf4.TwoOrbitUnfolder4s)

    [<Fact>]
    let ``create fails with empty TwoOrbitUnfolder4 array`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 4<sortingWidth>
        let ex = Assert.Throws<Exception>(fun () -> msuf4.create id width [||] |> ignore)
        Assert.Equal("Must have at least 1 TwoOrbitUnfolder4, got 0", ex.Message)

    [<Fact>]
    let ``create fails with invalid width`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 0<sortingWidth>
        let tou = createRandomTestTwoOrbitUf4 4 TwoOrbitPairType.Ortho None
        let ex = Assert.Throws<Exception>(fun () -> msuf4.create id width [| tou |] |> ignore)
        Assert.Equal("SortingWidth must be at least 1, got 0", ex.Message)

    [<Fact>]
    let ``create fails with mismatched TwoOrbitUnfolder4 order`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 8<sortingWidth>
        let tou = createRandomTestTwoOrbitUf4 4 TwoOrbitPairType.Ortho None
        let ex = Assert.Throws<Exception>(fun () -> msuf4.create id width [| tou |] |> ignore)
        Assert.Equal("All TwoOrbitUnfolder4 must have order 8", ex.Message)

    [<Theory>]
    [<InlineData("Ortho", 0, 1, 2, 3)>]
    [<InlineData("Para", 0, 2, 1, 3)>]
    [<InlineData("SelfRefl", 0, 3, 1, 2)>]
    let ``makeSorter produces correct Sorter for order 4`` (seedTypeStr: string) (low1: int) (hi1: int) (low2: int) (hi2: int) =
        let seedType = toTwoOrbitPairType seedTypeStr
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 4<sortingWidth>
        let tou = createRandomTestTwoOrbitUf4 4 seedType None
        let msuf4 = msuf4.create id width [| tou |]
        let sorter = msuf4.MakeSorter()
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.SortingWidth)
        let expectedCes = [| ce.create low1 hi1; ce.create low2 hi2 |]
        Assert.Equal<ce>(expectedCes, sorter.Ces)

    [<Fact>]
    let ``makeSorter handles order 8 correctly with Ortho seed and Para unfolding`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 8<sortingWidth>
        let seedType = TwoOrbitPairType.Ortho
        let tou = TwoOrbitUnfolder4.makeTestTwoOrbitUf4 seedType TwoOrbitPairType.Para 8
        let msuf4 = msuf4.create id width [| tou |]
        let sorter = msuf4.MakeSorter()
        let expectedCes = [| ce.create 0 6; ce.create 1 7; ce.create 2 4; ce.create 3 5 |]
        Assert.Equal<ce>(expectedCes, sorter.Ces)
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.SortingWidth)

    [<Fact>]
    let ``makeSorter handles order 8 correctly with Ortho seed and Ortho unfolding`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 8<sortingWidth>
        let seedType = TwoOrbitPairType.Ortho
        let tou = TwoOrbitUnfolder4.makeTestTwoOrbitUf4 seedType TwoOrbitPairType.Ortho 8
        let msuf4 = msuf4.create id width [| tou |]
        let sorter = msuf4.MakeSorter()
        let expectedCes = [| ce.create 0 1; ce.create 2 3; ce.create 4 5; ce.create 6 7 |]
        Assert.Equal<ce>(expectedCes, sorter.Ces)
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.SortingWidth)

    [<Fact>]
    let ``makeSorter handles order 8 correctly with Ortho seed and SelfRefl unfolding`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 8<sortingWidth>
        let seedType = TwoOrbitPairType.Ortho
        let tou = TwoOrbitUnfolder4.makeTestTwoOrbitUf4 seedType TwoOrbitPairType.SelfRefl 8
        let msuf4 = msuf4.create id width [| tou |]
        let sorter = msuf4.MakeSorter()
        let expectedCes = [| ce.create 0 7; ce.create 1 6; ce.create 2 5; ce.create 3 4 |]
        Assert.Equal<ce>(expectedCes, sorter.Ces)
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.SortingWidth)

    [<Fact>]
    let ``makeSorter handles order 16 correctly with Ortho seed and Para unfolding`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let order = 16
        let width = 16<sortingWidth>
        let seedType = TwoOrbitPairType.Ortho
        let tou = TwoOrbitUnfolder4.makeTestTwoOrbitUf4 seedType TwoOrbitPairType.Para order
        let msuf4 = msuf4.create id width [| tou |]
        let sorter = msuf4.MakeSorter()
        let expectedCes = [| ce.create 0 9; ce.create 1 8; ce.create 2 11; ce.create 3 10
                             ce.create 4 13; ce.create 5 12; ce.create 6 15; ce.create 7 14 |]
        Assert.Equal<ce>(expectedCes, sorter.Ces)
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.SortingWidth)

    [<Fact>]
    let ``equality based on id`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let order = 16
        let width = 16<sortingWidth>
        let tou1 = TwoOrbitUnfolder4.makeTestTwoOrbitUf4 TwoOrbitPairType.Ortho TwoOrbitPairType.Para order
        let tou2 = TwoOrbitUnfolder4.makeTestTwoOrbitUf4 TwoOrbitPairType.Para TwoOrbitPairType.Para order
        let msuf4_1 = msuf4.create id width [| tou1 |]
        let msuf4_2 = msuf4.create id width [| tou1; tou2 |]
        Assert.Equal(msuf4_1, msuf4_2)
        Assert.Equal(msuf4_1.GetHashCode(), msuf4_2.GetHashCode())

    [<Fact>]
    let ``inequality with different ids`` () =
        let id1 = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let id2 = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let order = 16
        let width = 16<sortingWidth>
        let tou = TwoOrbitUnfolder4.makeTestTwoOrbitUf4 TwoOrbitPairType.Ortho TwoOrbitPairType.Para order
        let msuf4_1 = msuf4.create id1 width [| tou |]
        let msuf4_2 = msuf4.create id2 width [| tou |]
        Assert.NotEqual(msuf4_1, msuf4_2)
        Assert.NotEqual(msuf4_1.GetHashCode(), msuf4_2.GetHashCode())