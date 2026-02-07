namespace GeneSort.Model.Test.Sorter.Rs

open System
open Xunit
open FSharp.UMX
open FsUnit.Xunit
open GeneSort.Core
open GeneSort.Component
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sorter

type MsrsRandGenTests() =
    // Helper function to create a mock random number generator
    let createMockRando (indices: int list) (floats: float list) =
        fun _ _ -> new MockRando(floats, indices) :> IRando

    // Helper function to create an MsrsRandGen instance
    let createModelRsGen rngType (width:int) stageLength orthoRate paraRate selfSymRate =
        let rates = OpsGenRates.create (orthoRate, paraRate, selfSymRate)
        let ratesArray = OpsGenRatesArray.create (Array.create stageLength rates)
        msrsRandGen.create rngType (UMX.tag<sortingWidth> width)  ratesArray

    [<Fact>]
    let ``MakeSorterModel creates correct number of stages`` () =
        let model = createModelRsGen rngType.Lcg 4 3 (1.0/3.0) (1.0/3.0) (1.0/3.0)
        let randoGen = createMockRando [0; 1; 0; 1; 0; 1] [0.2; 0.4; 0.6]
        let modelRs = model.MakeSorterModel randoGen 0
        modelRs.Perm_Rss.Length |> should equal 3

    [<Fact>]
    let ``MakeSorterModel creates permutations with correct width`` () =
        let model = createModelRsGen rngType.Lcg 4 2 (1.0/3.0) (1.0/3.0) (1.0/3.0)
        let randoGen = createMockRando [0; 1; 0; 1] [0.2; 0.4]
        let modelRs = model.MakeSorterModel randoGen 0
        modelRs.Perm_Rss |> Array.iter (fun perm -> perm.Order |> should equal (UMX.tag<sortingWidth> 4))

    [<Fact>]
    let ``MakeSorterModel throws on odd width`` () =
        let model = createModelRsGen rngType.Lcg 3 2 (1.0/3.0) (1.0/3.0) (1.0/3.0)
        let randoGen = createMockRando [0; 1] [0.2; 0.4]
        (fun () -> model.MakeSorterModel randoGen 0 |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``MakeSorterModel generates self-inverse permutations`` () =
        let model = createModelRsGen rngType.Lcg 4 2 0.0 0.0 1.0
        let randoGen = createMockRando [0; 1; 0; 0; 1] [0.2; 0.4]
        let modelRs = model.MakeSorterModel randoGen 0
        modelRs.Perm_Rss
        |> Array.iter (fun perm ->
            Perm_Si.isReflectionSymmetric perm.Perm_Si |> should equal true)

    [<Fact>]
    let ``MakeSorterModel generates unique IDs`` () =
        let model = createModelRsGen rngType.Lcg 4 2 (1.0/3.0) (1.0/3.0) (1.0/3.0)
        let randoGen = createMockRando [0; 1; 0; 1] [0.2; 0.4]
        let modelRs1 = model.MakeSorterModel randoGen 0
        let modelRs2 = model.MakeSorterModel randoGen 1
        modelRs1.Id |> should not' (equal modelRs2.Id)
