namespace GeneSort.Core.Test

open Xunit
open FsUnit.Xunit
open GeneSort.Core


type IndelRatesArrayTests() =
    let epsilon = 1e-10 // Tolerance for floating-point comparisons

    let createRates (m, i, d) = IndelRates.create (m, i, d)
    let indelRatesArray rates = IndelRatesArray.create (Array.create 1 rates)
    let inserter () = 0
    let mutator x = x + 1
    let arrayToMutate = [| 1 |]

    [<Fact>]
    let ``mutate should fail when array and rates lengths differ`` () =
        let rates = IndelRatesArray.create [| createRates (0.2, 0.3, 0.4); createRates (0.2, 0.3, 0.4) |]
        let array = [| 1 |]
        (fun () -> IndelRatesArray.mutate rates inserter mutator (fun () -> 0.0) array |> ignore)
        |> should throw typeof<System.Exception>

    [<Fact>]
    let ``mutate should apply Mutation mode correctly`` () =
        let rates = indelRatesArray (createRates (1.0, 0.0, 0.0)) // Always Mutation
        let result = IndelRatesArray.mutate rates inserter mutator (fun () -> 0.0) arrayToMutate
        result |> should equal [| 2 |] // mutator 1 = 2

    [<Fact>]
    let ``mutate should apply Insertion mode correctly`` () =
        let rates = indelRatesArray (createRates (0.0, 1.0, 0.0)) // Always Insertion
        let result = IndelRatesArray.mutate rates inserter mutator (fun () -> 0.0) arrayToMutate
        result |> should equal [| 0 |] // inserter () = 0

    [<Fact>]
    let ``mutate should apply Deletion mode and append insertion to maintain length`` () =
        let rates = indelRatesArray (createRates (0.0, 0.0, 1.0)) // Always Deletion
        let result = IndelRatesArray.mutate rates inserter mutator (fun () -> 0.0) arrayToMutate
        result |> should equal [| 0 |] // Deletion, then inserter () to maintain length

    [<Fact>]
    let ``mutate should apply NoAction mode correctly`` () =
        let rates = indelRatesArray (createRates (0.0, 0.0, 0.0)) // Always NoAction
        let result = IndelRatesArray.mutate rates inserter mutator (fun () -> 0.9) arrayToMutate
        result |> should equal [| 1 |] // Unchanged

    [<Fact>]
    let ``mutate should maintain length with multiple deletions`` () =
        let rates = IndelRatesArray.create [| createRates (0.0, 0.0, 1.0); createRates (0.0, 0.0, 1.0) |]
        let array = [| 1; 2 |]
        let result = IndelRatesArray.mutate rates inserter mutator (fun () -> 0.0) array
        result |> should equal [| 0; 0 |] // Two deletions, two insertions appended

    [<Fact>]
    let ``mutate should trim excess insertions to maintain length`` () =
        let rates = IndelRatesArray.create [| createRates (0.0, 1.0, 0.0); createRates (0.0, 1.0, 0.0) |] // Always Insertion
        let array = [| 1; 2 |]
        let result = IndelRatesArray.mutate rates inserter mutator (fun () -> 0.0) array
        result |> should haveLength 2 // Two insertions, trimmed to length 2
        result |> should equal [| 0; 1 |] // Both are inserter () calls

    [<Fact>]
    let ``mutate should maintain length with mixed operations`` () =
        let rates = IndelRatesArray.create [| createRates (1.0, 0.0, 0.0); createRates (0.0, 0.0, 1.0) |]
        let array = [| 1; 2 |]
        let floatPicker () = 0.0 // Triggers Mutation, then Deletion
        let result = IndelRatesArray.mutate rates inserter mutator floatPicker array
        result |> should equal [| 2; 0 |] // Mutation (1 -> 2), Deletion + Insertion (0)

    [<Fact>]
    let ``mutate should handle single-element array correctly`` () =
        let rates = indelRatesArray (createRates (0.0, 0.0, 1.0)) // Deletion
        let result = IndelRatesArray.mutate rates inserter mutator (fun () -> 0.0) arrayToMutate
        result |> should equal [| 0 |] // Deletion + Insertion to maintain length

