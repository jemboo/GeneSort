namespace GeneSort.Core.Test

open System
open Xunit
open FsUnit.Xunit
open GeneSort.Core
open GeneSort.Core.Mp.RatesAndOps

module IndelRatesArrayTests =
    let epsilon = 1e-10 // Tolerance for floating-point comparisons

    [<Fact>]
    let ``create should succeed with valid IndelRates array`` () =
        let rates = [| indelRates.create (0.2, 0.3, 0.4); indelRates.create (0.3, 0.3, 0.3) |]
        let indelRatesArray = indelRatesArray.create rates
        indelRatesArray.Length |> should equal 2
        indelRatesArray.Item(0).MutationRate |> should (equalWithin epsilon) 0.2
        indelRatesArray.RatesArray |> should equal rates

    [<Fact>]
    let ``create should fail with empty array`` () =
        (fun () -> indelRatesArray.create [||] |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``createLinearVariation should interpolate rates correctly`` () =
        let startRates = indelRates.create (0.2, 0.3, 0.4)
        let endRates = indelRates.create (0.4, 0.2, 0.3)
        let indelRatesArray = IndelRatesArray.createLinearVariation 3 startRates endRates
        indelRatesArray.Length |> should equal 3
        indelRatesArray.Item(0).MutationRate |> should (equalWithin epsilon) 0.2
        indelRatesArray.Item(0).InsertionRate |> should (equalWithin epsilon) 0.3
        indelRatesArray.Item(0).DeletionRate |> should (equalWithin epsilon) 0.4
        indelRatesArray.Item(2).MutationRate |> should (equalWithin epsilon) 0.4
        indelRatesArray.Item(2).InsertionRate |> should (equalWithin epsilon) 0.2
        indelRatesArray.Item(2).DeletionRate |> should (equalWithin epsilon) 0.3
        indelRatesArray.Item(1).MutationRate |> should (equalWithin epsilon) 0.3 // Midpoint
        indelRatesArray.Item(1).NoActionRate |> should (equalWithin epsilon) 0.1 // 1.0 - (0.3 + 0.25 + 0.35)

    [<Fact>]
    let ``createLinearVariation should fail with non-positive length`` () =
        let rates = indelRates.create (0.2, 0.3, 0.4)
        (fun () -> IndelRatesArray.createLinearVariation 0 rates rates |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``createSinusoidalVariation should apply sinusoidal variation`` () =
        let baseRates = indelRates.create (0.3, 0.3, 0.3)
        let amplitudes = indelRates.create (0.1, 0.1, 0.1)
        let indelRatesArray = IndelRatesArray.createSinusoidalVariation 5 baseRates amplitudes 1.0
        indelRatesArray.Length |> should equal 5
        // Check approximate values at index 0 (sin(0) = 0)
        indelRatesArray.Item(0).MutationRate |> should (equalWithin epsilon) 0.3
        // Check approximate values at index 2 (near peak, sin(π) = 0)
        indelRatesArray.Item(2).MutationRate |> should be (lessThanOrEqualTo 0.4)
        indelRatesArray.Item(2).MutationRate |> should be (greaterThanOrEqualTo 0.2)

    [<Fact>]
    let ``createSinusoidalVariation should fail with non-positive length`` () =
        let rates = indelRates.create (0.2, 0.3, 0.4)
        let amplitudes = indelRates.create (0.1, 0.1, 0.1)
        (fun () -> IndelRatesArray.createSinusoidalVariation 0 rates amplitudes 1.0 |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``createGaussianHotSpot should create peak at specified index`` () =
        let baseRates = indelRates.create (0.2, 0.2, 0.2)
        let hotSpotRates = indelRates.create (0.5, 0.3, 0.1)
        let indelRatesArray = IndelRatesArray.createGaussianHotSpot 5 baseRates 2 hotSpotRates 1.0
        indelRatesArray.Length |> should equal 5
        // Peak at index 2
        indelRatesArray.Item(2).MutationRate |> should (equalWithin epsilon) 0.5
        indelRatesArray.Item(2).InsertionRate |> should (equalWithin epsilon) 0.3
        indelRatesArray.Item(2).DeletionRate |> should (equalWithin epsilon) 0.1
        // Away from peak (index 0), closer to base rates
        indelRatesArray.Item(0).MutationRate |> should be (greaterThan 0.2)
        indelRatesArray.Item(0).MutationRate |> should be (lessThan 0.5)

    [<Fact>]
    let ``createGaussianHotSpot should fail with non-positive length`` () =
        let baseRates = indelRates.create (0.2, 0.2, 0.2)
        let hotSpotRates = indelRates.create (0.5, 0.3, 0.1)
        (fun () -> IndelRatesArray.createGaussianHotSpot 0 baseRates 0 hotSpotRates 1.0 |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``createGaussianHotSpot should fail with invalid hotSpotIndex`` () =
        let baseRates = indelRates.create (0.2, 0.2, 0.2)
        let hotSpotRates = indelRates.create (0.5, 0.3, 0.1)
        (fun () -> IndelRatesArray.createGaussianHotSpot 5 baseRates 5 hotSpotRates 1.0 |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``createGaussianHotSpot should fail with non-positive sigma`` () =
        let baseRates = indelRates.create (0.2, 0.2, 0.2)
        let hotSpotRates = indelRates.create (0.5, 0.3, 0.1)
        (fun () -> IndelRatesArray.createGaussianHotSpot 5 baseRates 2 hotSpotRates 0.0 |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``createStepHotSpot should apply hot spot rates in specified range`` () =
        let baseRates = indelRates.create (0.2, 0.2, 0.2)
        let hotSpotRates = indelRates.create (0.5, 0.3, 0.1)
        let indelRatesArray = IndelRatesArray.createStepHotSpot 5 baseRates 1 3 hotSpotRates
        indelRatesArray.Length |> should equal 5
        // Outside hot spot (index 0)
        indelRatesArray.Item(0).MutationRate |> should (equalWithin epsilon) 0.2
        indelRatesArray.Item(0).InsertionRate |> should (equalWithin epsilon) 0.2
        indelRatesArray.Item(0).DeletionRate |> should (equalWithin epsilon) 0.2
        // Inside hot spot (index 2)
        indelRatesArray.Item(2).MutationRate |> should (equalWithin epsilon) 0.5
        indelRatesArray.Item(2).InsertionRate |> should (equalWithin epsilon) 0.3
        indelRatesArray.Item(2).DeletionRate |> should (equalWithin epsilon) 0.1
        // Outside hot spot (index 4)
        indelRatesArray.Item(4).MutationRate |> should (equalWithin epsilon) 0.2

    [<Fact>]
    let ``createStepHotSpot should fail with non-positive length`` () =
        let baseRates = indelRates.create (0.2, 0.2, 0.2)
        let hotSpotRates = indelRates.create (0.5, 0.3, 0.1)
        (fun () -> IndelRatesArray.createStepHotSpot 0 baseRates 0 1 hotSpotRates |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``createStepHotSpot should fail with invalid range`` () =
        let baseRates = indelRates.create (0.2, 0.2, 0.2)
        let hotSpotRates = indelRates.create (0.5, 0.3, 0.1)
        (fun () -> IndelRatesArray.createStepHotSpot 5 baseRates 3 2 hotSpotRates |> ignore)
        |> should throw typeof<System.Exception>
        (fun () -> IndelRatesArray.createStepHotSpot 5 baseRates 0 5 hotSpotRates |> ignore)
        |> should throw typeof<System.Exception>