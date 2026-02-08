namespace GeneSort.Model.Test.Sorter.Ce

open System
open Xunit
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorter.Ce
open FsUnit.Xunit
open GeneSort.Model.Sorter


type MsceRandMutateTests() =
    let epsilon = 1e-10 // Tolerance for floating-point comparisons

    // Helper function to create a mock random number generator
    let createMockRando (indices: int list) (floats: float list) =
        fun _ _ -> new MockRando(floats, indices) :> IRando

    let mockRngFactory floatValue indexValue _ _ = MockRando(floatValue, indexValue) :> IRando
    let mockCeCode = 42 // Placeholder valid ceCode
    let newCeCode = 19 // Placeholder for mutated/inserted ceCode
    let sortingWidth = 10 |> UMX.tag<sortingWidth>
    let baseRates = IndelRates.create (0.2, 0.2, 0.2)
    let msceId = Guid.NewGuid() |> UMX.tag<sorterModelID>

    [<Fact>]
    let ``mutate applies Mutation mode correctly`` () =
        let arrayRates = IndelRatesArray.create [| IndelRates.create (1.0, 0.0, 0.0) |] // Always Mutation
        let msce = Msce.create msceId sortingWidth [| mockCeCode |]
        let msceMutate = MsceRandMutate.create (rngType.Lcg) arrayRates false msce
        
        let mockRando = createMockRando [newCeCode] [0.9]
        let result = msceMutate.MakeSorterModel mockRando 0
        result.CeCodes |> should equal [| newCeCode |]
        result.SortingWidth |> should equal sortingWidth
        result.CeLength |> should equal (1 |> UMX.tag<ceLength>)

    [<Fact>]
    let ``mutate applies Insertion mode and trims to ceCount`` () =
        let arrayRates = IndelRatesArray.create [| IndelRates.create (0.0, 1.0, 0.0); IndelRates.create (0.0, 1.0, 0.0) |] // Insertion; Insertion
        let msce = Msce.create msceId sortingWidth [| mockCeCode; mockCeCode |]
        let msceMutate = MsceRandMutate.create (rngType.Lcg) arrayRates false msce
        
        let mockRando = createMockRando [newCeCode; newCeCode] [0.9; 0.9]
        let result = msceMutate.MakeSorterModel mockRando 0
        result.CeCodes |> should equal [| newCeCode; mockCeCode; |] // Two insertions, trimmed to ceCount
        result.CeLength |> should equal (2 |> UMX.tag<ceLength>)
        result.SortingWidth |> should equal sortingWidth

    [<Fact>]
    let ``mutate applies Deletion mode and appends insertion`` () =
        let arrayRates = IndelRatesArray.create [| IndelRates.create (0.0, 0.0, 1.0); IndelRates.create (0.0, 1.0, 0.0) |] // Deletion; Insertion
        let msce = Msce.create msceId sortingWidth [| mockCeCode; mockCeCode |]
        let msceMutate = MsceRandMutate.create (rngType.Lcg) arrayRates false msce
        
        let mockRando = createMockRando [newCeCode] [0.9; 0.9]
        let result = msceMutate.MakeSorterModel mockRando 0
        result.CeCodes |> should equal [| newCeCode; mockCeCode |] // Deletion + Insertion
        result.CeLength |> should equal (2 |> UMX.tag<ceLength>)
        result.SortingWidth |> should equal sortingWidth

    [<Fact>]
    let ``mutate applies NoAction mode correctly`` () =
        let arrayRates = IndelRatesArray.create [| IndelRates.create (0.0, 0.0, 0.0) |] // Always NoAction
        let msce = Msce.create msceId sortingWidth [| mockCeCode |]
        let msceMutate = MsceRandMutate.create (rngType.Lcg) arrayRates false msce
        
        let mockRando = createMockRando [0] [0.9]
        let result = msceMutate.MakeSorterModel mockRando 0
        result.CeCodes |> should equal [| mockCeCode |] // Unchanged
        result.CeLength |> should equal (1 |> UMX.tag<ceLength>)
        result.SortingWidth |> should equal sortingWidth



