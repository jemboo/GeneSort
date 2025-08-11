namespace GeneSort.Model.Test.Sorter.Uf

open System
open FSharp.UMX
open Xunit
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter
open GeneSort.Core
open GeneSort.Sorter

type Msuf4Tests() =

    // Helper function to convert string to SeedType
    //let toSeedType (seedTypeStr: string) : SeedTypeUf4 =
    //    match seedTypeStr with
    //    | "Ortho" -> SeedTypeUf4.Ortho
    //    | "Para" -> SeedTypeUf4.Para
    //    | "SelfRefl" -> SeedTypeUf4.SelfRefl
    //    | _ -> failwith $"Invalid SeedType string: {seedTypeStr}"

    // Mock floatPicker for deterministic testing
    let mockFloatPicker () = 0.5

    // Helper function to create a TwoOrbitUf4GenRates for a given order
    let createTestGenRates (order: int) =
        Uf4GenRates.makeUniform order

    // Helper function to create a valid TwoOrbitUnfolder4 for a given order, SeedType, and optional TwoOrbitType override
    let createTestTwoOrbitUnfolder4 
            (order: int) 
            //(seedType: TwoOrbitType) 
            //(twoOrbitTypeOverride: TwoOrbitType option) 
     : TwoOrbitUf4  =
        let baseGenRates = createTestGenRates order
        let genRates : Uf4GenRates = Uf4GenRates.makeUniform order
            //{ 
            //  Uf4GenRates.order = baseGenRates.order
            //  seedOpsGenRates = OpsGenRates.createUniform ()
            //        //{ Ortho = if seedType = SeedTypeUf4.Ortho then 1.0 else 0.0
            //        //  Para = if seedType = SeedTypeUf4.Para then 1.0 else 0.0
            //        //  SelfRefl = if seedType = SeedTypeUf4.SelfRefl then 1.0 else 0.0 }

            //  opsGenRatesList = 
            //      match twoOrbitTypeOverride with
            //      | Some tot -> 
            //          List.init baseGenRates.opsGenRatesList.Length (fun _ ->  
            //              { TwoOrbitPairGenRates.Ortho = if tot = TwoOrbitType.Ortho then 1.0 else 0.0
            //                Para = if tot = TwoOrbitType.Para then 1.0 else 0.0
            //                SelfReflections = if tot = TwoOrbitType.SelfRefl then 1.0 else 0.0 })
            //      | None -> baseGenRates.opsGenRatesList }
        UnfolderOps4.makeTwoOrbitUf4 mockFloatPicker genRates

    // Helper function to create a valid Msuf4
    let createTestMsuf4 
                (id: Guid<sorterModelID>) 
                (width: int<sortingWidth>) 
                (count: int)
                : Msuf4 =
        let tou = createTestTwoOrbitUnfolder4 (%width)
        let touArray = Array.create count tou
        Msuf4.create id width touArray

    [<Fact>]
    let ``create succeeds with valid input`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 4<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 4
        let msuf4 = Msuf4.create id width [| tou |]
        Assert.Equal(id, msuf4.Id)
        Assert.Equal(width, msuf4.SortingWidth)
        Assert.Equal(1, %msuf4.StageCount)
        Assert.Equal<TwoOrbitUf4 array>([| tou |], msuf4.TwoOrbitUnfolder4s)

    [<Fact>]
    let ``create fails with empty TwoOrbitUnfolder4 array`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 4<sortingWidth>
        let ex = Assert.Throws<Exception>(fun () -> Msuf4.create id width [||] |> ignore)
        Assert.Equal("Must have at least 1 TwoOrbitUnfolder4, got 0", ex.Message)

    [<Fact>]
    let ``create fails with invalid width`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 0<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 4
        let ex = Assert.Throws<Exception>(fun () -> Msuf4.create id width [| tou |] |> ignore)
        Assert.Equal("SortingWidth must be at least 1, got 0", ex.Message)

    [<Fact>]
    let ``create fails with mismatched TwoOrbitUnfolder4 order`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 8<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 4
        let ex = Assert.Throws<Exception>(fun () -> Msuf4.create id width [| tou |] |> ignore)
        Assert.Equal("All TwoOrbitUnfolder4 must have order 8", ex.Message)

    [<Theory>]
    [<InlineData("Ortho", 0, 1, 2, 3)>]
    [<InlineData("Para", 0, 2, 1, 3)>]
    [<InlineData("SelfRefl", 0, 3, 1, 2)>]
    let ``makeSorter produces correct Sorter for order 4`` (seedTypeStr: string) (low1: int) (hi1: int) (low2: int) (hi2: int) =
        //let seedType = toSeedType seedTypeStr
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 4<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 4
        let msuf4 = Msuf4.create id width [| tou |]
        let sorter = (msuf4 :> ISorterModel).MakeSorter()
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.Width)
        let expectedCes = [| Ce.create low1 hi1; Ce.create low2 hi2 |]
        Assert.Equal<Ce>(expectedCes, sorter.Ces)

    [<Fact>]
    let ``makeSorter handles order 8 correctly with Ortho seed and Para unfolding`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 8<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 8
        let msuf4 = Msuf4.create id width [| tou |]
        let sorter = (msuf4 :> ISorterModel).MakeSorter()
        let expectedCes = [| Ce.create 0 6; Ce.create 1 7; Ce.create 2 4; Ce.create 3 5 |]
        Assert.Equal<Ce>(expectedCes, sorter.Ces)
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.Width)

    [<Fact>]
    let ``makeSorter handles order 8 correctly with Ortho seed and Ortho unfolding`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 8<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 8
        let msuf4 = Msuf4.create id width [| tou |]
        let sorter = (msuf4 :> ISorterModel).MakeSorter()
        let expectedCes = [| Ce.create 0 1; Ce.create 2 3; Ce.create 4 5; Ce.create 6 7 |]
        Assert.Equal<Ce>(expectedCes, sorter.Ces)
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.Width)

    [<Fact>]
    let ``makeSorter handles order 8 correctly with Ortho seed and SelfRefl unfolding`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 8<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 8
        let msuf4 = Msuf4.create id width [| tou |]
        let sorter = (msuf4 :> ISorterModel).MakeSorter()
        let expectedCes = [| Ce.create 0 7; Ce.create 1 6; Ce.create 2 5; Ce.create 3 4 |]
        Assert.Equal<Ce>(expectedCes, sorter.Ces)
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.Width)

    [<Fact>]
    let ``makeSorter handles order 16 correctly with Ortho seed and Para unfolding`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 16<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 16
        let msuf4 = Msuf4.create id width [| tou |]
        let sorter = (msuf4 :> ISorterModel).MakeSorter()
        let expectedCes = [| Ce.create 0 9; Ce.create 1 8; Ce.create 2 11; Ce.create 3 10
                             Ce.create 4 13; Ce.create 5 12; Ce.create 6 15; Ce.create 7 14 |]
        Assert.Equal<Ce>(expectedCes, sorter.Ces)
        Assert.Equal(%id |> UMX.tag<sorterId>, sorter.SorterId)
        Assert.Equal(width, sorter.Width)

    [<Fact>]
    let ``toString returns correct format`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 4<sortingWidth>
        let msuf4 = createTestMsuf4 id width 2
        let expected = sprintf "Msuf4(Id=%A, SortingWidth=%d, TwoOrbitUnfolder4Count=%d)" (%id) (%width) 2
        Assert.Equal(expected, Msuf4.toString msuf4)

    [<Fact>]
    let ``equality based on id`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 4<sortingWidth>
        let tou1 = createTestTwoOrbitUnfolder4 4
        let tou2 = createTestTwoOrbitUnfolder4 4
        let msuf4_1 = Msuf4.create id width [| tou1 |]
        let msuf4_2 = Msuf4.create id width [| tou1; tou2 |]
        Assert.Equal(msuf4_1, msuf4_2)
        Assert.Equal(msuf4_1.GetHashCode(), msuf4_2.GetHashCode())

    [<Fact>]
    let ``inequality with different ids`` () =
        let id1 = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let id2 = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 4<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 4
        let msuf4_1 = Msuf4.create id1 width [| tou |]
        let msuf4_2 = Msuf4.create id2 width [| tou |]
        Assert.NotEqual(msuf4_1, msuf4_2)
        Assert.NotEqual(msuf4_1.GetHashCode(), msuf4_2.GetHashCode())