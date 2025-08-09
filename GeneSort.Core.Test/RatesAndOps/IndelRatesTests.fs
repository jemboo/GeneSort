﻿namespace GeneSort.Core.Test

open System
open Xunit
open FsUnit.Xunit
open GeneSort.Core


type IndelRatesTests() =
    let epsilon = 1e-10 // Tolerance for floating-point comparisons

    [<Fact>]
    let ``create should succeed with valid rates`` () =
        let rates = IndelRates.create (0.2, 0.3, 0.4)
        rates.MutationRate |> should (equalWithin epsilon) 0.2
        rates.InsertionRate |> should (equalWithin epsilon) 0.3
        rates.DeletionRate |> should (equalWithin epsilon) 0.4
        rates.NoActionRate |> should (equalWithin epsilon) 0.1

    [<Fact>]
    let ``create should fail when mutationRate is negative`` () =
        (fun () -> IndelRates.create (-0.1, 0.3, 0.4) |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``create should fail when insertionRate is negative`` () =
        (fun () -> IndelRates.create (0.2, -0.1, 0.4) |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``create should fail when deletionRate is negative`` () =
        (fun () -> IndelRates.create (0.2, 0.3, -0.1) |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``create should fail when rates sum exceeds 1.0`` () =
        (fun () -> IndelRates.create (0.5, 0.4, 0.2) |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``create should succeed when rates sum to 1.0`` () =
        let rates = IndelRates.create (0.3, 0.3, 0.4)
        rates.NoActionRate |> should (equalWithin epsilon) 0.0

    [<Fact>]
    let ``PickMode should return Mutation when random value is below mutationThresh`` () =
        let rates = IndelRates.create (0.3, 0.2, 0.1)
        let floatPicker () = 0.15 // Within mutation range (0.0 to 0.3)
        rates.PickMode floatPicker |> should equal IndelMode.Mutation

    [<Fact>]
    let ``PickMode should return Insertion when random value is between mutationThresh and insertionThresh`` () =
        let rates = IndelRates.create (0.3, 0.2, 0.1)
        let floatPicker () = 0.45 // Within insertion range (0.3 to 0.5)
        rates.PickMode floatPicker |> should equal IndelMode.Insertion

    [<Fact>]
    let ``PickMode should return Deletion when random value is between insertionThresh and deletionThresh`` () =
        let rates = IndelRates.create (0.3, 0.2, 0.1)
        let floatPicker () = 0.55 // Within deletion range (0.5 to 0.6)
        rates.PickMode floatPicker |> should equal IndelMode.Deletion

    [<Fact>]
    let ``PickMode should return NoAction when random value is above deletionThresh`` () =
        let rates = IndelRates.create (0.3, 0.2, 0.1)
        let floatPicker () = 0.75 // Above deletion range (0.6 to 1.0)
        rates.PickMode floatPicker |> should equal IndelMode.NoAction

    [<Fact>]
    let ``Equals should return true for identical IndelRates`` () =
        let rates1 = IndelRates.create (0.2, 0.3, 0.4)
        let rates2 = IndelRates.create (0.2, 0.3, 0.4)
        rates1.Equals(rates2) |> should be True
        (rates1 :> IEquatable<IndelRates>).Equals(rates2) |> should be True

    [<Fact>]
    let ``Equals should return false for different IndelRates`` () =
        let rates1 = IndelRates.create (0.2, 0.3, 0.4)
        let rates2 = IndelRates.create (0.3, 0.3, 0.3)
        rates1.Equals(rates2) |> should be False

    [<Fact>]
    let ``GetHashCode should be equal for identical IndelRates`` () =
        let rates1 = IndelRates.create (0.2, 0.3, 0.4)
        let rates2 = IndelRates.create (0.2, 0.3, 0.4)
        rates1.GetHashCode() |> should equal (rates2.GetHashCode())