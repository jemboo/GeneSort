namespace GeneSort.Core.Test

open System
open Xunit
open FsUnit.Xunit
open GeneSort.Core
open GeneSort.Core.Mp.RatesAndOps

type IndelRatesTests() =
    let epsilon = 1e-10 // Tolerance for floating-point comparisons

    [<Fact>]
    let ``IndelRatesDto toIndelRates should correctly map to domain type`` () =
        let dto = { MutationThresh = 0.2; InsertionThresh = 0.5; DeletionThresh = 0.9 }
        let rates = IndelRatesDto.toIndelRates dto
        rates.MutationRate |> should (equalWithin epsilon) 0.2
        rates.InsertionRate |> should (equalWithin epsilon) 0.3 // 0.5 - 0.2
        rates.DeletionRate |> should (equalWithin epsilon) 0.4 // 0.9 - 0.5
        rates.NoActionRate |> should (equalWithin epsilon) 0.1 // 1.0 - 0.9

    [<Fact>]
    let ``IndelRatesDto fromIndelRates should correctly map to Dto`` () =
        let rates = IndelRates.create (0.2, 0.3, 0.4)
        let dto = IndelRatesDto.fromIndelRates rates
        dto.MutationThresh |> should (equalWithin epsilon) 0.2
        dto.InsertionThresh |> should (equalWithin epsilon) 0.5 // 0.2 + 0.3
        dto.DeletionThresh |> should (equalWithin epsilon) 0.9 // 0.2 + 0.3 + 0.4

    [<Fact>]
    let ``Roundtrip conversion through IndelRatesDto should preserve rates`` () =
        let original = IndelRates.create (0.2, 0.3, 0.4)
        let dto = IndelRatesDto.fromIndelRates original
        let converted = IndelRatesDto.toIndelRates dto
        converted.MutationRate |> should (equalWithin epsilon) original.MutationRate
        converted.InsertionRate |> should (equalWithin epsilon) original.InsertionRate
        converted.DeletionRate |> should (equalWithin epsilon) original.DeletionRate
        converted.NoActionRate |> should (equalWithin epsilon) original.NoActionRate